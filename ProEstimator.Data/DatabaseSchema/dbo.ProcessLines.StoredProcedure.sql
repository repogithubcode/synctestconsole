USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Ezra
-- Create date: 5/13/2021
-- Description: A copy of the GetProcessedLines function with table valued functions and table variables removed.
-- =============================================
CREATE PROCEDURE [dbo].[ProcessLines]
	@AdminInfoID		int, 
	@SupplementVersion	int = 0, 
	@ImageDirectory		varchar(500) = '', 
	@ForPreview			bit = 0,			-- True when putting together the line item preview, which doesn't calculate markup for prices
	@PrintPnLNotes		INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

CREATE TABLE #ProcessedLines
(
	  LineItemID	int
	, ForSummary	bit
	, PartSourceModified	bit
	, ModifiedSupplementVersion	int
	, ModifiedID	int
	, Panel	varchar(50)
	, Added	bit
	, Removed	bit
	, BettermentType	varchar(20)
	, BettermentValue	float
	, BettermentAmount	float
	, BettermentNote	varchar(50)
	, Sublet	bit
	, Action	varchar(20)
	, OperationDescription	varchar(100)
	, Description	varchar(200)
	, PartNumber	varchar(50)
	, Price	money
	, Quantity	int
	, PartSource	varchar(20)
	, RemovedPrice	money
	, RemovedQuantity	int
	, RemovedPartSource	varchar(20)
	, PartSourceImage	varchar(500)
	, LaborSummaryPaintPanel	varchar(200)
	, LaborTotalPaintPanel	float
	, LaborSummaryPaint	varchar(200)
	, LaborTotalPaint	float
	, LaborTotalPaintDollars float
	, LaborTotalClearcoat		real
	, ClearcoatIncluded			bit
	, ClearcoatSuppliesIncluded	bit
	, LaborSummaryExtra	varchar(200)
	, LaborTotalExtra	float
	, LaborTotalExtraOverlapped float
	, ExtraLaborType	varchar(20)
	, ExtraLaborIncludeIn	varchar(20)
	, LaborExtraRemovedType		varchar(50) 
	, LaborExtraRemovedIncludeInType		varchar(50) 
	, LaborExtraRemovedHours	float 
	, LaborExtraRemovedTotal	float 
	, LaborIncluded	bit
	, OverlapSupplement int
	, OtherCharges	float
	, OtherChargesLaborType	int
	, RemovedOtherCharges	float
	, RemovedOtherChargesLaborType	int
	, DeductionsMessage	varchar(100)
	, AdjacentMessage varchar(20)
	, Notes	varchar(5000)
	, LineNumber	int
	, SupplementVersion	int
	, DeletedBySupplement	int
	, SupplementDisplay	int
	, StepSupp	varchar(500)
	, Sorter	int
	, GroupNumber	int
	, OversizedDescription	varchar(200)
	, OversizedPrice	money
	, ModifierDescription	varchar(200)
	, ModifierPrice	money
	, LaborSummaryPaintPanelPrint	varchar(200)
	, LaborSummaryPaintPrint	varchar(200)
	, LaborSummaryExtraPrint	varchar(200)
	, IsPartsQuantity				BIT
	, IsLaborQuantity				BIT
	, IsPaintQuantity				BIT
	, PartsQuantity					REAL
	, LaborQuantity					REAL
	, PaintQuantity					REAL
    , PaintMaterialsBetterment      REAL
    , LaborBetterment				REAL
    , PaintLaborBetterment			REAL
)

BEGIN
   --insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'PL')
	DECLARE @IsImported BIT = (SELECT ISNULL(IsImported, 0) FROM AdminInfo with(nolock) WHERE ID = @AdminInfoID)
	DECLARE @Service_BarCode VarCHar(10) = Mitchell3.dbo.GetServiceBarcode(@AdminInfoID) 

	-- Set up variables used for controlling overlap calculations.  Older estimates and imported estimates use different calculations
	DECLARE @OverlapRoundingFirst INT = 7163683		-- Set this to the first estimate that should use the 0.001 instead of 0 comparison for including labor
	DECLARE @OtherLaborTypesIncludeFirst INT = 0	-- Before only Body labor was applying the Included overlap logic, but Mechanical and Structure should be too.  Estimates after this ID will include Mech and Structure as well

	DECLARE @UseNewOverlapFormula BIT = 0

	IF @AdminInfoID > @OverlapRoundingFirst		-- Set this to the first estimate that should use the new overlap formula
		SET @UseNewOverlapFormula = 1

	IF @IsImported = 1
		SET @UseNewOverlapFormula =0

	DECLARE @EstimationDataID INT = (SELECT ID FROM EstimationData WHERE AdminInfoID = @AdminInfoID) 

--SET @UseNewOverlapFormula = 0		-- TEMP - Ignore new formula

	-------------------------------------------------------------------------------------------------------------------------------------
	-- Get a temp table with summary data for the line items
	-------------------------------------------------------------------------------------------------------------------------------------
	CREATE TABLE #EstimateLineItemSummary
	(
		  LineItemID	int
		, Service_BarCode	varchar(20)
		, Category		varchar(200)
		, SubCategory	varchar(200)
		, barcode		varchar(50)
		, GroupNumber	int
		, nHeader		int
		, nSection		int
		, Step			varchar(200)
	)

	INSERT INTO #EstimateLineItemSummary 
	SELECT     
			EstimationLineItems.id AS LineItemID,     
			Detail.Service_BarCode,     
			Category.Category,     
			SubCategory.Subcategory,     
			Detail.barcode,     
			ISNULL   
			(   
				Category.nheader * 256 + SubCategory.nsection   
				, CASE WHEN ISNULL(ParentLineItems.SectionID, EstimationLineItems.SectionID) = -1 THEN 100000 ELSE ISNULL(ISNULL(ParentLineItems.SectionID, EstimationLineItems.SectionID), 100000) END    
			) 'GroupNumber',   
			SubCategory.nHeader,     
			SubCategory.nSection,     
			CASE 	    
				WHEN Category.Category IS NULL AND SubCategory.Subcategory IS NOT NULL THEN SubCategory.Subcategory     
 				WHEN Category.Category IS NOT NULL AND SubCategory.Subcategory IS NULL THEN Category.Category     
				WHEN Category.Category = SubCategory.Subcategory THEN Category.Category      
				ELSE Category.Category + ' \ ' + SubCategory.Subcategory     
			END 'Step'     
		FROM AdminInfo  with(nolock)
		INNER JOIN EstimationData  with(nolock) ON EstimationData.AdminInfoID = AdminInfo.ID   
		INNER JOIN EstimationLineItems with(nolock) ON EstimationLineItems.EstimationDataID  = EstimationData.ID   
		LEFT JOIN EstimationLineItems ParentLineItems ON EstimationLineItems.ParentLineID = ParentLineItems.ID
		LEFT OUTER JOIN Mitchell3.dbo.Detail with(nolock)  ON Detail.Service_BarCode = @Service_BarCode AND Detail.barcode = EstimationLineItems.Barcode    
			-- Limit to 1 join, sometimes there are multiple detail records with same barcodes
			AND Detail.npart = (SELECT MIN(npart) FROM Mitchell3.dbo.Detail WHERE Detail.Service_BarCode = @Service_BarCode AND Detail.barcode = EstimationLineItems.Barcode)
		LEFT OUTER JOIN Mitchell3.dbo.Part with(nolock) ON Part.Service_BarCode = Detail.Service_BarCode      
			 AND Part.nheader = Detail.nHeader     
			 AND Part.nsection = Detail.nsection 
			 AND Part.nPart = Detail.nPart    
		LEFT OUTER JOIN Mitchell3.dbo.SubCategory  ON SubCategory.Service_BarCode = Part.Service_BarCode AND SubCategory.nheader = Part.nHeader AND SubCategory.nsection = Part.nsection  
		LEFT OUTER JOIN Mitchell3.dbo.Category with(nolock) ON Category.Service_BarCode = SubCategory.Service_BarCode AND Category.nheader = SubCategory.nHeader 
		WHERE AdminInfo.ID = @AdminInfoID  
	-------------------------------------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------------------------

	-------------------------------------------------------------------------------------------------------------------------------------
	-- Get a temp table with the line items for this estimate
	-------------------------------------------------------------------------------------------------------------------------------------
	CREATE TABLE #LineItems
	(
		[id] [int] NOT NULL,
		[EstimationDataID] [int] NOT NULL,
		[StepID] [int] NULL,
		[PartNumber] [varchar](50) NULL,
		[PartSource] [varchar](10) NULL,
		[ActionCode] [varchar](20) NULL,
		[Description] [varchar](80) NULL,
		[Price] [money] NULL,
		[Qty] [int] NULL,
		[LaborTime] [real] NULL,
		[PaintTime] [real] NULL,
		[Other] [real] NULL,
		[ImageID] [int] NULL,
		[ActionDescription] [varchar](80) NULL,
		[PartOfOverhaul] [bit] NULL,
		[PartSourceVendorID] [int] NULL,
		[BettermentParts] [bit] NULL,
		[SubletPartsFlag] [bit] NULL,
		[BettermentMaterials] [bit] NULL,
		[SubletOperationFlag] [bit] NULL,
		[SupplementVersion] [tinyint] NULL,
		[LineNumber] [int] NULL,
		[UniqueSequenceNumber] [int] NULL,
		[ModifiesID] [int] NULL,
		[ACDCode] [char](1) NULL,
		[CustomerPrice] [real] NULL,
		[AutomaticCharge] [bit] NULL,
		[SourcePartNumber] [varchar](25) NULL,
		[SectionID] [int] NULL,
		[VehiclePosition] [varchar](5) NULL,
		[Barcode] [varchar](10) NULL,
		[dbPrice] [money] NULL,
		[AutoAdd] [bit] NULL,
		[Suppress] [bit] NULL,
		[AutoAddBarcodeParent] [varchar](10) NULL,
		[Date_Entered] [datetime] NOT NULL,
		[BettermentType] [char](10) NULL,
		[BettermentValue] [float] NULL,
		IsPartsQuantity BIT NULL,
		IsLaborQuantity BIT NULL,
		IsPaintQuantity BIT NULL,
		PresetShellID INT NULL,
		ParentLineID INT NULL
	)

	INSERT INTO #LineItems
	SELECT EstimationLineItems.*
	FROM EstimationData with(nolock)
	JOIN EstimationLineItems with(nolock) ON EstimationLineItems.EstimationDataID = EstimationData.id
	WHERE EstimationData.AdminInfoID = @AdminInfoID 
	-------------------------------------------------------------------------------------------------------------------------------------
	
	-------------------------------------------------------------------------------------------------------------------------------------
	 -- Get a temp table with the labor lines for this estimate
	 -------------------------------------------------------------------------------------------------------------------------------------
	 CREATE TABLE #LaborItems
	 (
		[id] [int] NOT NULL,
		[EstimationLineItemsID] [int] NOT NULL,
		[LaborType] [tinyint] NOT NULL,
		[LaborSubType] [smallint] NULL,
		[LaborTime] [real] NULL,
		[LaborCost] [money] NULL,
		[BettermentFlag] [bit] NULL,
		[SubletFlag] [bit] NULL,
		[UniqueSequenceNumber] [int] NULL,
		[ModifiesID] [int] NULL,
		[AdjacentDeduction] [tinyint] NULL,
		[MajorPanel] [bit] NULL,
		[BettermentPercentage] [real] NULL,
		[dbLaborTime] [real] NULL,
		[AdjacentDeductionLock] [bit] NULL,
		[barcode] [varchar](10) NULL,
		[Lock] [bit] NULL,
		[include] [bit] NULL,
		[WebEstID] [int] NULL
	 )

	INSERT INTO #LaborItems
	SELECT EstimationLineLabor.*
	FROM #LineItems LineItems
	JOIN EstimationLineLabor with(nolock) ON EstimationLineLabor.EstimationLineItemsID = LineItems.id
	-------------------------------------------------------------------------------------------------------------------------------------

	-------------------------------------------------------------------------------------------------------------------------------------
	-- Get the labor summary
	-------------------------------------------------------------------------------------------------------------------------------------
	CREATE TABLE #LaborSummary
(  
	 EstimationLineItemsID		int  
	,LaborSummaryPaintPanel		varchar(200)  
	,LaborTotalPaintPanel		real  
	,LaborSummaryPaint			varchar(200)  
	,LaborTotalPaint			real  
	,LaborTotalPaintDollars		real 
	,LaborTotalClearcoat		real 
	,ClearcoatLaborIncluded		bit 
	,ClearcoatSuppliesIncluded	bit 
	,LaborSummaryExtra			varchar(200)  
	,LaborTotalExtra			real  
	,LaborExtraType				varchar(50) 
	,LaborExtraIncludedInType	varchar(50) 
	,LaborExtraRemovedType		varchar(50) 
	,LaborExtraRemovedIncludedInType		varchar(50)  
	,LaborExtraRemovedHours		real  
	,LaborExtraRemovedTotal		real
	,LaborSummaryPaintPanelPrint	varchar(200)
	,LaborSummaryPaintLaborPrint	varchar(200)
	,LaborSummaryExtraPrint			varchar(200)
	,IsPartsQuantity				BIT
	,IsLaborQuantity				BIT
	,IsPaintQuantity				BIT
	,PartsQuantity					REAL
	,LaborQuantity					REAL
	,PaintQuantity					REAL
	,PaintMaterialsBetterment		REAL
	,LaborBetterment				REAL
	,PaintLaborBetterment			REAL
)   
 
 
	DECLARE @CustomerProfilesID INT =     
	(    
		SELECT AdminInfo.CustomerProfilesID    
		FROM EstimationData    
		JOIN AdminInfo ON EstimationData.AdminInfoID = AdminInfo.ID    
		WHERE EstimationData.ID = @EstimationDataID     
	)    
 
	DECLARE @ClearCoatInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 21 
		) > 0 THEN 1 ELSE 0 END 
	) 
	DECLARE @ClearCoatSuppliesInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 22 
		) > 0 THEN 1 ELSE 0 END 
	) 
   
	DECLARE @DollarOrHours CHAR =    
		CASE WHEN @ForPreview = 1 THEN 'h' 
		ELSE 
			CASE WHEN (SELECT TOP 1 ISNULL(Dollars, 0) FROM CustomerProfilePrint WHERE CustomerProfilesID = @CustomerProfilesID) = 1 THEN 'd' ELSE 'h' END    
		END   
   
	CREATE TABLE #LaborLines  
	(  
		  EstimationLineItemsID		int  
		, ModifiesID				int  
		, SupplementVersion			int  
		, LaborTypeID				tinyint  
		, LaborTypeName				varchar(50)  
		, LaborTime					real  
		, DeductionAmount			real  
		, Rate						real  
		, LaborCost					money  
		, Sub						varchar(10)  
	)  
  
	INSERT INTO #LaborLines     
	(  
		  EstimationLineItemsID	  
		, ModifiesID  
		, SupplementVersion	  
		, LaborTypeID  
		, LaborTypeName	  
		, LaborTime	  
		, DeductionAmount  
		, Rate  
		, LaborCost  
		, Sub  
	)  
	SELECT  
		EstimationLineItems.ID,    
		EstimationLineItems.ModifiesID,    
		ISNULL(EstimationLineItems.SupplementVersion, 0) AS SupplementVersion,    
 
		-- The "paint types" are all counted as one when comparing between supplements, on other words changing between paint types doesn't remove the original 
		-- paint type and add the new, only change the difference in time for any paint type. 
		CASE WHEN (EstimationLineLabor.LaborType IN (18, 19, 29)) THEN 16 ELSE EstimationLineLabor.LaborType END AS LaborTypeID,    
 
		ISNULL(LaborTypes.LaborType, '') AS LaborTypeName,    
		ISNULL(EstimationLineLabor.LaborTime, 0) AS LaborTime,    
		CASE     
			WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction     
			WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction     
			ELSE 0     
		END AS DeductionAmount,    
		ISNULL(ISNULL(CustomerProfileRates2.Rate, CustomerProfileRates.rate),0) AS Rate,    
		ISNULL(EstimationLineLabor.LaborCost, 0) AS LaborCost,    
		CASE WHEN EstimationLineLabor.SubletFlag <> 0 THEN '(Sub)' ELSE '' END AS Sub    
	FROM EstimationLineItems    
	LEFT JOIN EstimationLineLabor ON EstimationLineLabor.EstimationLineItemsID = EstimationLineItems.ID    
	LEFT JOIN LaborTypes ON LaborTypes.ID = EstimationLineLabor.LaborType     
    
	LEFT JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates.RateType = LaborTypes.RateTypesID    
	LEFT JOIN CustomerProfileRates CustomerProfileRates2 ON CustomerProfileRates2.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates2.RateType = CustomerProfileRates.IncludeIn     
	LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = @CustomerProfilesID     
    
	WHERE EstimationLineItems.EstimationDataID = @EstimationDataID    
		AND (EstimationLineLabor.LaborType NOT IN (20, 21, 22, 26, 27, 28)  OR EstimationLineLabor.Include = 1)    
		AND EstimationLineLabor.LaborType > 0 
 
	-- User a cursor to loop through all of the line item IDs    
	DECLARE @cursor CURSOR	    
	SET @cursor = CURSOR FOR    
		SELECT DISTINCT EstimationLIneItemsID     
		FROM #LaborLines    
    
	DECLARE @lineItemID INT    
    
	OPEN @cursor    
	FETCH NEXT FROM @cursor    
	INTO @lineItemID    
    
	WHILE @@FETCH_STATUS = 0    
	BEGIN    
		DECLARE @LaborSummaryPaintPanel VARCHAR(200) = ''    
		DECLARE @LaborSummaryPaintLabor VARCHAR(200) = ''   
		DECLARE @LaborSummaryExtra VARCHAR(200) = ''   		   

		DECLARE @LaborSummaryPaintPanelPrint	VARCHAR(200) = ''    
		DECLARE @LaborSummaryPaintLaborPrint	VARCHAR(200) = ''  
		DECLARE @LaborSummaryExtraPrint			VARCHAR(200) = ''   		
    
		DECLARE @LaborTotalPaintPanel REAL    
		DECLARE @LaborTotalPaint REAL   
		DECLARE @LaborTotalPaintDollars REAL 
		DECLARE @LaborTotalClearcoat REAL   
		DECLARE @LaborTotalClearcoatDollars REAL 
		DECLARE @LaborTotalExtra REAL    
		DECLARE @LaborExtraType VARCHAR(50) 
		DECLARE @LaborExtraIncludedInType VARCHAR(50) 
 
		DECLARE @LaborExtraRemovedType VARCHAR(50) = '' 
		DECLARE @LaborExtraRemovedIncludedInType VARCHAR(50) 
		DECLARE @LaborExtraRemovedHours REAL   
		DECLARE @LaborExtraRemovedTotal REAL  

		DECLARE @isPartsQuantityFlag		BIT
		DECLARE @partsQuantity				REAL
		DECLARE @isLaborQuantityFlag		BIT
		DECLARE @laborQuantity				REAL
		DECLARE @isPaintQuantityFlag		BIT  
		DECLARE @paintQuantity				REAL

        DECLARE @PaintMaterialsBetterment	REAL = 0    
        DECLARE @LaborBetterment			REAL = 0  
        DECLARE @PaintLaborBetterment		REAL = 0

		SELECT @isPartsQuantityFlag = IsPartsQuantityFlag, @partsQuantity = PartsQuantity, @isLaborQuantityFlag = IsLaborQuantityFlag,
			   @laborQuantity = LaborQuantity, @isPaintQuantityFlag = IsPaintQuantityFlag, @paintQuantity = PaintQuantity
		FROM   [dbo].[GetLineItemQuantitySummary](@lineItemID)

		-- Paint Panel is the "Paint Time" value in the paint section of a line item.  The LaborType can be different depending on the Paint Type selection 
		-- but they are all considered Paint Panel 
		SELECT @LaborSummaryPaintPanel = @LaborSummaryPaintPanel +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
								THEN '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
								ELSE '' 
							END    
						END   
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    

        SELECT @PaintLaborBetterment = @PaintLaborBetterment +    
                                CASE    
                                        WHEN @DollarOrHours = 'h'    
                                                THEN    
                                                        CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
                                                                THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1)
                                                                ELSE 0    
                                                        END    
                                        ELSE    
                                                CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
                                                        THEN CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)
                                                        ELSE 0
                                                END    
                                END    
        FROM #LaborLines AS LaborLines  
        WHERE    
                LaborLines.EstimationLineItemsID = @lineItemID    
                AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)

		-- @LaborSummaryPaintPanelPrint
		SELECT @LaborSummaryPaintPanelPrint = @LaborSummaryPaintPanelPrint +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
								THEN '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)) + ' ' + LaborLines.LaborTypeName + ', '    
								ELSE '' 
							END    
					END    
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    

        -- @PaintMaterialsBetterment
        SELECT @PaintMaterialsBetterment = @PaintMaterialsBetterment +    
                                CASE    
                                        WHEN @DollarOrHours = 'h'    
                                                THEN    
                                                        CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
                                                                THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1)
                                                                ELSE 0  
                                                        END    
                                        ELSE    
                                                CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
                                                        THEN CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)
                                                        ELSE 0
                                                END    
                                END    
        FROM #LaborLines AS LaborLines  
        WHERE    
                LaborLines.EstimationLineItemsID = @lineItemID    
                AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)

		SELECT @LaborTotalPaintPanel =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount)  * @paintQuantity, 0)    
			FROM #LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    
		
		-- The clearcoat labor is calculated seperately from the other paint types 
		SELECT @LaborTotalClearcoat =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0) 
			FROM #LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID = 20 
		
		SELECT @LaborTotalClearcoatDollars =     
			ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0) 
			FROM #LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID = 20 
		 
		-- All other paint types are added together 
		SELECT @LaborSummaryPaintLabor = @LaborSummaryPaintLabor +     
			CASE     
				WHEN @DollarOrHours = 'h'    
					THEN    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0     
							THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
							ELSE ''    
						END    
				ELSE     
					'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
			END    
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 
    
		-- @LaborSummaryPaintLaborPrint
		SELECT @LaborSummaryPaintLaborPrint = @LaborSummaryPaintLaborPrint +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0     
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)) + ' ' + LaborLines.LaborTypeName + ', '    
					END    
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 

		SELECT @LaborTotalPaint = ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0)    
			FROM #LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)     
 
		SELECT @LaborTotalPaintDollars = ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0)  
			FROM #LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)  
 
		-- Extra labor is the Labor Time value in section 2 of the line item details, grouped by Labor Type. 
		SELECT 
			  @LaborExtraType = LaborTypes.LaborType 
			, @LaborExtraIncludedInType = LaborTypesMainIncludeIn.LaborType 
			FROM #LaborLines AS LaborLines  
			LEFT JOIN LaborTypes ON LaborTypes.id = LaborLines.LaborTypeID 
			LEFT OUTER JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates.RateType = LaborTypes.RateTypesID 
			LEFT OUTER JOIN LaborTypes LaborTypesMainIncludeIn ON LaborTypesMainIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
    
		SELECT @LaborSummaryExtra = @LaborSummaryExtra +     
			CASE     
				WHEN @DollarOrHours = 'h'    
					THEN    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
							ELSE ''    
						END    
				ELSE     
					'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '     
			END    
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)     
    

		SELECT @LaborSummaryExtraPrint = @LaborSummaryExtraPrint +    
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
									ELSE ''    
								END    
						ELSE     
							'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @LaborQuantity)) + ' ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '     
					END  
		FROM #LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)

        SELECT @LaborBetterment = @LaborBetterment +    
                                CASE    
                                        WHEN @DollarOrHours = 'h'    
                                                THEN    
                                                        CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
                                                                THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 1)    
                                                                ELSE 0    
                                                        END    
                                        ELSE    
                                                CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @LaborQuantity)      
                                END  
        FROM #LaborLines AS LaborLines  
        WHERE    
                LaborLines.EstimationLineItemsID = @lineItemID    
                AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)

		SELECT @LaborTotalExtra =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 0)    
			FROM #LaborLines AS LaborLines  
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
		 
		-- Deleted extra labor.  This is extra labor that was in the previous supplement but not this one 
		SELECT @LaborExtraRemovedType = LaborLines.LaborTypeName   
		FROM #LaborLines LaborLines 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		SELECT  
			@LaborExtraRemovedType = LaborLines.LaborTypeName 
			, @LaborExtraRemovedIncludedInType = LaborTypesIncludeIn.LaborType 
		FROM #LaborLines LaborLines 
		LEFT JOIN LaborTypes ON LaborLines.LaborTypeID = LaborTypes.id 
		LEFT OUTER JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates.RateType = LaborTypes.RateTypesID 
		LEFT OUTER JOIN LaborTypes LaborTypesIncludeIn ON LaborTypesIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
    
		SELECT @LaborExtraRemovedHours =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount), 0) * -1 
			FROM #LaborLines LaborLines 
			WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
				AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
				AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		SELECT @LaborExtraRemovedTotal =     
			((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate) * -1  
		FROM #LaborLines LaborLines 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		INSERT INTO #LaborSummary (    
			EstimationLineItemsID,    
			LaborSummaryPaintPanel,    
			LaborTotalPaintPanel,    
			LaborSummaryPaint,    
			LaborTotalPaint,   
			LaborTotalPaintDollars, 
			LaborTotalClearcoat, 
			ClearcoatLaborIncluded,  
			ClearcoatSuppliesIncluded, 
			LaborSummaryExtra,    
			LaborTotalExtra, 
			LaborExtraType, 
			LaborExtraIncludedInType, 
			LaborExtraRemovedType, 
			LaborExtraRemovedIncludedInType, 
			LaborExtraRemovedHours, 
			LaborExtraRemovedTotal,
			LaborSummaryPaintPanelPrint,
			LaborSummaryPaintLaborPrint,
			LaborSummaryExtraPrint
			,IsPartsQuantity
			,IsLaborQuantity
			,IsPaintQuantity
			,PartsQuantity
			,LaborQuantity
			,PaintQuantity
            ,PaintMaterialsBetterment
            ,LaborBetterment
            ,PaintLaborBetterment
		)    
		VALUES    
		(    
			@lineItemID,    
			SUBSTRING(@LaborSummaryPaintPanel, 0, LEN(@LaborSummaryPaintPanel)),    
			ROUND(@LaborTotalPaintPanel, 1),    
			SUBSTRING(@LaborSummaryPaintLabor, 0, LEN(@LaborSummaryPaintLabor)),    
			ROUND(@LaborTotalPaint, 1),   
			ROUND(@LaborTotalPaintDollars + @LaborTotalClearcoatDollars, 1),  
			ROUND(@LaborTotalClearcoat, 1),   
			@ClearCoatInPaint, 
			@ClearCoatSuppliesInPaint, 
			SUBSTRING(@LaborSummaryExtra, 0, LEN(@LaborSummaryExtra)),    
			ROUND(@LaborTotalExtra, 1), 
			ISNULL(@LaborExtraType, ''), 
			ISNULL(@LaborExtraIncludedInType, ''), 
			ISNULL(@LaborExtraRemovedType, ''),  
			ISNULL(@LaborExtraRemovedIncludedInType, ''), 
			ROUND(ISNULL(@LaborExtraRemovedHours, 0), 1), 
			ROUND(ISNULL(@LaborExtraRemovedTotal, 0), 2),
			SUBSTRING(@LaborSummaryPaintPanelPrint, 0, LEN(@LaborSummaryPaintPanelPrint)), 		
			SUBSTRING(@LaborSummaryPaintLaborPrint, 0, LEN(@LaborSummaryPaintLaborPrint)), 
			SUBSTRING(@LaborSummaryExtraPrint, 0, LEN(@LaborSummaryExtraPrint)),
			@IsPartsQuantityFlag,
			@IsLaborQuantityFlag,
			@IsPaintQuantityFlag,
			@partsQuantity,
			@laborQuantity,
			@paintQuantity,
	        @PaintMaterialsBetterment,
            @LaborBetterment,
            @PaintLaborBetterment
		)    
    
		FETCH NEXT FROM @cursor    
		INTO @lineItemID    
	END     

	-- End of GetLaborSummary
	-------------------------------------------------------------------------------------------------------------------------------------

	-------------------------------------------------------------------------------------------------------------------------------------
	-- Get a table with PDR summary data
	-------------------------------------------------------------------------------------------------------------------------------------
	CREATE TABLE #PDRSummary   
(  
	  AdminInfoID			int  
	, EstimateDataPanelID	int  
	, SupplementVersion		int  
	, LineNumber			int  
	, ForEOR				bit  
	, Panel					varchar(50)  
	, Description			varchar(200)  
	, Price					money  
	, OversizedDescription	varchar(200)  
	, OversizedPrice		money  
	, ModifierDescription	varchar(200)  
	, ModifierPrice			money  
	, Notes					varchar(300)  
	, Sorter				int  
	, GroupNumber			int  
)  
 
	
	  
	CREATE TABLE #OversizedSummary  
	(  
		EstimateDataPanelID		int,   
		Supplement				int,   
		SupplementDeleted		int, 
		Count					int,   
		Size					varchar(50),   
		Depth					varchar(50),   
		Total					money 				  
	)  
  
	INSERT INTO #OversizedSummary   
   
	-- Add oversized dents added   
	SELECT    
		  PDR_EstimateDataPanel.ID AS EstimateDataPanelID	   
		, PDR_EstimateDataPanelOversize.SupplementAdded AS Supplement   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, COUNT(*) AS Count   
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
		, SUM(Amount) AS Total   
	FROM PDR_EstimateDataPanel     
	JOIN PDR_EstimateDataPanelOversize ON PDR_EstimateDataPanel.ID = PDR_EstimateDataPanelOversize.EstimateDataPanelID  
	JOIN PDR_SizeLookup ON PDR_EstimateDataPanelOversize.Size = PDR_SizeLookup.ID 
	JOIN PDR_DepthLookup ON PDR_EstimateDataPanelOversize.Depth = PDR_DepthLookup.ID  
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
		--AND PDR_EstimateDataPanelOversize.SupplementDeleted = 0 
		--AND (PDR_EstimateDataPanelOversize.SupplementDeleted <> PDR_EstimateDataPanelOversize.SupplementAdded OR PDR_EstimateDataPanelOversize.SupplementDeleted = 0)   
	GROUP BY    
		  PDR_EstimateDataPanel.ID   
		, PDR_EstimateDataPanelOversize.SupplementAdded   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
   
	UNION   
   
	---- Add lines for oversized dents that were removed   
	--SELECT    
	--	  PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
	--	, PDR_EstimateDataPanelOversize.SupplementDeleted AS Supplement   
	--	, COUNT(*) AS Count   
	--	, PDR_SizeLookup.Size   
	--	, PDR_DepthLookup.Depth   
	--	, SUM(Amount) * -1 AS Total   
	--FROM PDR_EstimateDataPanel    
	--JOIN PDR_EstimateDataPanelOversize ON PDR_EstimateDataPanelOversize.EstimateDataPanelID = PDR_EstimateDataPanel.ID   
	--JOIN PDR_SizeLookup ON PDR_SizeLookup.ID = PDR_EstimateDataPanelOversize.Size   
	--JOIN PDR_DepthLookup ON PDR_DepthLookup.ID = PDR_EstimateDataPanelOversize.Depth   
	--WHERE    
	--	PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
	--	AND PDR_EstimateDataPanelOversize.SupplementDeleted > 0   
	--	AND PDR_EstimateDataPanelOversize.SupplementDeleted <> PDR_EstimateDataPanelOversize.SupplementAdded    
	--GROUP BY    
	--	PDR_EstimateDataPanel.ID   
	--	, PDR_EstimateDataPanelOversize.SupplementDeleted   
	--	, PDR_SizeLookup.Size   
 --		, PDR_DepthLookup.Depth   
   
	-- Add generic oversized dents and their supplements   
	--UNION    
   
	SELECT    
		PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
		, 0 AS Supplement   
		, 0 
		, PDR_EstimateDataPanel.OversizedDents AS Count   
		, 'Oversized' As Size   
		, '' AS Depth   
		, PDR_Rate.Amount * PDR_EstimateDataPanel.OversizedDents AS Total   
	FROM PDR_EstimateDataPanel   
	LEFT OUTER JOIN PDR_RateProfile ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID AND PDR_RateProfile.Deleted = 0
	LEFT OUTER JOIN PDR_Rate ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_EstimateDataPanel.PanelID = PDR_Rate.PanelID AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
		AND PDR_EstimateDataPanel.OversizedDents <> 0   
   
	UNION   
   
	SELECT    
		PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
		, Sup.SupplementVersion AS Supplement   
		, 0 
		, Sup.OversizedDents AS Count   
		, 'Oversized' As Size   
		, '' AS Depth   
		, PDR_Rate.Amount * Sup.OversizedDents AS Total   
	FROM PDR_EstimateDataPanel   
	JOIN PDR_EstimateDataPanelSupplementChange Sup ON PDR_EstimateDataPanel.ID = Sup.EstimateDataPanelID  
	LEFT OUTER JOIN PDR_RateProfile ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID  AND PDR_RateProfile.Deleted = 0   
	LEFT OUTER JOIN PDR_Rate ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_EstimateDataPanel.PanelID = PDR_Rate.PanelID AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
		AND Sup.OversizedDents <> 0   
   
	--SELECT * FROM #OversizedSummary   
	-- End of getting #OversizedSummary   
	---------------------------------------------------------------------------------------------------------------------------------   
	   
	   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create another temporary table to add final result lines to with data from the #OversizedSummary made above   
	-- There will be 1 line for each data panel   
	---------------------------------------------------------------------------------------------------------------------------------   
	CREATE TABLE #OversizedResults  
	(  
		EstimateDataPanelID		int,   
		SupplementVersion		int,   
		Description				varchar(300),   
		Total					money 				  
	)  
	-- Loop through each data panel ID in the summary table and add a line to the results table   
	DECLARE @PDRcursor CURSOR	   
	SET @PDRcursor = CURSOR FOR   
		SELECT DISTINCT EstimateDataPanelID, dbo.Biggest(Supplement, SupplementDeleted) As Supplement 
		FROM #OversizedSummary   
   
	DECLARE @dataPanelID INT   
    DECLARE @suppVersion INT

	OPEN @PDRcursor   
	FETCH NEXT FROM @PDRcursor   
	INTO @dataPanelID, @suppVersion   
   
	WHILE @@FETCH_STATUS = 0   
		BEGIN   
   
			DECLARE @Summary VARCHAR(200) = ''   
			DECLARE @Total MONEY = 0   
   
			SELECT    
				  @Summary = @Summary + CAST(Count AS VARCHAR) + ' x ' + Size + ' ' + Depth + ', '   
				, @Total = @Total + Total   
				FROM #OversizedSummary   
				WHERE    
					EstimateDataPanelID = @dataPanelID 
					AND Supplement <= @suppVersion   
					AND (SupplementDeleted > @suppVersion OR SupplementDeleted = 0) 
					AND (Supplement = (SELECT MAX(Supplement) FROM #OversizedSummary WHERE EstimateDataPanelID = @dataPanelID AND Size = 'Oversized' AND Supplement <= @suppVersion) OR Size <> 'Oversized')	 
   
			INSERT INTO #OversizedResults    
			(   
				  EstimateDataPanelID   
				, SupplementVersion   
				, Description   
				, Total   
			)   
			VALUES   
			(   
				  @dataPanelID   
				, @suppVersion   
				, SUBSTRING(@Summary, 0, LEN(@Summary))   
				, @Total   
			)   
   
			FETCH NEXT FROM @PDRcursor   
			INTO @dataPanelID, @suppVersion   
		END     
   
	--SELECT * FROM @OversizedResults   
   
	-- End of creating #OversizedResults   
	---------------------------------------------------------------------------------------------------------------------------------   
   
   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create a table to replace the PDR_EstimateDataPanel table that joins in data from the SupplementChange table so that we have    
	-- a record for the base supplement as well as the supplements.   
   
	CREATE TABLE #PanelData  
	(  
		EstimateDataPanelID		int,   
		PanelID					int,   
		QuantityID				int,   
		SizeID					int,   
		OversizedDents			int,   
		Multiplier				int,   
		CustomCharge			money,
		SupplementVersion		int,   
		LineNumber				int,   
		Notes					varchar(300) 				  
	)  
   
	INSERT INTO #PanelData   
  
	SELECT DISTINCT * FROM (   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, 0 As SupplementVersion, base.LineNumber, base.Description    
		FROM PDR_EstimateDataPanel base   
		LEFT OUTER JOIN #OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND OversizedResults.SupplementVersion = 0
		WHERE AdminInfoID = @AdminInfoID   
			AND (base.QuantityID > 0 OR base.SizeID > 0 OR base.OversizedDents > 0 OR OversizedResults.EstimateDataPanelID IS NOT NULL OR ISNULL(CustomCharge, 0) <> 0)   
   
		UNION    
   
		SELECT base.ID, base.PanelID, Sup.QuantityID, Sup.SizeID, Sup.OversizedDents, Sup.Multiplier, Sup.CustomCharge, Sup.SupplementVersion, Sup.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelSupplementChange Sup ON base.ID = Sup.EstimateDataPanelID   
		LEFT OUTER JOIN #OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND Sup.SupplementVersion = OversizedResults.SupplementVersion  
		WHERE base.AdminInfoID = @AdminInfoID 		  
   
	) AS panelData   
  
	-- Add any EstimateDataPanels who haven't been changed this supplement but have an oversize change   
	INSERT INTO #PanelData  
  
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, ISNULL(sup.OversizedDents, base.OversizedDents) AS OversizedDents, ISNULL(sup.Multiplier, base.Multiplier) AS Multiplier, ISNULL(sup.CustomCharge, base.CustomCharge), oversize.SupplementAdded As SupplementVersion, dbo.Biggest(ISNULL(sup.LineNumber, 0), base.LineNumber) AS LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID    
		LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange sup ON base.ID = sup.EstimateDataPanelID AND sup.SupplementVersion = (SELECT MAX(SupplementVersion) FROM PDR_EstimateDataPanelSupplementChange WHERE EstimateDataPanelID = base.ID) 
		WHERE base.AdminInfoID = @AdminInfoID AND oversize.SupplementAdded > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
		UNION   
   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, oversize.SupplementDeleted As SupplementVersion, base.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID  
		WHERE base.AdminInfoID = @AdminInfoID AND oversize.SupplementDeleted > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
    	DELETE FROM #PanelData WHERE SupplementVersion > @SupplementVersion

	--DROP TABLE #SupplementVersions   

	--SELECT * FROM @PanelData
	---------------------------------------------------------------------------------------------------------------------------------   
	  
	INSERT INTO #PDRSummary   
   
	SELECT -- DISTINCT  *
		DISTINCT    
			  @AdminInfoID  
			, PanelData.EstimateDataPanelID As ID   
			, PanelData.SupplementVersion   
			--, (SELECT MAX(LineNumber) FROM @PanelData WHERE EstimateDataPanelID = PanelData.EstimateDataPanelID) AS LineNumber 
			, LineNumber
			, CASE    
				WHEN PanelData.SupplementVersion = 
				dbo.Smallest(
				(   
					SELECT MAX(SupplementVersion)    
					FROM #PanelData panelDataSub    
					WHERE panelDataSub.EstimateDataPanelID = PanelData.EstimateDataPanelID   
				), @SupplementVersion)
				AND (PDR_Rate.Amount IS NOT NULL OR ISNULL(OversizedResults.Total, 0) <> 0 OR ISNULL(CustomCharge, 0) <> 0) THEN 1 ELSE 0 END As ForEOR    
			, ISNULL(PDR_CategoryMap.CategoryName, UPPER(PDR_Panel.PanelName)) AS panel   
			, PDR_Panel.PanelName + ' Dents '  
			  + CASE 
					WHEN PanelData.CustomCharge <> 0 THEN '- Custom Charge'
				ELSE  
					CASE   
						WHEN PDR_Rate.ID IS NULL   
							THEN CASE WHEN ISNULL(OversizedResults.Total, 0) <> 0 THEN '' ELSE 'DELETED' END  
						ELSE   
							CASE WHEN ISNULL(PDR_RateProfile.HideDentCounts, 0) = 0   
								THEN CAST(PDR_QuantityLookup.Min AS VARCHAR) + ' - ' + CAST(PDR_QuantityLookup.Max AS VARCHAR)    
							ELSE ' -'   
							END   
						+ ' ' + PDR_SizeLookup.Size   
					END 
			  END AS Description   
			, CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END AS Price   
			, ISNULL(OversizedResults.Description, '') As OversizedDescription   
			, ISNULL(OversizedResults.Total, 0) As OversizedPrice   
			, CASE   
				 WHEN ISNULL(PDR_Multiplier.ID, 0) > 0   
					THEN CAST(ISNULL(PDR_Multiplier.Value, 0) AS VARCHAR(50)) + '% ' + PDR_Multiplier.Name + ' Modifier'    
					ELSE ''    
				END AS ModifierDescription   
			, CASE WHEN ISNULL(PDR_Multiplier.ID, 0) = 0 THEN 0 ELSE CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END * (PDR_Multiplier.Value / 100) END AS ModifierPrice   
			, PanelData.Notes   
			, (ISNULL(Category.nheader, 1000) * 256) + PDR_Panel.SortOrder AS Sorter		   
			, (ISNULL(Category.nheader, 1000) * 256) + PDR_Panel.SortOrder AS GroupNumber   
		FROM PDR_EstimateData   
   
		-- Join in PDR panels with selections   
		JOIN #PanelData AS PanelData ON 1 = 1   
   
		-- Join the rate profile data   
		JOIN PDR_RateProfile ON PDR_RateProfile.ID = PDR_EstimateData.RateProfileID  AND PDR_RateProfile.Deleted = 0
		LEFT OUTER JOIN PDR_Rate ON    
			PDR_Rate.RateProfileID = PDR_RateProfile.ID    
			AND PanelData.SizeID = PDR_Rate.Size   
			AND PanelData.PanelID = PDR_Rate.PanelID  
			AND PanelData.QuantityID = PDR_Rate.Quantity   
   
		LEFT OUTER JOIN PDR_EstimateDataPanelOversize ON PanelData.EstimateDataPanelID = PDR_EstimateDataPanelOversize.EstimateDataPanelID    
   
		JOIN PDR_Panel ON PanelData.PanelID = PDR_Panel.PanelID   
   
		LEFT OUTER JOIN PDR_CategoryMap ON PDR_Panel.PanelID = PDR_CategoryMap.PanelID   
		LEFT OUTER JOIN Mitchell3.dbo.Category ON Category.Service_Barcode = @Service_BarCode AND PDR_CategoryMap.CategoryName = Category.Category   
   
		LEFT OUTER JOIN PDR_SizeLookup ON PanelData.SizeID = PDR_SizeLookup.ID   
		LEFT OUTER JOIN PDR_QuantityLookup ON PanelData.QuantityID = PDR_QuantityLookup.ID   
		LEFT OUTER JOIN PDR_Multiplier ON PanelData.Multiplier = PDR_Multiplier.ID   
   
		LEFT OUTER JOIN #OversizedResults AS OversizedResults ON OversizedResults.EstimateDataPanelID = PanelData.EstimateDataPanelID AND OversizedResults.SupplementVersion = PanelData.SupplementVersion   
   
		WHERE    
			PDR_EstimateData.AdminInfoID = @AdminInfoID   
			AND PanelData.SupplementVersion <= @SupplementVersion
			--AND    
			--(   
			--	PDR_Rate.Amount * CASE WHEN ISNULL(PDR_Multiplier.Value, 0) = 0 THEN 1 ELSE PDR_Multiplier.Value / 100 END > 0   
			--)   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- END OF PDR
	---------------------------------------------------------------------------------------------------------------------------------   


	 INSERT INTO #ProcessedLines 
	SELECT  
		EstimationLineItems.ID 
 ,
		-- Don't include lines that have been change by a supplement 
		CASE WHEN ISNULL(SuppOverride.ID, 0) > 0 AND ISNULL(EstimationLineItems.SupplementVersion, 0) < @SupplementVersion THEN 0 ELSE 1 END AS ForSummary, 
  
		-- Set a flag if a line has been modified by another line with a different part source, these lines aren't shown in the main list ofline items  
		CASE WHEN EstimationLineItems.id <> EstimationLineItemsModified.id AND EstimationLineItems.PartSource <> EstimationLineItemsModified.PartSource THEN 1 ELSE 0 END AS PartSourceModified,  
		CASE WHEN EstimationLineItems.id <> EstimationLineItemsModified.id THEN ISNULL(EstimationLineItemsModified.SupplementVersion, 0) ELSE 0 END AS ModifiedSupplementVersion,  
		EstimationLineItemsModified.ID AS ModifiedID, 
	  
		-- If there is no panel, return ZZZ so it is sorted at the end of the list.  The report will not show ZZZ  
				ISNULL(  
			CASE WHEN @Service_Barcode IS NULL
				THEN
					dbo.GetManualSection(ISNULL(ModifiesData.SectionID, EstimationLineItems.SectionID))
				ELSE
					NULLIF(  
						CASE WHEN ModifiesData.id IS NULL  
							THEN ISNULL(Mitchell3.dbo.GetCategory(@Service_BarCode, CASE WHEN ParentLineItems.SectionID IS NULL THEN EstimationLineItems.SectionID ELSE ParentLineItems.SectionID END), ISNULL(T1.Step, ''))  
						ELSE Mitchell3.dbo.GetCategory(@Service_BarCode, ModifiesData.SectionID)  
						END
					, '')
				END
		, 'ZZZ') As 'Panel',  
  
		CASE WHEN ISNULL(EstimationLineItems.SupplementVersion,0) > 0 AND EstimationLineItems.ModifiesID = -1 THEN 1 ELSE 0 END 'Added',  
		CASE   
			WHEN (ISNULL(SuppOverride.id, 0) > 0 AND SuppOverride.ActionDescription LIKE 'Delete%')  
				OR (ISNULL(ModifiesData.id, 0) > 0 AND EstimationLineItems.ActionDescription LIKE 'Delete%')  
			THEN 1   
		--WHEN EstimationLineItems.id <> EstimationLineItemsModified.id AND EstimationLineItems.PartSource <> EstimationLineItemsModified.PartSource THEN 1  
		WHEN EstimationLineItems.id <> EstimationLineItemsModified.id THEN 1  
		ELSE 0 END 'Removed',  
  
		REPLACE(ISNULL(EstimationLineItems.BettermentType, ''), ' ', '') AS BettermentType,  
		ISNULL(EstimationLineItems.BettermentValue, 0) AS BettermentValue,  
		CASE	  
			WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'P' THEN  
				CASE   
					WHEN EstimationOverlap.OverlapAdjacentFlag = 1 THEN 0 
					ELSE   
						((CASE WHEN ISNULL(EstimationLineItems.BettermentParts, 0) = 1 THEN ISNULL(EstimationLineItems.Price, 0) ELSE 0 END) + 
						(CASE WHEN ISNULL(EstimationLineItems.BettermentPaintMaterials, 0) = 1 THEN ISNULL(LaborSummary.PaintMaterialsBetterment, 0) ELSE 0 END) + 
						--labor
						(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ISNULL(LaborSummary.LaborBetterment,0) ELSE 0 END) + 
						--paint
						(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ISNULL(LaborSummary.PaintLaborBetterment,0) ELSE 0 END))
						 *   
						 CASE   
							WHEN EstimationLineItems.BettermentType = 'P'  
								THEN (ISNULL(EstimationLineItems.BettermentValue,0) / 100) * -1  
							ELSE -1  
						END  
				END   
			  
			WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'D' THEN  
				ISNULL(EstimationLineItems.BettermentValue,0) * -1  
			ELSE 0  
		END AS BettermentAmount,  
  
		CASE  
			WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'P' THEN  
				'Betterment ' + CAST(EstimationLineItems.BettermentValue AS VARCHAR) + '%'  + 

				(CASE WHEN ISNULL(EstimationLineItems.BettermentParts,0) = 1 THEN ' Parts' + ',' ELSE '' END) + 
				(CASE WHEN ISNULL(EstimationLineItems.BettermentPaintMaterials,0) = 1 THEN ' Paint Materials' + ',' ELSE '' END) + 
				(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ' Labor' + ',' ELSE '' END) + 
				(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ' Paint Labor' + ',' ELSE '' END)

			WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'D' THEN  
				'Betterment $' + CAST(EstimationLineItems.BettermentValue AS VARCHAR)  + 

				(CASE WHEN ISNULL(EstimationLineItems.BettermentParts,0) = 1 THEN ' Parts' + ',' ELSE '' END) + 
				(CASE WHEN ISNULL(EstimationLineItems.BettermentPaintMaterials,0) = 1 THEN ' Paint Materials' + ',' ELSE '' END) + 
				(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ' Labor' + ',' ELSE '' END) + 
				(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ' Paint Labor' ELSE '' END)

			ELSE ''  
		END As BettermentNote,
  
		ISNULL(EstimationLineItems.BettermentParts,0), 
		ISNULL(EstimationLineItems.BettermentPaintMaterials,0), 
		(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN 1 ELSE 0 END), 
		(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN 1 ELSE 0 END),  
		
		PaintMaterialsBetterment, 
		LaborBetterment, 
		PaintLaborBetterment,
  
		CASE WHEN ISNULL(EstimationLineItems.SubletOperationFlag,0) <> 0 THEN 1 ELSE 0 END 'Sublet',  
  
		CASE WHEN EstimationLineItems.ActionCode = 'PDR' THEN 'PDR' 
		ELSE
			CASE WHEN ISNULL(EstimationLineItems.Description + ' ','') NOT LIKE ISNULL(ActionList.ActionLong + ' ','') + '%' THEN ISNULL(ActionList.ActionLong + ' ','') ELSE '' END
		END AS 'Action',  
  
		COALESCE(EstimationLineItems.ActionDescription, ModifiesData.ActionDescription) AS OperationDescription, 
 
		dbo.HtmlEncode(UPPER(ISNULL(COALESCE(EstimationLineItems.Description, ModifiesData.Description), '')) +    
		CASE WHEN EstimationOverlap.OverlapAdjacentFlag = 1
			THEN ', Included in ' + ISNULL(EstimationLineItemsAssembly.Description,' Assembly')   
			ELSE ''   
		END  
		+ CASE WHEN ISNULL(EstimationLineItems.SubletOperationFlag, 0) = 1 THEN ' Sublet' ELSE '' END
		+ CASE WHEN ISNULL(OtherLaborType.LaborType, '') <> '' 
			THEN 
				-- Add a Dash if there is a description and labor type
				CASE WHEN ISNULL(COALESCE(EstimationLineItems.Description, ModifiesData.Description), '') <> '' THEN ' - ' ELSE '' END
				+ ISNULL(OtherLaborType.LaborType, '') 
			ELSE '' 
			END 
		+ ' ' + UPPER(COALESCE(EstimationLineItems.ActionDescription, ModifiesData.ActionDescription)))
		AS 'Description',   
		
		CASE	
			WHEN EstimationLineItems.ActionCode = 'Replace' THEN
				CASE   
					WHEN   
						ISNULL(COALESCE(EstimationLineItems.BettermentType, ModifiesData.BettermentType), '') <> ''   
						AND ISNULL(COALESCE(EstimationLineItems.BettermentValue, ModifiesData.BettermentValue), 0) > 0   
						--AND ISNULL(COALESCE(EstimationLineItems.BettermentParts, ModifiesData.BettermentParts), 0) = 1  -- Ezra 6/6 this was in the old query, but BettermentParts never seems to be set to true.  Not sure how this worked...
					THEN '(Bet) ' ELSE '' END +  
				CASE   
					WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) = 'LKQ' AND ISNULL(CustomerProfilesMisc.LKQText, '') <> '' THEN CustomerProfilesMisc.LKQText  
					WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) = 'LKQ' AND ISNULL(CustomerProfilesMisc.LKQText,'')='' THEN '*LKQ*'  
					WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) = 'After' THEN isnull(nullif(COALESCE(EstimationLineItems.SourcePartNumber, ModifiesData.SourcePartNumber),''), COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) )  
					WHEN COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) NOT LIKE '**%' THEN COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) ELSE ''   
				END 
			ELSE ''
		END	'PartNumber',  
  
		CASE   
			WHEN EstimationOverlap.OverlapAdjacentFlag = 1 THEN 0 
			ELSE 
				CASE WHEN @ForPreview = 0 
					THEN
						EstimationLineItems.Price + (EstimationLineItems.Price * (CustomerProfileRates.DiscountMarkup / 100)) 
					ELSE
						EstimationLineItems.Price 
					END  
		END 
		'Price',   
  
		CASE WHEN EstimationLineItems.Price = 0 THEN null ELSE dbo.GetLineItemQuantity(EstimationLineItems.ID) END 'Quantity',   
		CASE WHEN ISNULL(EstimationLineItems.PartSource, '') != '' THEN ISNULL(EstimationLineItems.PartSource, '') ELSE 'Other' END AS 'PartSource',  
 
		-- If the part source changed, get the full price of the modified part and select it as subtracted 
		CASE  
			WHEN ISNULL(@SupplementVersion, 0) > 0 AND ISNULL(EstimationLineitems.PartSource, '') <> ISNULL(EstimationLineItemsModifies.PartSource, '') AND ISNULL(EstimationLineItemsModifies.PartSource, '') <> '' 
				THEN 
					CASE WHEN @ForPreview = 0 
						THEN
							EstimationLineItems.Price + (EstimationLineItems.Price * (CustomerProfileRatesRemoved.DiscountMarkup / 100)) 
						ELSE
							EstimationLineItems.Price 
						END  
			ELSE 0 
		END AS RemovedPrice, 
 
		CASE  
			WHEN ISNULL(@SupplementVersion, 0) > 0 AND ISNULL(EstimationLineitems.PartSource, '') <> ISNULL(EstimationLineItemsModifies.PartSource, '') 
				THEN dbo.GetLineItemQuantity(EstimationLineItems.ModifiesID) 
			ELSE 0 
		END AS RemovedQuantity, 
 
		CASE  
			WHEN ISNULL(@SupplementVersion, 0) > 0 AND ISNULL(EstimationLineitems.PartSource, '') <> ISNULL(EstimationLineItemsModifies.PartSource, '') 
				THEN ISNULL(EstimationLineItemsModifies.PartSource, '') 
			ELSE '' 
		END AS RemovedPartSource, 
 
		CASE 
			 WHEN EstimationLineItems.PartSource = 'After' THEN @ImageDirectory + 'Aftermarket.gif'  
			 WHEN EstimationLineItems.PartSource = 'LKQ' THEN @ImageDirectory + 'LKQ.gif'  
			 WHEN EstimationLineItems.PartSource = 'Reman' THEN @ImageDirectory + 'Remanufacturered.gif'  
			 ELSE @ImageDirectory + 'empty.gif'  
		END As PartSourceImage,  
  
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintPanel, '')) AS 'LaborSummaryPaintPanel',
		ISNULL(ROUND(LaborSummary.LaborTotalPaintPanel, 1), 0), 
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaint, '')) AS 'LaborSummaryPaint', 
		ISNULL(ROUND(LaborSummary.LaborTotalPaint, 1), 0), 
		ISNULL(ROUND(LaborSummary.LaborTotalPaintDollars, 1), 0),
		ISNULL(ROUND(LaborSummary.LaborTotalClearcoat, 1), 0),
		ISNULL(LaborSummary.ClearcoatLaborIncluded, ''),
		ISNULL(LaborSummary.ClearcoatSuppliesIncluded, ''),
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryExtra, '')) AS 'LaborSummaryExtra', 
		ROUND(ISNULL(LaborSummary.LaborTotalExtra, 0), 1) AS LaborTotalExtra,
		ROUND(
			-- For Imported estimates, subtract EstimationLineItems.LaborTime from the body labor
			CASE WHEN ISNULL(AdminInfo.IsImported, 0) = 1  THEN
				CASE WHEN ISNULL(LaborSummary.LaborTotalExtra, 0) + ISNULL(EstimationLineItems.LaborTime, 0) < 0 THEN 0
					 ELSE ISNULL(LaborSummary.LaborTotalExtra, 0) + ISNULL(EstimationLineItems.LaborTime, 0)
					 END
			ELSE
			-- For regular estimates, subtract the overlap amountfrom the overlap table
				CASE WHEN ISNULL(LaborSummary.LaborTotalExtra, 0) < ISNULL(EstimationOverlap.Minimum, 0) THEN ISNULL(LaborSummary.LaborTotalExtra, 0)	-- Labor must be > the minimum to get deducted
				ELSE 
					-- Remove the amount from the labor, cap at 0
					CASE WHEN ISNULL(LaborSummary.LaborTotalExtra, 0) + ISNULL(EstimationOverlap.Amount, 0) < 0 THEN 0
					ELSE ISNULL(LaborSummary.LaborTotalExtra, 0) + ISNULL(EstimationOverlap.Amount, 0)
					END
				END
			END
			
		, 1) AS LaborTotalExtraOverlapped, 
		ISNULL(LaborSummary.LaborExtraType, ''),
		ISNULL(LaborSummary.LaborExtraIncludedInType, ''),
		ISNULL(LaborSummary.LaborExtraRemovedType, ''), 
		ISNULL(LaborSummary.LaborExtraRemovedIncludedInType, ''),
		ISNULL(ROUND(LaborSummary.LaborExtraRemovedHours, 1), 0), 
		ISNULL(ROUND(LaborSummary.LaborExtraRemovedTotal, 1), 0), 
 
		CASE WHEN @UseNewOverlapFormula = 0 AND ISNULL(AdminInfo.IsImported, 0) = 0 THEN
			-- Use the old formula
			CASE  
				WHEN ActionList.ActionLong IN ('R+R','R+I','R&I','Replace')
				AND  
				( 
					(EstimationOverlap2.Amount IS NOT NULL AND EstimationOverlap2.Amount = 0)  
					OR  
					( 
						ABS(LaborSummary.LaborTotalExtra) > 0 
						AND 
						( 
							( 
								ISNULL(EstimationOverlap1.Minimum, 0) = 0 
								AND (ABS(LaborSummary.LaborTotalExtra) <= ABS(EstimationOverlap1.Amount)) 
							) 
							OR  
							( 
								ISNULL(EstimationOverlap1.Minimum, 0) <> 0  
								AND (ABS(LaborSummary.LaborTotalExtra) - ABS(ISNULL(EstimationOverlap1.Minimum, 0)) <= CASE WHEN @AdminInfoID > @OverlapRoundingFirst THEN 0.001 ELSE 0 END)	-- 4/10/2019 - fixed a bug where a rounding error caused labor to not be included
							) 
						)  
					)					 
				)  
				THEN 1 
				ELSE 0 
			END 

		-- Otherwise start with labor not included, this will be calculated in the next query using the new formula
		ELSE 0 END AS LaborIncluded, 

		CASE WHEN @AdminInfoID > @OverlapRoundingFirst THEN ISNULL(EstimationOverlap.SupplementLevel, 0) ELSE 0 END AS OverlapSupplement,
 
		ISNULL(EstimationLineLaborOther.LaborCost, 0) +
			CASE WHEN @ForPreview = 1 THEN 0
			ELSE (ISNULL(EstimationLineLaborOther.LaborCost, 0) * (ISNULL(OtherLaborRate.DiscountMarkup, 0) / 100)) 
			END
		AS OtherCharges, 

		ISNULL(EstimationLineLaborOther.LaborType, 0) AS OtherChargesLaborType, 
 
		CASE 
			WHEN ISNULL(EstimationLineLaborOther.LaborType, 0) <> ISNULL(EstimationLineLaborModifiesOther.LaborType, 0) 
				THEN ISNULL(EstimationLineLaborModifiesOther.LaborCost, 0) + (ISNULL(EstimationLineLaborModifiesOther.LaborCost, 0) * (ISNULL(ModifiesOtherLaborRate.DiscountMarkup, 0) / 100))
			ELSE 0 
		END AS RemovedOtherCharges, 
 
		CASE 
			WHEN ISNULL(EstimationLineLaborOther.LaborType, 0) <> ISNULL(EstimationLineLaborModifiesOther.LaborType, 0) 
				THEN ISNULL(EstimationLineLaborModifiesOther.LaborType, 0) 
			ELSE 0 
		END AS RemovedOtherChargesLaborType, 
 
		CASE WHEN ISNULL(EstimationLineLaborPaint.AdjacentDeduction, 0) > 0 THEN 
			cast(EstimationLineLaborPaint.LaborTime AS VARCHAR(20)) 
			+ ' hrs. Paint - ' + 
			CASE  
				WHEN EstimationLineLaborPaint.AdjacentDeduction = 1 THEN FocusWrite.dbo.FormatNumber(CustomerProfilesPaint.AdjacentDeduction,1)  
				WHEN EstimationLineLaborPaint.AdjacentDeduction = 2 THEN FocusWrite.dbo.FormatNumber(CustomerProfilesPaint.NonAdjacentDeduction,1)  
				ELSE ''  
			END  
			+ ' hrs. Adjacent Deduction' 
		ELSE '' END  
		
		AS DeductionsMessage, 

		CASE 
			WHEN EstimationLineLaborPaint.AdjacentDeduction = 1 THEN 'Adj O/L'
			WHEN EstimationLineLaborPaint.AdjacentDeduction = 2 THEN 'Non Adj O/L'
			ELSE '' 
		END AS AdjacentMessage,
 
		CASE   
			WHEN ISNULL(CustomerProfilePrint.PrintPublicNotes, 0) = 1 THEN ISNULL(CONVERT(VarChar(5000), EstimationNotes.Notes), '')  
			ELSE ''  
		END 'Notes',  
  
		COALESCE(EstimationLineItems.LineNumber, ModifiesData.LineNumber) As LineNumber,  
  
		ISNULL(EstimationLineItems.SupplementVersion, 0) AS 'SupplementVersion',  
  
		CASE   
			WHEN SuppOverride.ActionDescription LIKE 'Delete%'  
			THEN SuppOverride.SupplementVersion ELSE 0 END 'DeletedBySupplement',  
  
		dbo.Biggest(ISNULL(EstimationLineItems.SupplementVersion, 0), ISNULL(SuppOverride.SupplementVersion, 0)) AS 'SupplementDisplay',
		--ISNULL(COALESCE(EstimationLineItems.SupplementVersion, SuppOverride.SupplementVersion), 0) AS 'SupplementDisplay',  
  
		--ISNULL(T1.Step,t2.step) +'-'+  
		ISNULL(T1.Step,'') +'-'+
		CONVERT(VarChar(3),ISNULL(EstimationLineItems.SupplementVersion,0)) 'StepSupp'  

		, ISNULL(T1.nheader, 1000) * 256 AS Sorter 
		, LineItemSummary.GroupNumber 
		, '' AS OversizedDescription 
		, 0 AS OversizedPrice 
		, '' AS ModifierDescription 
		, 0 AS ModifierPrice,
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintPanelPrint, '')) AS LaborSummaryPaintPanelPrint,
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintLaborPrint, '')) AS LaborSummaryPaintLaborPrint, 
		dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryExtraPrint, '')) AS LaborSummaryExtraPrint
		,ISNULL(LaborSummary.IsPartsQuantity, EstimationLineItems.IsPartsQuantity)
		,ISNULL(LaborSummary.IsLaborQuantity, EstimationLineItems.IsLaborQuantity)
		,ISNULL(LaborSummary.IsPaintQuantity, EstimationLineItems.IsPaintQuantity)
		,ISNULL(LaborSummary.PartsQuantity, EstimationLineItems.Qty)
		,ISNULL(LaborSummary.LaborQuantity, 1)
		,ISNULL(LaborSummary.PaintQuantity, 1)
	FROM AdminInfo  
	JOIN EstimationData with(nolock) ON EstimationData.AdminInfoID = AdminInfo.ID
	JOIN #LineItems EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID   
	-- TODO - Customer profile sups, not ready to go live
	--LEFT JOIN CustomerProfiles ON CustomerProfiles.id = AdminInfo.CustomerProfilesID AND ISNULL(CustomerProfiles.Supplement, 0) = @SupplementVersion 
	JOIN CustomerProfiles with(nolock) ON CustomerProfiles.id = AdminInfo.CustomerProfilesID  
	JOIN CustomerProfilePrint with(nolock) ON CustomerProfilePrint.id = (SELECT TOP 1 id FROM CustomerProfilePrint WHERE CustomerProfilesID = CustomerProfiles.id)
	JOIN CustomerProfilesMisc with(nolock) ON CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id 
	LEFT JOIN VehicleInfo with(nolock) ON EstimationData.ID = VehicleInfo.EstimationDataId	
	LEFT JOIN #LineItems ParentLineItems ON EstimationLineItems.ParentLineID = ParentLineItems.ID
	LEFT JOIN ActionList with(nolock) ON   EstimationLineItems.ActionCode =  ActionList.ActionShort 
	LEFT JOIN #EstimateLineItemSummary AS LineItemSummary ON EstimationLineItems.id  = LineItemSummary.LineItemID  
	LEFT JOIN #EstimateLineItemSummary T1 ON T1.Service_BarCode = @Service_BarCode AND T1.barcode = ISNULL(ParentLineItems.Barcode, EstimationLineItems.Barcode)

	--LEFT JOIN (SELECT DISTINCT GroupNumber, Step FROM #EstimateLineItemSummary AS T2Base WHERE NOT T2Base.Service_BarCode IS NULL) T2 ON T2.GroupNumber = EstimationLineItems.SectionID 
  
	LEFT JOIN EstimationNotes with(nolock)   
		ON EstimationLineItems.ID  =   EstimationNotes.EstimationLineItemsID
		AND (EstimationNotes.Printed = 1 AND ISNULL(CustomerProfilePrint.PrintPublicNotes, 0) = 1)   
	LEFT JOIN #LineItems EstimationLineItemsModified ON Focuswrite.Dbo.GetLatestLineItemMod(EstimationLineItems.ID)   =  EstimationLineItemsModified.id
	LEFT JOIN #LineItems EstimationLineItemsModifies ON EstimationLineItems.ModifiesID = EstimationLineItemsModifies.ID  
	LEFT JOIN (	        
		SELECT 'LKQ' 'PartSource', 9 'RateType'  
		UNION SELECT 'OEM', 8  
		UNION SELECT 'Retail', 8  
		UNION SELECT 'After', 10  
		UNION SELECT 'Reman', 13  
		UNION SELECT 'Other', 18 	  
	) PartSourceRateTypes ON CASE WHEN ISNULL(EstimationLineItems.PartSource, '') = '' THEN 'Other' ELSE EstimationLineItems.PartSource END = PartSourceRateTypes.PartSource
	LEFT JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = CustomerProfiles.id AND CustomerProfileRates.RateType = PartSourceRateTypes.RateType   

	LEFT JOIN (	        
		SELECT 'LKQ' 'PartSource', 9 'RateType'  
		UNION SELECT 'OEM', 8  
		UNION SELECT 'Retail', 8  
		UNION SELECT 'After', 10  
		UNION SELECT 'Reman', 13  
		UNION SELECT 'Other', 18 	  
	) PartSourceRemovedRateTypes ON CASE WHEN ISNULL(EstimationLineItemsModifies.PartSource, '') = '' THEN '' ELSE EstimationLineItemsModifies.PartSource END = PartSourceRemovedRateTypes.PartSource
	LEFT JOIN CustomerProfileRates AS CustomerProfileRatesRemoved ON CustomerProfiles.id = CustomerProfileRatesRemoved.CustomerProfilesID   AND CustomerProfileRatesRemoved.RateType = PartSourceRemovedRateTypes.RateType
   
	LEFT JOIN #LaborItems EstimationLineLaborMain ON EstimationLineItems.ID = EstimationLineLaborMain.EstimationLineItemsID AND EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25)  
	
	LEFT JOIN LaborTypes LaborTypesMain   ON  EstimationLineLaborMain.LaborType  = LaborTypesMain.id
  
	LEFT JOIN #LaborItems EstimationLineLaborPaint   
		ON  EstimationLineItems.ID    = EstimationLineLaborPaint.EstimationLineItemsID
		AND  
		(  
			EstimationLineLaborPaint.LaborType in (9,16,19,18,29)  -- Clearcoat, Base Coat, 2 Stage, 3 Stage, 2 Tone 
			OR   
			(EstimationLineLaborPaint.LaborType = 26 AND EstimationLineItems.ActionCode = 'Blend')   
		)  
	LEFT JOIN LaborTypes LaborTypesPaint with(nolock)  ON EstimationLineLaborPaint.LaborType = LaborTypesPaint.id
  
	LEFT JOIN #LaborItems EstimationLineLaborOther ON EstimationLineItems.ID  = EstimationLineLaborOther.EstimationLineItemsID AND EstimationLineLaborOther.LaborType in (13,14,15,30,31)  
	LEFT JOIN LaborTypes OtherLaborType with(nolock) ON EstimationLineLaborOther.LaborType = OtherLaborType.id
	LEFT JOIN CustomerProfileRates OtherLaborRate with(nolock) ON CustomerProfiles.id =  OtherLaborRate.CustomerProfilesID AND OtherLaborType.RateTypesID = OtherLaborRate.RateType

	LEFT JOIN #LaborItems EstimationLineLaborModifiesOther ON  EstimationLineItemsModifies.ID = EstimationLineLaborModifiesOther.EstimationLineItemsID AND EstimationLineLaborModifiesOther.LaborType in (13,14,15,30,31) 
	LEFT JOIN LaborTypes ModifiesOtherLaborType with(nolock) ON ModifiesOtherLaborType.id = EstimationLineLaborModifiesOther.LaborType
	LEFT JOIN CustomerProfileRates ModifiesOtherLaborRate with(nolock) ON CustomerProfiles.id = ModifiesOtherLaborRate.CustomerProfilesID  AND ModifiesOtherLaborType.RateTypesID = ModifiesOtherLaborRate.RateType

	-- New correct overlap logoc
	--LEFT JOIN EstimationOverlap ON @UseNewOverlapFormula = 1 AND EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1 AND EstimationOverlap.SupplementLevel <= @SupplementVersion AND EstimationOverlap.UserAccepted = 1
	LEFT JOIN  
	( 
		SELECT
		  EstimationLineItemsID1
		, EstimationLineItemsID2
		, MIN(Amount) AS Amount
		, MIN(Minimum) AS Minimum
		, MAX(SupplementLevel) AS SupplementLevel
		, MAX(OverlapAdjacentFlag) AS OverlapAdjacentFlag
		FROM 
		(
			SELECT  
				  CASE WHEN LaborSummary1.LaborTotalExtra <= LaborSummary2.LaborTotalExtra THEN EstimationLineItemsID1 ELSE EstimationLineItemsID2 END AS EstimationLineItemsID1 
				, Amount 
				, Minimum 
				, SupplementLevel	SupplementLevel 
				, CASE WHEN CASE WHEN OverlapAdjacentFlag = 'S' THEN 1 ELSE 0 END > 0 THEN 1 ELSE 0 END AS OverlapAdjacentFlag 
				, CASE WHEN OverlapAdjacentFlag = 'S' THEN CASE WHEN LaborSummary1.LaborTotalExtra <= LaborSummary2.LaborTotalExtra THEN EstimationLineItemsID2 ELSE EstimationLineItemsID1 END ELSE 0 END As EstimationLineItemsID2 
			FROM EstimationData  with(nolock)
			JOIN EstimationLineItems with(nolock) ON EstimationData.id = EstimationLineItems.EstimationDataID 
			LEFT JOIN EstimationOverlap with(nolock) ON  
				EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1  
				AND EstimationOverlap.SupplementLevel <= @SupplementVersion
				AND EstimationOverlap.UserAccepted = 1 
				--AND dbo.IsLineDeletedBySupplement(EstimationOverlap.EstimationLineItemsID2, @SupplementVersion) = 0 
 
			LEFT JOIN #LaborSummary LaborSummary1 ON EstimationOverlap.EstimationLineItemsID1 = LaborSummary1.EstimationLineItemsID 
			LEFT JOIN #LaborSummary LaborSummary2 ON EstimationOverlap.EstimationLineItemsID2 = LaborSummary2.EstimationLineItemsID 
 
			WHERE  
				AdminInfoID = @AdminInfoID   
				AND EstimationLineItemsID1 IS NOT NULL 
		) base
		GROUP BY 
		  EstimationLineItemsID1
		, EstimationLineItemsID2
	) EstimationOverlap ON @UseNewOverlapFormula = 1 AND EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1 
  
	-- Old incorrect overlap logic
	LEFT JOIN dbo.GetEstimateOverlap1(@AdminInfoID) AS EstimationOverlap1 ON @UseNewOverlapFormula = 0 AND EstimationOverlap1.EstimationLineItemsID1 = dbo.GetOriginalLineItemID(EstimationLineItems.id) AND EstimationOverlap1.SupplementVersion <= @SupplementVersion
	LEFT JOIN dbo.GetEstimateOverlap2(@AdminInfoID) AS EstimationOverlap2 ON @UseNewOverlapFormula = 0 AND EstimationOverlap2.EstimationLineItemsID2 = dbo.GetOriginalLineItemID(EstimationLineItems.id) AND EstimationOverlap2.SupplementVersion <= @SupplementVersion

	LEFT JOIN #LineItems EstimationLineItemsAssembly ON  EstimationOverlap.EstimationLineItemsID2  = EstimationLineItemsAssembly.id AND EstimationOverlap.OverlapAdjacentFlag = 1  
  
	-- Ezra - 12/13/2019 - added AND clause to not join supplement changes beyond what we are pulling data for - delete this comment in a while if there are no side effects
	
	 LEFT JOIN #LineItems ModifiesData ON ModifiesData.id = EstimationLineItems.ModifiesID   
	
	 LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = AdminInfo.CustomerProfilesID 
	 LEFT JOIN #LineItems SuppOverride ON EstimationLineItems.id = SuppOverride.ModifiesID   --AND SuppOverride.SupplementVersion <= @SupplementVersion  
	 LEFT JOIN #LaborSummary AS LaborSummary ON EstimationLineItems.ID   = LaborSummary.EstimationLineItemsID
 
	WHERE  
		AdminInfo.ID = @AdminInfoID 
 
		AND (ISNULL(EstimationLineItems.SupplementVersion, 0) <= @SupplementVersion) 

	-------------------------------------------------------------------------------------------------------------------------------------------------------- 
	---- Add the PDR lines to the main results 
 
	UNION 
 
	SELECT --DISTINCT 
		  PDRSummary.EstimateDataPanelID As ID 
		, ForEOR AS ForSummary 
		, 0 AS PartSourceModified 
		, 0 AS ModifiedSupplementVersion 
		, 0 AS ModifiedID 
		, PDRSummary.Panel 
		, CASE WHEN PDRSummary.SupplementVersion = @SupplementVersion AND @SupplementVersion > 0 THEN 1 ELSE 0 END AS Added 
		, CASE WHEN ForEOR = 0 AND PDRSummary.SupplementVersion < @SupplementVersion THEN 1 ELSE 0 END AS Removed 
		, '' AS BettermentType 
		, 0 AS BettermentValue 
		, 0 AS BettermentAmount 
		, '' AS BettermentNote 
        , 0 AS PaintMaterialsBetterment
        , 0 AS LaborBetterment
        , 0 AS PaintLaborBetterment
		, 0 AS Sublet 
		, 'PDR Matrix' AS Action 
		, ''  AS OperationDescription
		, PDRSummary.Description 
		, '' AS PartNumber 
		, ISNULL(PDRSummary.Price, 0) AS Price 
		, 1 AS Quantity 
		, '' AS PartSource 
		, 0 AS RemovedPrice 
		, 0 AS RemovedQuantity 
		, '' AS RemovedPartSource 
		, @ImageDirectory + 'empty.gif' AS PartSourceImage 
		, '' AS LaborSummaryPaintPanel 
		, 0 AS LaborTotalPaintPanel 
		, '' AS LaborSummaryPaint 
		, 0 AS LaborTotalPaint 
		, 0 AS LaborTotalPaintDollars
		, 0
		, 0
		, 0
		, '' AS LaborSummaryExtra 
		, 0 AS LaborTotalExtra
		, 0 AS LaborTotalExtraOverlapped
		, '' AS ExtraLaborType
		, '' AS ExtraLaborIncludeInType
		, '' AS LaborExtraRemovedType
		, '' AS LaborExtraRemovedIncludeInType
		, 0 LaborExtraRemovedHours
		, 0 LaborExtraRemovedTotal
		, 0 AS LaborIncluded 
		, 0 AS OverlapSupplement
		, 0 AS OtherCharges 
		, 0 AS OtherChargesLaborType 
		, 0 AS RemovedOtherCharges 
		, 0 AS RemovedOtherChargesLaborType 
		, '' DeductionsMessage 
		, '' AdjacentMessage
		, PDRSummary.Notes 
		, PDRSummary.LineNumber 
		, PDRSummary.SupplementVersion 
		, 0 AS DeletedBySupplement 
		, PDRSummary.SupplementVersion AS SupplementDisplay 
		, '' AS StepSupp 
		, PDRSummary.Sorter		 
		, PDRSummary.GroupNumber 
		, PDRSummary.OversizedDescription AS OversizedDescription 
		, PDRSummary.OversizedPrice AS OversizedPrice 
		, PDRSummary.ModifierDescription AS ModifierDescription 
		, PDRSummary.ModifierPrice AS ModifierPrice
		, '' AS LaborSummaryPaintPanelPrint 
		, '' AS LaborSummaryPaintPrint 
		, '' AS LaborSummaryExtraPrint
		, 0 AS IsPartsQuantity
		, 0 AS IsLaborQuantity
		, 0 AS IsPaintQuantity
		, 1 AS PartsQuantity
		, 1 AS LaborQuantity
		, 1 AS PaintQuantity
	FROM #PDRSummary AS PDRSummary 
	WHERE PDRSummary.AdminInfoID = @AdminInfoID AND PDRSummary.SupplementVersion <= @SupplementVersion 
 
	---- END PDR 
	--------------------------------------------------------------------------------------------------------------------------------------------------------

	IF @PrintPnLNotes = 1
	BEGIN
		CREATE TABLE #ProcessedLinesBarCode
		(
			LineItemID	int,
			SectionID INT,
			Barcode VARCHAR(10)		
		)

		INSERT INTO #ProcessedLinesBarCode(LineItemID,SectionID,Barcode) 
		SELECT	LineItemID, EstimationLineItems.SectionID, EstimationLineItems.Barcode
		FROM	#ProcessedLines PL INNER JOIN EstimationLineItems ON PL.LineItemID = EstimationLineItems.ID

		DECLARE @Service_BarCode1 VarChar(10)
		DECLARE @Year INT  

		 IF @AdminInfoID > 1583497   
		  BEGIN   
		   SELECT   
			  @Service_BarCode1 = Vehicle_Service_Xref.Service_Barcode
			, @Year = Year  
		   FROM VehicleInfo   
		   INNER JOIN EstimationData ON EstimationData.id = VehicleInfo.EstimationDataId   
		   INNER JOIN Vinn.dbo.Vehicle_Service_Xref ON Vehicle_Service_Xref.VehicleID = VehicleInfo.VehicleID   
		   WHERE EstimationData.AdminInfoID = @AdminInfoID   
		  END   
		 ELSE   
		  BEGIN   
		   SELECT @Service_BarCode1 = Mitchell3.Dbo.GetServiceBarcode(@AdmininfoID)  
		  END   

		 UPDATE Details SET Notes = CASE WHEN Details.Action IN ('Replace','R&I ') THEN CASE WHEN ISNULL(Details.Notes, '')= '' THEN REPLACE(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''),'&','&amp;')
										WHEN ISNULL(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''), '')= '' THEN REPLACE(Details.Notes,'&','&amp;')
										ELSE  Details.Notes +  ' <br/> ' + REPLACE(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''),'&','&amp;') END
									ELSE '' END
		 FROM #ProcessedLines Details INNER JOIN #ProcessedLinesBarCode PLBC ON Details.LineItemID = PLBC.LineItemID   
		 LEFT JOIN Mitchell3.dbo.DetailNotes ON DetailNotes.Service_Barcode = @Service_BarCode1 AND DetailNotes.Barcode = PLBC.barcode  
		 LEFT JOIN Mitchell3.dbo.IOHChanges ON IOHChanges.Service_Barcode = @Service_BarCode1 AND IOHChanges.BarcodeIOH = PLBC.barcode 
		 WHERE PLBC.SectionID > 0
	END

	IF @UseNewOverlapFormula = 1 OR @IsImported = 1
		BEGIN
			-- Mark LaborIncluded when the overlapped body labor is <= 0 and the pre-overlapped value > 0
			UPDATE #ProcessedLines
			SET LaborIncluded = 1
			WHERE 
				(
					((@AdminInfoID >= @OtherLaborTypesIncludeFirst OR @IsImported = 1) AND ExtraLaborType IN ('Body' , 'Mechanical', 'Structure'))
					OR (@AdminInfoID < @OtherLaborTypesIncludeFirst AND @IsImported = 0 AND ExtraLaborType = 'Body')
				)
				
				AND LaborTotalExtraOverlapped = 0 
				AND LaborTotalExtra > 0

			-- If the labor total overlapped is < the labor total, use the overlapped value as the labor total extra value
			UPDATE #ProcessedLines
			SET 
				  LaborSummaryExtra = CAST(LaborTotalExtraOverlapped AS VARCHAR) + ' hrs. Body'
				, LaborTotalExtra = LaborTotalExtraOverlapped
			WHERE ExtraLaborType = 'Body' AND LaborTotalExtraOverlapped < LaborTotalExtra
		END

	-- If a panel has the Primary part in the estimate as a repair, replace, or blend, than any R&I lines should be exculted from the overlap calculation.
	-- This query updates the processed lines and sets the labor included to 0 when the above logic is true.
	UPDATE #ProcessedLines
		SET LaborIncluded = 0 
		FROM #ProcessedLines Lines
		JOIN 
		(
			SELECT Panel, MAX(CASE WHEN IsPrimaryPanel = 1 THEN 1 ELSE 0 END) AS PrimaryPanelUsed
			FROM #ProcessedLines Lines
			JOIN  
			(
				SELECT EstimationLineItems.id AS LineItemID, CASE WHEN EstimationLineItems.ActionCode IN ('Repair', 'Refinish', 'Blend') AND ISNULL(Hotspot.Callout_Number, 0) = 1 THEN 1 ELSE 0 END AS IsPrimaryPanel
				FROM EstimationData with(nolock)
				LEFT OUTER JOIN EstimationLineItems with(nolock) ON EstimationLineItems.EstimationDataID = EstimationData.ID
				LEFT OUTER JOIN Mitchell3.dbo.Detail    ON 
					EstimationLineItems.PartNumber <> 'ORDER FROM DEALER' 
					AND Detail.Part_Number = EstimationLineItems.PartNumber
					--AND CASE WHEN EstimationLineItems.VehiclePosition = 'R' THEN 1 ELSE 2 END = Detail.Right_Left_Code
					AND (EstimationLineItems.VehiclePosition = '' OR CASE WHEN EstimationLineItems.VehiclePosition = 'R' THEN 1 ELSE 2 END = Detail.Right_Left_Code)
				LEFT OUTER JOIN Mitchell3.dbo.Hotspot with(nolock) ON 
					Hotspot.Service_Barcode = Detail.Service_BarCode
					AND Hotspot.nheader = Detail.nheader
					AND Hotspot.nsection = Detail.nsection
					AND Hotspot.npart = Detail.npart
				WHERE EstimationData.AdminInfoID = @AdminInfoID
			) AS Panel ON Panel.LineItemID = Lines.LineItemID
			GROUP BY Panel
		) AS PanelUsed
		ON PanelUsed.Panel = Lines.Panel
		WHERE Action = 'R&I' AND PanelUsed.PrimaryPanelUsed = 1


		SELECT * FROM #ProcessedLines

END
END
GO
