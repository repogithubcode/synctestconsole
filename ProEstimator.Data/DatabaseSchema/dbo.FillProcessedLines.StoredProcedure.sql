USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO






--USE [FocusWrite]
--GO

--/****** Object:  StoredProcedure [dbo].[FillProcessedLines]    Script Date: 30-06-2023 12:06:48 ******/
--DROP PROCEDURE [dbo].[FillProcessedLines]
--GO

--/****** Object:  StoredProcedure [dbo].[FillProcessedLines]    Script Date: 30-06-2023 12:06:48 ******/
--SET ANSI_NULLS ON
--GO

--SET QUOTED_IDENTIFIER ON
--GO



-- =============================================
-- Author:		Ezra
-- Create date: 11/18/2021
-- Description: A copy of the GetProcessedLines function to fill a table with results
-- Update file: FillProcessedLines-03.sql
-- [dbo].[FillProcessedLines] 13087883
-- =============================================
CREATE PROCEDURE [dbo].[FillProcessedLines]
	@AdminInfoID				int 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	---------------------------------------------------------------------------------------------------------------------------------------------------
	-- Cache useful data
	---------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @IsImported BIT
	DECLARE @EstimationDataID INT
	DECLARE @MaxSupplement INT

	DECLARE @CustomerProfilesID INT      

	SELECT 
		  @MaxSupplement = EstimationData.LockLevel
		, @IsImported = ISNULL(AdminInfo.IsImported, 0)
		, @EstimationDataID = EstimationData.ID
		, @CustomerProfilesID = AdminInfo.CustomerProfilesID
	FROM AdminInfo
	LEFT OUTER JOIN EstimationData ON AdminInfo.ID = EstimationData.AdminInfoID
	WHERE AdminInfo.ID = @AdminInfoID
	
	DECLARE @PrintProfileID INT = 
	(
		SELECT MAX(ID)
		FROM CustomerProfilePrint 
		WHERE CustomerProfilePrint.CustomerProfilesID = @CustomerProfilesID
	)

	DECLARE @Service_BarCode VarCHar(10) = Mitchell3.dbo.GetServiceBarcode(@AdminInfoID) 

	DECLARE @ClearCoatInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn 
			FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 21 
		) > 0 THEN 1 ELSE 0 END 
	) 

	DECLARE @ClearCoatSuppliesInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn 
			FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 22 
		) > 0 THEN 1 ELSE 0 END 
	) 

	-- Cache Profile Rates, they are joined a lot.
	CREATE TABLE #ProfileRates
	(
		RateType		int,
		Rate			real,
		CapType			tinyint,
		Cap				real,
		Taxable			bit,
		DiscountMarkup	real,
		IncludeIn		tinyint
	)

	INSERT INTO #ProfileRates
	SELECT RateType, Rate, CapType, Cap, Taxable, DiscountMarkup, IncludeIn
	FROM CustomerProfileRates
	WHERE CustomerProfilesID = @CustomerProfilesID

	---------------------------------------------------------------------------------------------------------------------------------------------------
	-- Set up variables used for controlling overlap calculations.  Older estimates and imported estimates use different calculations
	---------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @OverlapRoundingFirst INT = 7163683		-- Set this to the first estimate that should use the 0.001 instead of 0 comparison for including labor
	DECLARE @OtherLaborTypesIncludeFirst INT = 0	-- Before only Body labor was applying the Included overlap logic, but Mechanical and Structure should be too.  Estimates after this ID will include Mech and Structure as well

	DECLARE @UseNewOverlapFormula BIT = 0

	IF @AdminInfoID > @OverlapRoundingFirst		-- Set this to the first estimate that should use the new overlap formula
		SET @UseNewOverlapFormula = 1

	IF @IsImported = 1
		SET @UseNewOverlapFormula =0

	DECLARE @StartTime DATETIME = GETDATE();

	DECLARE @SupplementVersion INT = 0

	DECLARE @AluminumEstimateID INT = (SELECT AluminumEstimateID FROM SiteGlobals WHERE ID = 1)

	-------------------------------------------------------------------------------------------------------------------------------------
	-- Make a table of supplements that have processed lines, only process previous supplements if they don't have processed lines
	-------------------------------------------------------------------------------------------------------------------------------------
	CREATE TABLE #SupplementsCached
	(
		Supplement		int	
	)

	INSERT INTO #SupplementsCached
	SELECT DISTINCT Supplement
	FROM ProcessedLines
	WHERE EstimateID = @AdminInfoID

	WHILE @SupplementVersion <= @MaxSupplement
		BEGIN

			-------------------------------------------------------------------------------------------------------------------------------------
			-- Only process the supplement if it is the most recent or if it hasn't already been processed
			-------------------------------------------------------------------------------------------------------------------------------------
			IF @SupplementVersion = @MaxSupplement OR (SELECT Supplement FROM #SupplementsCached WHERE Supplement = @SupplementVersion) IS NULL 
			BEGIN
			
				-------------------------------------------------------------------------------------------------------------------------------------
				-- Get a temp table with the line items for this estimate so we don't have to query the main table, also cache some detail data not in the line items table.
				-------------------------------------------------------------------------------------------------------------------------------------
				CREATE TABLE #LineItemsCache
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
					ParentLineID INT NULL,
					IsOtherChargesQuantity BIT NULL,
					LaborIncluded BIT NULL,

					-- These are joined in and cached
					OriginalID		int,
					Category		varchar(200),
					SubCategory		varchar(200),
					GroupNumber		int,
					nHeader			int,
					nSection		int,
					Step			varchar(200)
				)

				INSERT INTO #LineItemsCache
				SELECT     
					EstimationLineItems.*,   
					EstimationLineItems.ModifiesID,
					Category.Category,     
					SubCategory.Subcategory,     
					ISNULL   
					(   
						Category.nheader * 256 + SubCategory.nsection   
						, CASE WHEN ISNULL(ParentLineItems.SectionID, EstimationLineItems.SectionID) IN (-1, 0) THEN 100000 ELSE ISNULL(ISNULL(ParentLineItems.SectionID, EstimationLineItems.SectionID), 100000) END    
					) 'GroupNumber',   
					SubCategory.nHeader,     
					SubCategory.nSection,     
					CASE 	    
						WHEN Category.Category IS NULL AND SubCategory.Subcategory IS NOT NULL THEN SubCategory.Subcategory     
 						WHEN Category.Category IS NOT NULL AND SubCategory.Subcategory IS NULL THEN Category.Category     
						WHEN Category.Category = SubCategory.Subcategory THEN Category.Category      
						ELSE Category.Category + ' \ ' + SubCategory.Subcategory     
					END 'Step'     
				FROM EstimationLineItems     
				LEFT JOIN EstimationLineItems ParentLineItems ON EstimationLineItems.ParentLineID = ParentLineItems.ID

				LEFT OUTER JOIN Mitchell3.dbo.Detail  
					ON Detail.Service_BarCode = @Service_BarCode 
					AND EstimationLineItems.Barcode = Detail.barcode    
					-- Limit to 1 join, sometimes there are multiple detail records with same barcodes
					AND Detail.npart = (SELECT MIN(npart) FROM Mitchell3.dbo.Detail WHERE Detail.Service_BarCode = @Service_BarCode AND Detail.barcode = EstimationLineItems.Barcode)
	
				LEFT OUTER JOIN Mitchell3.dbo.SubCategory  
					ON Detail.Service_BarCode = SubCategory.Service_BarCode
					AND Detail.nHeader = SubCategory.nheader
					AND Detail.nsection = SubCategory.nsection
	
				LEFT OUTER JOIN Mitchell3.dbo.Category 
					ON SubCategory.Service_BarCode = Category.Service_BarCode
					AND SubCategory.nHeader = Category.nheader

				WHERE 
					EstimationLineItems.EstimationDataID = @EstimationDataID
					AND ISNULL(EstimationLineItems.SupplementVersion, 0) <= @SupplementVersion

				-- SELECT * FROM #LineItemsCache

				PRINT '#LineItemsCache ' + CAST(DATEDIFF(millisecond, @StartTime, GETDATE()) AS VARCHAR); SET @StartTime = GETDATE();
				-------------------------------------------------------------------------------------------------------------------------------------
	
				DECLARE @ModifiedIDs TABLE
				(
					  LineItemID		int
					, OriginalID		int
				)

				INSERT INTO @ModifiedIDs
				SELECT ID, OriginalID
				FROM #LineItemsCache

				DECLARE @counter INT = 0
				WHILE @counter <= @SupplementVersion
					BEGIN
						UPDATE Base
						SET Base.OriginalID = Joined.OriginalID
						FROM @ModifiedIDs AS Base
						LEFT OUTER JOIN @ModifiedIDs AS Joined ON Base.OriginalID = Joined.LineItemID
						WHERE ISNULL(Joined.OriginalID, -1) > 0 AND Joined.OriginalID <> Base.OriginalID

						SET @counter = @counter + 1
					END

				UPDATE @ModifiedIDs
				SET OriginalID = LineItemID 
				WHERE OriginalID = -1
	
				--SELECT * FROM @ModifiedIDs

				-------------------------------------------------------------------------------------------------------------------------------------
				-- Get a temp table with the labor lines
				-------------------------------------------------------------------------------------------------------------------------------------
				CREATE TABLE #LaborLines  
				(  
					  EstimationLineItemsID		int  
					, ModifiesID				int  
					, SupplementVersion			int  
					, LaborTypeID				tinyint  
					, LaborTypeName				varchar(50)  
					, LaborTime					real  
					, AdjacentDeduction			tinyint
					, DeductionAmount			real  
					, Rate						real  
					, LaborCost					money  
					, Sub						varchar(10)
					, LaborType					tinyint
					, BettermentFlag			BIT
				)  

				DECLARE @bettermentType CHAR 
  
				INSERT INTO #LaborLines     
				SELECT  
					EstimationLineItems.ID,    
					EstimationLineItems.ModifiesID,    
					ISNULL(EstimationLineItems.SupplementVersion, 0) AS SupplementVersion,    
 
					-- The "paint types" are all counted as one when comparing between supplements, on other words changing between paint types doesn't remove the original 
					-- paint type and add the new, only change the difference in time for any paint type. 
					CASE WHEN (EstimationLineLabor.LaborType IN (18, 19, 29)) THEN 16 ELSE EstimationLineLabor.LaborType END AS LaborTypeID,    
 
					CASE 
						WHEN LaborTypes.ID = 5 AND @AdminInfoID > @AluminumEstimateID THEN 'Aluminum'	-- After a point we renamed Detail to Aluminum
						ELSE ISNULL(LaborTypes.LaborType, '') 
					END AS LaborTypeName,    
					ISNULL(EstimationLineLabor.LaborTime, 0) AS LaborTime,   
					EstimationLineLabor.AdjacentDeduction,
					CASE     
						WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction     
						WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction     
						ELSE 0     
					END AS DeductionAmount,    
					ISNULL(ISNULL(CustomerProfileRatesIncludeIn.Rate, CustomerProfileRates.rate), 0) AS Rate,    
					ISNULL(EstimationLineLabor.LaborCost, 0) AS LaborCost,    
					CASE WHEN EstimationLineLabor.SubletFlag <> 0 THEN '(Sub)' ELSE '' END AS Sub,
					EstimationLineLabor.LaborType,
					EstimationLineLabor.BettermentFlag
				FROM #LineItemsCache EstimationLineItems    
				LEFT JOIN EstimationLineLabor ON EstimationLineItems.ID = EstimationLineLabor.EstimationLineItemsID    
				LEFT JOIN LaborTypes ON EstimationLineLabor.LaborType = LaborTypes.ID    
    
				LEFT JOIN #ProfileRates CustomerProfileRates ON LaborTypes.RateTypesID = CustomerProfileRates.RateType 
				LEFT JOIN #ProfileRates CustomerProfileRatesIncludeIn ON CustomerProfileRates.IncludeIn = CustomerProfileRatesIncludeIn.RateType  

				LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = @CustomerProfilesID     
    
				WHERE 
					EstimationLineLabor.LaborType > 0 
					AND (EstimationLineLabor.LaborType NOT IN (20, 21, 22, 26, 27, 28) OR EstimationLineLabor.Include = 1)    
		
 
				--SELECT * FROM #LaborLines
				PRINT '#LaborLines ' + CAST(DATEDIFF(millisecond, @StartTime, GETDATE()) AS VARCHAR); SET @StartTime = GETDATE();
 
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------
				-- Get the labor summary
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------
				CREATE TABLE #LaborSummary
				(  
					  EstimationLineItemsID			int  
					, LaborSummaryPaintPanel		varchar(200)  
					, LaborSummaryPaintPanelPrint	varchar(200)
					, LaborTotalPaintPanel			real  
					, LaborSummaryPaint				varchar(200)  
					, LaborSummaryPaintLaborPrint	varchar(200)
					, LaborTotalPaint				real  
					, LaborTotalPaintDollars		real 
					, LaborTotalClearcoat			real 
					, ClearcoatLaborIncluded		bit 
					, ClearcoatSuppliesIncluded		bit 
					, LaborSummaryExtra				varchar(200)  
					, LaborSummaryExtraPrint		varchar(200)
					, LaborTotalExtra				real  
					, LaborExtraType				varchar(50) 
					, LaborExtraIncludedInType		varchar(50) 
					, LaborExtraRemovedType			varchar(50) 
					, LaborExtraRemovedIncludedInType	varchar(50)  
					, LaborExtraRemovedHours		real  
					, LaborExtraRemovedTotal		real
					, IsPartsQuantity				BIT
					, IsOtherChargesQuantity		BIT
					, IsLaborQuantity				BIT
					, IsPaintQuantity				BIT
					, PartsQuantity					REAL
					, LaborQuantity					REAL
					, PaintQuantity					REAL
					, OtherChargesQuantity			REAL
					, LaborSummaryPaintPanelPrintDescSubText	varchar(200)
					, LaborSummaryExtraPrintDescSubText	varchar(200)
					, PaintMaterialsBetterment      REAL
					, LaborBetterment				REAL
					, PaintLaborBetterment			REAL
				)   

				-- User a cursor to loop through all of the line item IDs    
				DECLARE @cursor CURSOR	  
	
				SET @cursor = CURSOR FOR    
					SELECT DISTINCT EstimationLIneItemsID     
					FROM #LaborLines    
    
				DECLARE @LineLoopID INT    
    
				OPEN @cursor    
				FETCH NEXT FROM @cursor    
				INTO @LineLoopID    
    
				WHILE @@FETCH_STATUS = 0    
				BEGIN    
					DECLARE @LaborSummaryPaintPanel VARCHAR(200) = ''    
					DECLARE @LaborSummaryPaintLabor VARCHAR(200) = ''   
					DECLARE @LaborSummaryExtra VARCHAR(200) = ''   		   

					DECLARE @LaborSummaryPaintPanelPrintDescSubText	VARCHAR(200) = ''    
					DECLARE @LaborSummaryPaintPanelPrint	VARCHAR(200) = ''    
					DECLARE @LaborSummaryPaintLaborPrint	VARCHAR(200) = ''  
					DECLARE @LaborSummaryExtraPrint			VARCHAR(200) = ''   	
					DECLARE @LaborSummaryExtraPrintDescSubText		VARCHAR(200) = ''      
    
					DECLARE @LaborTotalPaintPanel REAL    
					DECLARE @LaborTotalPaintPanelDollars REAL
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
				
					DECLARE @isPartsQuantity		BIT
					DECLARE @isLaborQuantity		BIT
					DECLARE @isPaintQuantity		BIT
					DECLARE @isOtherChargesQuantity	BIT
					DECLARE @partsQuantity			INT
					DECLARE @laborQuantity			INT
					DECLARE @paintQuantity			INT
					DECLARE @otherChargesQuantity	INT

					DECLARE @PaintMaterialsBetterment	REAL = 0    
					DECLARE @LaborBetterment			REAL = 0  
					DECLARE @PaintLaborBetterment		REAL = 0

					DECLARE @quantity INT = (SELECT ISNULL(Qty, 0) FROM #LineItemsCache WHERE id = @LineLoopID)

					SET @bettermentType = (SELECT BettermentType FROM #LineItemsCache WHERE id = @LineLoopID)
		
					DECLARE @modifiedLineItemID INT = (SELECT ISNULL(ModifiesID, -1) FROM #LineItemsCache WHERE id = @LineLoopID) 
					IF (@modifiedLineItemID > 0)
						BEGIN
							SET @quantity = @quantity + dbo.GetLineItemQuantity(@modifiedLineItemID) 
						END

					SELECT 
						  @isPartsQuantity = ISNULL(IsPartsQuantity, 0)
						, @isLaborQuantity = ISNULL(IsLaborQuantity, 0)
						, @isPaintQuantity = ISNULL(IsPaintQuantity, 0)
						, @isOtherChargesQuantity = ISNULL(IsOtherChargesQuantity, 0)
						, @partsQuantity = CASE WHEN IsPartsQuantity = 1 THEN @quantity ELSE 1 END
						, @laborQuantity = CASE WHEN IsLaborQuantity = 1 THEN @quantity ELSE 1 END
						, @paintQuantity = CASE WHEN IsPaintQuantity = 1 THEN @quantity ELSE 1 END
						, @otherChargesQuantity = CASE WHEN IsOtherChargesQuantity = 1 THEN @quantity ELSE 1 END
					FROM #LineItemsCache LineItems
					WHERE ID = @LineLoopID

					-- Paint Panel is the "Paint Time" value in the paint section of a line item.  The LaborType can be different depending on the Paint Type selection 
					-- but they are all considered Paint Panel 
					SELECT @LaborSummaryPaintPanel = @LaborSummaryPaintPanel +     
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
							THEN '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
							ELSE '' 
						END    
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    
		
					SELECT @LaborSummaryPaintPanelPrint = CAST(@LaborSummaryPaintPanelPrint AS VARCHAR) + 
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
							ELSE ''    
						END,
						@LaborSummaryPaintPanelPrintDescSubText = CAST(@LaborSummaryPaintPanelPrintDescSubText AS VARCHAR) + 
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount), 1) AS VARCHAR) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
							ELSE ''    
						END
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)   
						
					SELECT @LaborTotalPaintPanel =     
						ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount)  * @paintQuantity, 0)    
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    

					SELECT @LaborTotalPaintPanelDollars =     
						ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0) 
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)  
		
					-- The clearcoat labor is calculated seperately from the other paint types 
					SELECT @LaborTotalClearcoat =     
						ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0) 
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID = 20 
		
					SELECT @LaborTotalClearcoatDollars =     
						ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0) 
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID = 20 
		 
					-- All other paint types are added together 
					SELECT @LaborSummaryPaintLabor = @LaborSummaryPaintLabor + '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 
		
					SELECT @LaborSummaryPaintLaborPrint = @LaborSummaryPaintLaborPrint +     
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0     
							THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
							ELSE ''    
						END    
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 

					SELECT @LaborTotalPaint = ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0)    
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)     
 
					SELECT @LaborTotalPaintDollars = ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0)  
						FROM #LaborLines AS LaborLines    
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)  
 
					-- Extra labor is the Labor Time value in section 2 of the line item details, grouped by Labor Type. 
					SELECT 
						 @LaborExtraType =	CASE 
							WHEN LaborTypes.ID = 5 AND @AdminInfoID > @AluminumEstimateID THEN 'Aluminum'	-- After a point we renamed Detail to Aluminum
							ELSE ISNULL(LaborTypes.LaborType, '') 
						END
						, @LaborExtraIncludedInType = LaborTypesMainIncludeIn.LaborType 
						FROM #LaborLines AS LaborLines  
						LEFT JOIN LaborTypes ON LaborTypes.id = LaborLines.LaborTypeID 
						LEFT OUTER JOIN #ProfileRates CustomerProfileRates ON CustomerProfileRates.RateType = LaborTypes.RateTypesID 
						LEFT OUTER JOIN LaborTypes LaborTypesMainIncludeIn ON LaborTypesMainIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
    
					SELECT @LaborSummaryExtra = @LaborSummaryExtra + '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '     
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)     
					SELECT @LaborSummaryExtraPrint = @LaborSummaryExtraPrint +    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
							ELSE ''    
						END,
						@LaborSummaryExtraPrintDescSubText = @LaborSummaryExtraPrintDescSubText +    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount), 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
							ELSE ''    
						END 
					FROM #LaborLines AS LaborLines  
					WHERE     
						LaborLines.EstimationLineItemsID = @LineLoopID     
						AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)

					SELECT @LaborTotalExtra =     
						ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 0)    
						FROM #LaborLines AS LaborLines  
						WHERE LaborLines.EstimationLineItemsID = @LineLoopID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
		 
					-- Deleted extra labor.  This is extra labor that was in the previous supplement but not this one 
					SELECT @LaborExtraRemovedType = LaborLines.LaborTypeName   
					FROM #LaborLines LaborLines 
					WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
					SELECT  
						@LaborExtraRemovedType = LaborLines.LaborTypeName 
						, @LaborExtraRemovedIncludedInType = LaborTypesIncludeIn.LaborType 
					FROM #LaborLines LaborLines 
					LEFT JOIN LaborTypes ON LaborLines.LaborTypeID = LaborTypes.id 
					LEFT OUTER JOIN #ProfileRates CustomerProfileRates ON CustomerProfileRates.RateType = LaborTypes.RateTypesID 
					LEFT OUTER JOIN LaborTypes LaborTypesIncludeIn ON LaborTypesIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
					WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
    
					SELECT @LaborExtraRemovedHours =     
						ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount), 0) * -1 
						FROM #LaborLines LaborLines 
						WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
							AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
							AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
					SELECT @LaborExtraRemovedTotal =     
						((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate) * -1  
					FROM #LaborLines LaborLines 
					WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM #LaborLines WHERE EstimationLineItemsID = @LineLoopID) 
						AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
 					IF(@bettermentType = 'P')
					BEGIN 
						SELECT @PaintMaterialsBetterment = (ISNULL(@LaborTotalPaint,0) + ISNULL(@LaborTotalPaintPanel,0)  + ISNULL(@LaborTotalClearcoat,0)) * ProfileRates.Rate
															FROM #ProfileRates ProfileRates WHERE ProfileRates.RateType = 7

						SELECT @PaintLaborBetterment = ISNULL(@LaborTotalPaintDollars,0)  + ISNULL(@LaborTotalPaintPanelDollars,0)  + ISNULL(@LaborTotalClearcoatDollars,0) 

						SELECT @LaborBetterment = CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
															THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * ISNULL(LaborLines.Rate,0)  * @LaborQuantity, 1)    
													   ELSE 0    
												  END 
						FROM #LaborLines AS LaborLines  
						WHERE    
								LaborLines.EstimationLineItemsID = @LineLoopID    
								AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)
					END

					INSERT INTO #LaborSummary (    
						EstimationLineItemsID,    
						LaborSummaryPaintPanel,    
						LaborSummaryPaintPanelPrint,
						LaborTotalPaintPanel,    
						LaborSummaryPaint, 
						LaborSummaryPaintLaborPrint,
						LaborTotalPaint,   
						LaborTotalPaintDollars, 
						LaborTotalClearcoat, 
						ClearcoatLaborIncluded,  
						ClearcoatSuppliesIncluded, 
						LaborSummaryExtra,    
						LaborSummaryExtraPrint,
						LaborTotalExtra, 
						LaborExtraType, 
						LaborExtraIncludedInType, 
						LaborExtraRemovedType, 
						LaborExtraRemovedIncludedInType, 
						LaborExtraRemovedHours, 
						LaborExtraRemovedTotal,
						IsPartsQuantity,
						IsLaborQuantity,
						IsPaintQuantity,
						IsOtherChargesQuantity,
						PartsQuantity,
						LaborQuantity,
						PaintQuantity,
						OtherChargesQuantity,
						LaborSummaryPaintPanelPrintDescSubText,
						LaborSummaryExtraPrintDescSubText,
						PaintMaterialsBetterment, 
						LaborBetterment, 
						PaintLaborBetterment 
					)    
					VALUES    
					(    
						@LineLoopID,    
						SUBSTRING(@LaborSummaryPaintPanel, 0, LEN(@LaborSummaryPaintPanel)),    
						SUBSTRING(@LaborSummaryPaintPanelPrint, 0, LEN(@LaborSummaryPaintPanelPrint)),    
						ROUND(@LaborTotalPaintPanel, 1),    
						SUBSTRING(@LaborSummaryPaintLabor, 0, LEN(@LaborSummaryPaintLabor)),  
						SUBSTRING(@LaborSummaryPaintLaborPrint, 0, LEN(@LaborSummaryPaintLaborPrint)), 
						ROUND(@LaborTotalPaint, 1),   
						ROUND(@LaborTotalPaintDollars + @LaborTotalClearcoatDollars, 2),  
						ROUND(@LaborTotalClearcoat, 1),   
						@ClearCoatInPaint, 
						@ClearCoatSuppliesInPaint, 
						SUBSTRING(@LaborSummaryExtra, 0, LEN(@LaborSummaryExtra)),  
						SUBSTRING(@LaborSummaryExtraPrint, 0, LEN(@LaborSummaryExtraPrint)),
						ROUND(@LaborTotalExtra, 1), 
						ISNULL(@LaborExtraType, ''), 
						ISNULL(@LaborExtraIncludedInType, ''), 
						ISNULL(@LaborExtraRemovedType, ''),  
						ISNULL(@LaborExtraRemovedIncludedInType, ''), 
						ROUND(ISNULL(@LaborExtraRemovedHours, 0), 1), 
						ROUND(ISNULL(@LaborExtraRemovedTotal, 0), 2),
						@isPartsQuantity,
						@isLaborQuantity,
						@isPaintQuantity,
						@isOtherChargesQuantity,
						@partsQuantity,
						@laborQuantity,
						@paintQuantity,
						@otherChargesQuantity,
						SUBSTRING(@LaborSummaryPaintPanelPrintDescSubText, 0, LEN(@LaborSummaryPaintPanelPrintDescSubText)),
						SUBSTRING(@LaborSummaryExtraPrintDescSubText, 0, LEN(@LaborSummaryExtraPrintDescSubText)),
						@PaintMaterialsBetterment, 
						@LaborBetterment, 
						@PaintLaborBetterment
					)    
    
					FETCH NEXT FROM @cursor    
					INTO @LineLoopID    
				END     

				PRINT '#LaborSummary ' + CAST(DATEDIFF(millisecond, @StartTime, GETDATE()) AS VARCHAR); SET @StartTime = GETDATE();

				--SELECT * FROM #LaborSummary
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------
				-- End of GetLaborSummary
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------

	 
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------
				-- PDR Summary
				-------------------------------------------------------------------------------------------------------------------------------------
				-------------------------------------------------------------------------------------------------------------------------------------

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
				LEFT OUTER JOIN PDR_Rate ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_Rate.PanelID = 1 AND PDR_Rate.Size = 9   
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
					LEFT OUTER JOIN #PanelData AS PanelData ON base.PanelID = PanelData.PanelID
					WHERE base.AdminInfoID = @AdminInfoID AND PanelData.PanelID IS NULL AND oversize.SupplementAdded > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
					UNION   
   
					SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, oversize.SupplementDeleted As SupplementVersion, base.LineNumber, base.Description   
					FROM PDR_EstimateDataPanel base   
					JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID  
					LEFT OUTER JOIN #PanelData AS PanelData ON base.PanelID = PanelData.PanelID
					WHERE base.AdminInfoID = @AdminInfoID AND PanelData.PanelID IS NULL AND oversize.SupplementDeleted > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData) 
   
    				DELETE FROM #PanelData WHERE SupplementVersion > @SupplementVersion

				--DROP TABLE #SupplementVersions   

				--SELECT * FROM @PanelData
				---------------------------------------------------------------------------------------------------------------------------------   
	 
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

					--SELECT * FROM #PDRSummary
	
				---------------------------------------------------------------------------------------------------------------------------------   
				---------------------------------------------------------------------------------------------------------------------------------   
				-- END OF PDR
				---------------------------------------------------------------------------------------------------------------------------------   
				---------------------------------------------------------------------------------------------------------------------------------   
	

				---------------------------------------------------------------------------------------------------------------------------------   
				-- Fill the Overlap table
				---------------------------------------------------------------------------------------------------------------------------------   
				CREATE TABLE #EstimationOverlap
				(  
					EstimationLineItemsID1	int,   
					EstimationLineItemsID2	int,   
					Amount					float,
					Minimum					float,
					SupplementLevel			tinyint,
					OverlapAdjacentFlag		int
				) 

				INSERT INTO #EstimationOverlap
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
						FROM #LineItemsCache AS EstimationLineItems
						LEFT JOIN EstimationOverlap ON  
							EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1  
							AND EstimationOverlap.SupplementLevel <= @SupplementVersion
							AND EstimationOverlap.UserAccepted = 1 
							--AND dbo.IsLineDeletedBySupplement(EstimationOverlap.EstimationLineItemsID2, @SupplementVersion) = 0 
 
						LEFT JOIN #LaborSummary LaborSummary1 ON EstimationOverlap.EstimationLineItemsID1 = LaborSummary1.EstimationLineItemsID 
						LEFT JOIN #LaborSummary LaborSummary2 ON EstimationOverlap.EstimationLineItemsID2 = LaborSummary2.EstimationLineItemsID 
 
						WHERE EstimationLineItemsID1 IS NOT NULL 
					) base
					GROUP BY 
					  EstimationLineItemsID1
					, EstimationLineItemsID2


		
				---------------------------------------------------------------------------------------------------------------------------------------------------
				---------------------------------------------------------------------------------------------------------------------------------------------------
				-- Create the processed lines table.
				---------------------------------------------------------------------------------------------------------------------------------------------------
				---------------------------------------------------------------------------------------------------------------------------------------------------
	
				CREATE TABLE #ProcessedLines
				(
					  LineItemID	int
					, ForSummary	bit
					, PartSourceModified	bit
					, ModifiedSupplementVersion	int
					, ModifiedID	int
					, OriginalID	int
					, Panel	varchar(50)
					, Added	bit
					, Removed	bit
					, BettermentType	varchar(20)
					, BettermentValue	float
					, BettermentAmount	float
					, BettermentNote	varchar(8000)
					, Sublet	bit
					, Action	varchar(20)
					, OperationDescription	varchar(100)
					, Description	varchar(200)
					, PartNumber	varchar(50)
					, Price	money
					, PrivePreview money
					, Quantity	int
					, PartSource	varchar(20)
					, RemovedPrice	money
					, RemovedPricePreview money
					, RemovedQuantity	int
					, RemovedPartSource	varchar(20)
					, PartSourceImage	varchar(500)
					, LaborSummaryPaintPanelD	varchar(200)
					, LaborSummaryPaintPanelH	varchar(200)
					, LaborTotalPaintPanel	float
					, LaborSummaryPaintD	varchar(200)
					, LaborSummaryPaintH	varchar(200)
					, LaborTotalPaint	float
					, LaborTotalPaintDollars float
					, LaborTotalClearcoat		real
					, ClearcoatIncluded			bit
					, ClearcoatSuppliesIncluded	bit
					, LaborSummaryExtraD	varchar(200)
					, LaborSummaryExtraH	varchar(200)
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
					, OtherChargesPreview float
					, OtherChargesLaborType	int
					, RemovedOtherCharges	float
					, RemovedOtherChargesLaborType	int
					, DeductionsMessage	varchar(100)
					, AdjacentMessage varchar(20)
					, Notes	varchar(5000)
					, NotesPNL varchar(5000)
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
					, IsPartsQuantity				BIT
					, IsLaborQuantity				BIT
					, IsPaintQuantity				BIT
					, PartsQuantity					REAL
					, LaborQuantity					REAL
					, PaintQuantity					REAL
					, IsOtherChargesQuantity		BIT
					, OtherChargesQuantity			REAL
					, LaborSummaryPaintPanelPrintDescSubText	VARCHAR(200)
					, LaborSummaryExtraPrintDescSubText	VARCHAR(200)
				)

				INSERT INTO #ProcessedLines 
				SELECT  
					EstimationLineItems.ID,
					-- Don't include lines that have been change by a supplement 
					CASE WHEN ISNULL(SuppOverride.ID, 0) > 0 AND ISNULL(EstimationLineItems.SupplementVersion, 0) < @SupplementVersion THEN 0 ELSE 1 END AS ForSummary, 
  
					-- Set a flag if a line has been modified by another line with a different part source, these lines aren't shown in the main list ofline items  
					CASE WHEN EstimationLineItems.id <> EstimationLineItemsModified.id AND EstimationLineItems.PartSource <> EstimationLineItemsModified.PartSource THEN 1 ELSE 0 END AS PartSourceModified,  
					CASE WHEN EstimationLineItems.id <> EstimationLineItemsModified.id THEN ISNULL(EstimationLineItemsModified.SupplementVersion, 0) ELSE 0 END AS ModifiedSupplementVersion,  
					EstimationLineItemsModified.ID AS ModifiedID, 
					OriginalIDs.OriginalID AS OriginalID,
	  
					-- If there is no panel, return ZZZ so it is sorted at the end of the list.  The report will not show ZZZ  
							ISNULL(  
						CASE WHEN @Service_Barcode IS NULL
							THEN
								dbo.GetManualSection(ISNULL(ModifiesData.SectionID, EstimationLineItems.SectionID))
							ELSE
								NULLIF(  
									CASE WHEN ModifiesData.id IS NULL  
										THEN ISNULL(Mitchell3.dbo.GetCategory(@Service_BarCode, CASE WHEN ParentLineItems.SectionID IS NULL THEN EstimationLineItems.SectionID ELSE ParentLineItems.SectionID END), ISNULL(EstimationLineItems.Step, ''))  
									ELSE Mitchell3.dbo.GetCategory(@Service_BarCode, EstimationLineItems.SectionID) 
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
						CASE   
							WHEN EstimationLineItems.BettermentType = 'P'  
							THEN 
								((CASE WHEN ISNULL(EstimationLineItems.BettermentParts, 0) = 1 THEN ISNULL(EstimationLineItems.Price,0) ELSE 0 END) + 
								(CASE WHEN ISNULL(EstimationLineItems.BettermentMaterials, 0) = 1 THEN ISNULL(LaborSummary.PaintMaterialsBetterment,0) ELSE 0 END) + 
								--labor
								(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ISNULL(LaborSummary.LaborBetterment,0) ELSE 0 END) + 
								--paint
								(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ISNULL(LaborSummary.PaintLaborBetterment,0) ELSE 0 END))
	
								*		
								(EstimationLineItems.BettermentValue / 100) * -1  
							ELSE -1  
						END 
					END     
       
					WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'D' THEN    
						((CASE WHEN ISNULL(EstimationLineItems.BettermentParts, 0) = 1 THEN ISNULL(EstimationLineItems.BettermentValue,0) ELSE 0 END) + 
						(CASE WHEN ISNULL(EstimationLineItems.BettermentMaterials, 0) = 1 THEN ISNULL(EstimationLineItems.BettermentValue,0) ELSE 0 END) + 
						--labor
						(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ISNULL(EstimationLineItems.BettermentValue,0) ELSE 0 END) + 
						--paint
						(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ISNULL(EstimationLineItems.BettermentValue,0) ELSE 0 END))

						* -1     
					ELSE 0    
					END AS BettermentAmount, 
  
					CASE    
					WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'P' THEN    
					'Betterment ' + CAST(EstimationLineItems.BettermentValue AS VARCHAR) + '%' +     

					(CASE WHEN ISNULL(EstimationLineItems.BettermentParts,0) = 1 THEN ' Parts' + ',' ELSE '' END) + 
					(CASE WHEN ISNULL(EstimationLineItems.BettermentMaterials,0) = 1 THEN ' Paint Materials' + ',' ELSE '' END) + 
					(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ' Labor' + ',' ELSE '' END) + 
					(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ' Paint Labor' + ',' ELSE '' END)

					WHEN ISNULL(EstimationLineItems.BettermentType, '' ) = 'D' THEN    
					'Betterment $' + CAST(EstimationLineItems.BettermentValue AS VARCHAR)  + 

					(CASE WHEN ISNULL(EstimationLineItems.BettermentParts,0) = 1 THEN ' Parts' + ',' ELSE '' END) + 
					(CASE WHEN ISNULL(EstimationLineItems.BettermentMaterials,0) = 1 THEN ' Paint Materials' + ',' ELSE '' END) + 
					(CASE WHEN EstimationLineLaborMain.LaborType in (1,2,3,4,5,6,8,24,25) AND EstimationLineLaborMain.BettermentFlag = 1 THEN ' Labor' + ',' ELSE '' END) + 
					(CASE WHEN EstimationLineLaborPaint.LaborType in (9,16,18,19,29) AND EstimationLineLaborPaint.BettermentFlag = 1 THEN ' Paint Labor' ELSE '' END)    
					ELSE ''    
					END As BettermentNote,  
  
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
						WHEN EstimationLineItems.ActionCode = 'Replace' OR EstimationLineItems.ActionCode = 'Other' THEN
							CASE   
								WHEN   
									ISNULL(COALESCE(EstimationLineItems.BettermentType, ModifiesData.BettermentType), '') <> ''   
									AND ISNULL(COALESCE(EstimationLineItems.BettermentValue, ModifiesData.BettermentValue), 0) > 0   
									--AND ISNULL(COALESCE(EstimationLineItems.BettermentParts, ModifiesData.BettermentParts), 0) = 1  -- Ezra 6/6 this was in the old query, but BettermentParts never seems to be set to true.  Not sure how this worked...
								THEN '(Bet) ' ELSE '' END +  
							CASE   
								WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) = 'LKQ' AND ISNULL(CustomerProfilesMisc.LKQText, '') <> '' THEN CustomerProfilesMisc.LKQText  
								WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) = 'LKQ' AND ISNULL(CustomerProfilesMisc.LKQText,'')='' THEN '*LKQ*'  
								WHEN COALESCE(EstimationLineItems.PartSource, ModifiesData.PartSource) IN ('After', 'Other', 'Reman') THEN isnull(nullif(COALESCE(EstimationLineItems.SourcePartNumber, ModifiesData.SourcePartNumber),''), COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) )  
								WHEN COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) NOT LIKE '**%' THEN COALESCE(EstimationLineItems.PartNumber, ModifiesData.PartNumber) 
								ELSE ''   
							END 
						ELSE ''
					END	'PartNumber',   
  
					CASE   
						WHEN EstimationOverlap.OverlapAdjacentFlag = 1 THEN 0 
						ELSE EstimationLineItems.Price + (EstimationLineItems.Price * (CustomerProfileRates.DiscountMarkup / 100)) 
					END 
					'Price',   

					CASE   
						WHEN EstimationOverlap.OverlapAdjacentFlag = 1 THEN 0 
						ELSE EstimationLineItems.Price  
					END 
					'PricePreview',   
  
					CASE WHEN EstimationLineItems.Price = 0 THEN null ELSE dbo.GetLineItemQuantity(EstimationLineItems.ID) END 'Quantity',   
					CASE WHEN ISNULL(EstimationLineItems.PartSource, '') != '' THEN ISNULL(EstimationLineItems.PartSource, '') ELSE 'Other' END AS 'PartSource',  
 
					-- If the part source changed, get the full price of the modified part and select it as subtracted 
					CASE  
						WHEN ISNULL(@SupplementVersion, 0) > 0 AND ISNULL(EstimationLineitems.PartSource, '') <> ISNULL(EstimationLineItemsModifies.PartSource, '') AND ISNULL(EstimationLineItemsModifies.PartSource, '') <> '' 
							THEN EstimationLineItems.Price + (EstimationLineItems.Price * (CustomerProfileRatesRemoved.DiscountMarkup / 100)) 
						ELSE 0 
					END AS RemovedPrice, 

					CASE  
						WHEN ISNULL(@SupplementVersion, 0) > 0 AND ISNULL(EstimationLineitems.PartSource, '') <> ISNULL(EstimationLineItemsModifies.PartSource, '') AND ISNULL(EstimationLineItemsModifies.PartSource, '') <> '' 
							THEN EstimationLineItems.Price 
						ELSE 0 
					END AS RemovedPricePreview, 
 
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
						 WHEN EstimationLineItems.PartSource = 'After' THEN 'Aftermarket.gif'  
						 WHEN EstimationLineItems.PartSource = 'LKQ' THEN 'LKQ.gif'  
						 WHEN EstimationLineItems.PartSource = 'Reman' THEN 'Remanufacturered.gif'  
						 ELSE 'empty.gif'  
					END As PartSourceImage,  
  
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintPanel, '')) AS 'LaborSummaryPaintPanel',
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintPanelPrint, '')) AS 'LaborSummaryPaintPanelPrint',
					ISNULL(ROUND(LaborSummary.LaborTotalPaintPanel, 1), 0), 
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaint, '')) AS 'LaborSummaryPaint', 
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintLaborPrint, '')) AS LaborSummaryPaintLaborPrint, 
					ISNULL(ROUND(LaborSummary.LaborTotalPaint, 1), 0), 
					ISNULL(ROUND(LaborSummary.LaborTotalPaintDollars, 2), 0),
					ISNULL(ROUND(LaborSummary.LaborTotalClearcoat, 1), 0),
					ISNULL(LaborSummary.ClearcoatLaborIncluded, ''),
					ISNULL(LaborSummary.ClearcoatSuppliesIncluded, ''),
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryExtra, '')) AS 'LaborSummaryExtra', 
					dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryExtraPrint, '')) AS LaborSummaryExtraPrint,
					ROUND(ISNULL(LaborSummary.LaborTotalExtra, 0), 1) AS LaborTotalExtra,
					ROUND(
						-- For Imported estimates, subtract EstimationLineItems.LaborTime from the body labor
						CASE WHEN @IsImported = 1  THEN
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
					ISNULL(ROUND(LaborSummary.LaborExtraRemovedTotal, 2), 0),

					CASE WHEN @UseNewOverlapFormula = 0 AND @IsImported = 0 THEN
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
 
					ISNULL(EstimationLineLaborOther.LaborCost, 0) + (ISNULL(EstimationLineLaborOther.LaborCost, 0) * (ISNULL(OtherLaborRate.DiscountMarkup, 0) / 100)) AS OtherCharges, 

					ISNULL(EstimationLineLaborOther.LaborCost, 0) AS OtherChargesPreview, 

					ISNULL(EstimationLineLaborOther.LaborTypeID, 0) AS OtherChargesLaborType, 
 
					CASE 
						WHEN ISNULL(EstimationLineLaborOther.LaborTypeID, 0) <> ISNULL(EstimationLineLaborModifiesOther.LaborTypeID, 0) 
							THEN ISNULL(EstimationLineLaborModifiesOther.LaborCost, 0) + (ISNULL(EstimationLineLaborModifiesOther.LaborCost, 0) * (ISNULL(ModifiesOtherLaborRate.DiscountMarkup, 0) / 100))
						ELSE 0 
					END AS RemovedOtherCharges, 
 
					CASE 
						WHEN ISNULL(EstimationLineLaborOther.LaborTypeID, 0) <> ISNULL(EstimationLineLaborModifiesOther.LaborTypeID, 0) 
							THEN ISNULL(EstimationLineLaborModifiesOther.LaborTypeID, 0) 
						ELSE 0 
					END AS RemovedOtherChargesLaborType, 
 
					CASE WHEN ISNULL(EstimationLineLaborPaint.AdjacentDeduction, 0) > 0 THEN 
						cast(EstimationLineLaborPaint.LaborTime AS VARCHAR(20)) 
						+ ' hrs. Paint - ' + 
						CASE  
							WHEN EstimationLineLaborPaint.AdjacentDeduction = 1 THEN FocusWrite.dbo.FormatNumber(CustomerProfilesPaint.AdjacentDeduction,1) + ' hrs. Adjacent'  
							WHEN EstimationLineLaborPaint.AdjacentDeduction = 2 THEN FocusWrite.dbo.FormatNumber(CustomerProfilesPaint.NonAdjacentDeduction,1) + ' hrs. Non Adjacent'  
							ELSE ''  
						END  
						+ ' Deduction' 
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
					'' AS 'NotesPNL',
  
					COALESCE(EstimationLineItems.LineNumber, ModifiesData.LineNumber) As LineNumber,  
  
					ISNULL(EstimationLineItems.SupplementVersion, 0) AS 'SupplementVersion',  
  
					CASE   
						WHEN SuppOverride.ActionDescription LIKE 'Delete%'  
						THEN SuppOverride.SupplementVersion ELSE 0 END 'DeletedBySupplement',  
  
					dbo.Biggest(ISNULL(EstimationLineItems.SupplementVersion, 0), ISNULL(SuppOverride.SupplementVersion, 0)) AS 'SupplementDisplay',
					--ISNULL(COALESCE(EstimationLineItems.SupplementVersion, SuppOverride.SupplementVersion), 0) AS 'SupplementDisplay',  
  
					--ISNULL(EstimationLineItems.Step,t2.step) +'-'+  
					ISNULL(EstimationLineItems.Step,'') +'-'+
					CONVERT(VarChar(3),ISNULL(EstimationLineItems.SupplementVersion,0)) 'StepSupp'  

					, ISNULL(EstimationLineItems.nheader, 1000) * 256 AS Sorter 
					, EstimationLineItems.GroupNumber 
					, '' AS OversizedDescription 
					, 0 AS OversizedPrice 
					, '' AS ModifierDescription 
					, 0 AS ModifierPrice
					,ISNULL(LaborSummary.IsPartsQuantity, EstimationLineItems.IsPartsQuantity)
					,ISNULL(LaborSummary.IsLaborQuantity, EstimationLineItems.IsLaborQuantity)
					,ISNULL(LaborSummary.IsPaintQuantity, EstimationLineItems.IsPaintQuantity)
					,ISNULL(LaborSummary.PartsQuantity, dbo.GetLineItemQuantity(EstimationLineItems.ID))
					,ISNULL(LaborSummary.LaborQuantity, 1)
					,ISNULL(LaborSummary.PaintQuantity, 1)
					,ISNULL(LaborSummary.IsOtherChargesQuantity, EstimationLineItems.IsOtherChargesQuantity)
					,ISNULL(LaborSummary.OtherChargesQuantity, 1)
					,dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryPaintPanelPrintDescSubText, '')) AS 'LaborSummaryPaintPanelPrintDescSubText'
					,dbo.HtmlEncode(ISNULL(LaborSummary.LaborSummaryExtraPrintDescSubText, '')) AS 'LaborSummaryExtraPrintDescSubText'
				FROM #LineItemsCache EstimationLineItems 
				-- TODO - Customer profile sups, not ready to go live
				--LEFT JOIN CustomerProfiles ON CustomerProfiles.id = AdminInfo.CustomerProfilesID AND ISNULL(CustomerProfiles.Supplement, 0) = @SupplementVersion 
				JOIN CustomerProfilePrint ON CustomerProfilePrint.id = @PrintProfileID
				JOIN CustomerProfilesMisc ON CustomerProfilesMisc.CustomerProfilesID = @CustomerProfilesID
				LEFT JOIN #LineItemsCache ParentLineItems ON EstimationLineItems.ParentLineID = ParentLineItems.ID
				LEFT JOIN ActionList ON REPLACE(EstimationLineItems.ActionCode, 'RI', 'R I') = ActionList.ActionShort 
  
				LEFT JOIN EstimationNotes   
					ON EstimationLineItems.ID = EstimationNotes.EstimationLineItemsID
					AND (EstimationNotes.Printed = 1 AND ISNULL(CustomerProfilePrint.PrintPublicNotes, 0) = 1)   
				--LEFT JOIN #LineItemsCache EstimationLineItemsModified ON Focuswrite.Dbo.GetLatestLineItemMod(EstimationLineItems.ID) = EstimationLineItemsModified.id
				LEFT JOIN #LineItemsCache EstimationLineItemsModified ON EstimationLineItems.ID = EstimationLineItemsModified.ModifiesID
				LEFT JOIN #LineItemsCache EstimationLineItemsModifies ON EstimationLineItems.ModifiesID = EstimationLineItemsModifies.ID  
				LEFT JOIN (	        
					SELECT 'LKQ' 'PartSource', 9 'RateType'  
					UNION SELECT 'OEM', 8  
					UNION SELECT 'Retail', 8  
					UNION SELECT 'After', 10  
					UNION SELECT 'Reman', 13  
					UNION SELECT 'Other', 18 	  
				) PartSourceRateTypes ON CASE WHEN ISNULL(EstimationLineItems.PartSource, '') = '' THEN 'Other' ELSE EstimationLineItems.PartSource END = PartSourceRateTypes.PartSource
				LEFT JOIN #ProfileRates CustomerProfileRates ON CustomerProfileRates.RateType = PartSourceRateTypes.RateType   

				LEFT JOIN (	        
					SELECT 'LKQ' 'PartSource', 9 'RateType'  
					UNION SELECT 'OEM', 8  
					UNION SELECT 'Retail', 8  
					UNION SELECT 'After', 10  
					UNION SELECT 'Reman', 13  
					UNION SELECT 'Other', 18 	  
				) PartSourceRemovedRateTypes ON CASE WHEN ISNULL(EstimationLineItemsModifies.PartSource, '') = '' THEN '' ELSE EstimationLineItemsModifies.PartSource END = PartSourceRemovedRateTypes.PartSource
				LEFT JOIN #ProfileRates AS CustomerProfileRatesRemoved ON CustomerProfileRatesRemoved.RateType = PartSourceRemovedRateTypes.RateType
   
				LEFT JOIN #LaborLines EstimationLineLaborPaint   
					ON  EstimationLineItems.ID = EstimationLineLaborPaint.EstimationLineItemsID
					AND  
					(  
						EstimationLineLaborPaint.LaborTypeID in (9,16,19,18,29)  -- Clearcoat, Base Coat, 2 Stage, 3 Stage, 2 Tone 
						OR   
						(EstimationLineLaborPaint.LaborTypeID = 26 AND EstimationLineItems.ActionCode = 'Blend')   
					)  
	
				LEFT JOIN #LaborLines EstimationLineLaborMain ON EstimationLineItems.ID  = EstimationLineLaborMain.EstimationLineItemsID AND EstimationLineLaborMain.LaborTypeID in (1,2,3,4,5,6,8,24,25)
				LEFT JOIN #LaborLines EstimationLineLaborOther ON EstimationLineItems.ID  = EstimationLineLaborOther.EstimationLineItemsID AND EstimationLineLaborOther.LaborTypeID in (13,14,15,30,31)  
				LEFT JOIN LaborTypes OtherLaborType ON EstimationLineLaborOther.LaborTypeID = OtherLaborType.id
				LEFT JOIN #ProfileRates OtherLaborRate ON OtherLaborType.RateTypesID = OtherLaborRate.RateType

				LEFT JOIN #LaborLines EstimationLineLaborModifiesOther ON  EstimationLineItemsModifies.ID = EstimationLineLaborModifiesOther.EstimationLineItemsID AND EstimationLineLaborModifiesOther.LaborTypeID in (13,14,15,30,31) 
				LEFT JOIN LaborTypes ModifiesOtherLaborType ON ModifiesOtherLaborType.id = EstimationLineLaborModifiesOther.LaborTypeID
				LEFT JOIN #ProfileRates ModifiesOtherLaborRate ON ModifiesOtherLaborType.RateTypesID = ModifiesOtherLaborRate.RateType

				-- New correct overlap logoc
				--LEFT JOIN EstimationOverlap ON @UseNewOverlapFormula = 1 AND EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1 AND EstimationOverlap.SupplementLevel <= @SupplementVersion AND EstimationOverlap.UserAccepted = 1
				LEFT JOIN #EstimationOverlap AS EstimationOverlap ON @UseNewOverlapFormula = 1 AND EstimationLineItems.ID = EstimationOverlap.EstimationLineItemsID1 
  
				-- Old incorrect overlap logic
				LEFT JOIN dbo.GetEstimateOverlap1(@AdminInfoID) AS EstimationOverlap1 ON @UseNewOverlapFormula = 0 AND EstimationOverlap1.EstimationLineItemsID1 = dbo.GetOriginalLineItemID(EstimationLineItems.id) AND EstimationOverlap1.SupplementVersion <= @SupplementVersion
				LEFT JOIN dbo.GetEstimateOverlap2(@AdminInfoID) AS EstimationOverlap2 ON @UseNewOverlapFormula = 0 AND EstimationOverlap2.EstimationLineItemsID2 = dbo.GetOriginalLineItemID(EstimationLineItems.id) AND EstimationOverlap2.SupplementVersion <= @SupplementVersion

				LEFT JOIN #LineItemsCache EstimationLineItemsAssembly ON  EstimationOverlap.EstimationLineItemsID2  = EstimationLineItemsAssembly.id AND EstimationOverlap.OverlapAdjacentFlag = 1  
  
				-- Ezra - 12/13/2019 - added AND clause to not join supplement changes beyond what we are pulling data for - delete this comment in a while if there are no side effects
	
				 LEFT JOIN #LineItemsCache ModifiesData ON ModifiesData.id = EstimationLineItems.ModifiesID   
	
				 LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = @CustomerProfilesID 
				 LEFT JOIN #LineItemsCache SuppOverride ON EstimationLineItems.id = SuppOverride.ModifiesID   --AND SuppOverride.SupplementVersion <= @SupplementVersion  
				 LEFT JOIN #LaborSummary AS LaborSummary ON EstimationLineItems.ID   = LaborSummary.EstimationLineItemsID

				 LEFT JOIN @ModifiedIDs AS OriginalIDs ON EstimationLineItems.ID = OriginalIDs.LineItemID
 
				WHERE (ISNULL(EstimationLineItems.SupplementVersion, 0) <= @SupplementVersion) 

				-------------------------------------------------------------------------------------------------------------------------------------------------------- 
				---- Add the PDR lines to the main results 
 
				UNION 
 
				SELECT --DISTINCT 
					  PDRSummary.EstimateDataPanelID As ID 
					, ForEOR AS ForSummary 
					, 0 AS PartSourceModified 
					, 0 AS ModifiedSupplementVersion 
					, 0 AS ModifiedID 
					, 0 AS OriginalID
					, PDRSummary.Panel 
					, CASE WHEN PDRSummary.SupplementVersion = @SupplementVersion AND @SupplementVersion > 0 THEN 1 ELSE 0 END AS Added 
					, CASE WHEN ForEOR = 0 AND PDRSummary.SupplementVersion < @SupplementVersion THEN 1 ELSE 0 END AS Removed 
					, '' AS BettermentType 
					, 0 AS BettermentValue 
					, 0 AS BettermentAmount 
					, '' AS BettermentNote 
					, 0 AS Sublet 
					, 'PDR Matrix' AS Action 
					, ''  AS OperationDescription
					, PDRSummary.Description 
					, '' AS PartNumber 
					, ISNULL(PDRSummary.Price, 0) AS Price 
					, ISNULL(PDRSummary.Price, 0) AS PricePreview
					, 1 AS Quantity 
					, '' AS PartSource 
					, 0 AS RemovedPrice 
					, 0 AS RemovedPricePreview
					, 0 AS RemovedQuantity 
					, '' AS RemovedPartSource 
					, 'empty.gif' AS PartSourceImage 
					, '' AS LaborSummaryPaintPanel 
					, '' AS LaborSummaryPaintPanelPrint 
					, 0 AS LaborTotalPaintPanel 
					, '' AS LaborSummaryPaint 
					, '' AS LaborSummaryPaintPrint 
					, 0 AS LaborTotalPaint 
					, 0 AS LaborTotalPaintDollars
					, 0
					, 0
					, 0
					, '' AS LaborSummaryExtra 
					, '' AS LaborSummaryExtraPrint
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
					, 0 AS OtherChargesPreview
					, 0 AS OtherChargesLaborType 
					, 0 AS RemovedOtherCharges 
					, 0 AS RemovedOtherChargesLaborType 
					, '' DeductionsMessage 
					, '' AdjacentMessage
					, PDRSummary.Notes 
					, '' AS NotesPNL
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
					, 0 AS IsPartsQuantity
					, 0 AS IsLaborQuantity
					, 0 AS IsPaintQuantity
					, 1 AS PartsQuantity
					, 1 AS LaborQuantity
					, 1 AS PaintQuantity
					, 0 AS IsOtherChargesQuantity
					, 1 AS OtherChargesQuantity
					, '' AS LaborSummaryPaintPanelPrintDescSubText 
					, '' AS LaborSummaryExtraPrintDescSubText
				FROM #PDRSummary AS PDRSummary 
				WHERE 
					PDRSummary.AdminInfoID = @AdminInfoID 
					AND PDRSummary.SupplementVersion <= @SupplementVersion 
 
				---- END PDR 
				--------------------------------------------------------------------------------------------------------------------------------------------------------

				-- Update the PnL notes
				CREATE TABLE #ProcessedLinesBarCode
				(
					LineItemID	int,
					SectionID INT,
					Barcode VARCHAR(10)		
				)

				INSERT INTO #ProcessedLinesBarCode(LineItemID, SectionID, Barcode) 
				SELECT	LineItemID, EstimationLineItems.SectionID, EstimationLineItems.Barcode
				FROM	#ProcessedLines ProcessedLines 
				INNER JOIN #LineItemsCache EstimationLineItems ON ProcessedLines.LineItemID = EstimationLineItems.ID

				UPDATE Details 
				SET NotesPNL = CASE 
								WHEN Details.Action IN ('Replace','R&I ') THEN 
									CASE 
										WHEN ISNULL(Details.Notes, '')= '' THEN REPLACE(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''),'&','&amp;')
										WHEN ISNULL(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''), '')= '' THEN REPLACE(Details.Notes,'&','&amp;')
										ELSE REPLACE(REPLACE(ISNULL('<B>Labor time is for '+ IOHChanges.Part_Desc+'</B><BR>','') + DetailNotes.Notes, '·', ''),'&','&amp;') 
									END
								ELSE '' 
							END
					FROM #ProcessedLines Details INNER JOIN #ProcessedLinesBarCode PLBC ON Details.LineItemID = PLBC.LineItemID   
					LEFT JOIN Mitchell3.dbo.DetailNotes ON DetailNotes.Service_Barcode = @Service_BarCode AND DetailNotes.Barcode = PLBC.barcode  
					LEFT JOIN Mitchell3.dbo.IOHChanges ON IOHChanges.Service_Barcode = @Service_BarCode AND IOHChanges.BarcodeIOH = PLBC.barcode 
					WHERE PLBC.SectionID > 0

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
							  LaborSummaryExtraH = CAST(LaborTotalExtraOverlapped AS VARCHAR) + ' hrs. Body'
							, LaborTotalExtra = LaborTotalExtraOverlapped
						WHERE ExtraLaborType = 'Body' AND LaborTotalExtraOverlapped < LaborTotalExtra

						-- If the Force Labor Include check box is checked, force the labor to be included
						UPDATE #ProcessedLines
						SET LaborIncluded = CASE WHEN ISNULL(#LineItemsCache.LaborIncluded, 0) = 1 THEN 1 ELSE #ProcessedLines.LaborIncluded END
						FROM #ProcessedLines  
						LEFT OUTER JOIN #LineItemsCache ON #ProcessedLines.LineItemID = #LineItemsCache.ID
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
							SELECT 
								EstimationLineItems.id AS LineItemID
								, CASE WHEN EstimationLineItems.ActionCode IN ('Repair', 'Refinish', 'Blend') AND ISNULL(Hotspot.Callout_Number, 0) = 1 THEN 1 ELSE 0 END AS IsPrimaryPanel
							FROM #LineItemsCache EstimationLineItems
							LEFT OUTER JOIN Mitchell3.dbo.Detail ON 
								EstimationLineItems.PartNumber <> 'ORDER FROM DEALER' 
								AND EstimationLineItems.PartNumber = Detail.Part_Number
								AND (EstimationLineItems.VehiclePosition = '' OR CASE WHEN EstimationLineItems.VehiclePosition = 'R' THEN 1 ELSE 2 END = Detail.Right_Left_Code)
							LEFT OUTER JOIN Mitchell3.dbo.Hotspot ON 
								Detail.Service_BarCode = Hotspot.Service_Barcode
								AND Detail.nheader = Hotspot.nheader
								AND Detail.nsection = Hotspot.nsection 
								AND Detail.npart = Hotspot.npart
						) AS Panel ON Lines.LineItemID = Panel.LineItemID
						GROUP BY Panel
					) AS PanelUsed
					ON PanelUsed.Panel = Lines.Panel
					WHERE Action = 'R&I' AND PanelUsed.PrimaryPanelUsed = 1

					--SELECT * FROM #LineItemsCache

					---------------------------------------------------------------------------------------------------------------------------------------------------
					-- Delete the existing processed line(s)
					---------------------------------------------------------------------------------------------------------------------------------------------------
					DELETE FROM ProcessedLines 
					WHERE EstimateID = @AdminInfoID AND Supplement = @SupplementVersion

					--SELECT * FROM #ProcessedLines
					INSERT INTO ProcessedLines
					SELECT @AdminInfoID, @SupplementVersion, *
					FROM #ProcessedLines

					DROP TABLE #LaborLines
					DROP TABLE #LaborSummary
					DROP TABLE #LineItemsCache
					DROP TABLE #OversizedResults
					DROP TABLE #OversizedSummary
					DROP TABLE #PanelData
					DROP TABLE #PDRSummary
					DROP TABLE #EstimationOverlap
					DROP TABLE #ProcessedLines
					DROP TABLE #ProcessedLinesBarCode

			END		-- End of IF statement where processing happens

			SET @SupplementVersion = @SupplementVersion + 1

		END			-- End of supplement number loop

		EXEC UpdateAdminTotals @AdminInfoID = @AdminInfoID
	/*
	DROP TABLE #ProfileRates
	DROP TABLE #SupplementsCached
	*/
END
GO
