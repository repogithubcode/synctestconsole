USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- [dbo].[EstimateReport_GetTotals] 13087878
CREATE PROCEDURE [dbo].[EstimateReport_GetTotals]
	@AdminInfoID				INT,  
	@SupplementVersion			INT = 0,  
	@ForEOR						BIT = 1,  
	@AlwaysIncludeGT			BIT = 0,	-- By default don't show the Grant Total if it's the same as the Net Total.   
	@PdrOnly					INT = 0,
	@AddOnsOnly					BIT = 0,
	@IncludeShowTopLineField	BIT = 0
AS  
BEGIN  
  
	 -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.   
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component   
	 IF 1=2 BEGIN   
       SET FMTONLY OFF   
     END   
	  
	 -- Get the sub totals  
	create table #EstimateSubTotals    
	( 
		RateName				varchar(50),   
		FinalLineItemTotal		float,   
		Taxable					bit,   
		Rate					float,   
		Hours					float,   
		SortOrder				varchar(50), 
		Notes					varchar(300), 
		CapType					int, 
		Cap						real,
		DirectLineItemTotal				real,
		DiscountMarkupLineItemTotal		real
	) 
 
	IF @ForEOR = 1  
		BEGIN 
			--insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'GESTF')
			INSERT INTO #EstimateSubTotals 
			SELECT *  
			FROM dbo.GetEstimateSubTotalsFunction(@AdminInfoID, @SupplementVersion, @PdrOnly, @AddOnsOnly) 
			WHERE FinalLineItemTotal > 0  
				OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')	-- Get negative "other charges" lines 
		END 
	ELSE 
		BEGIN 
 
			create table #CurrentSubTotals    
			( 
				RateName				varchar(50),   
				FinalLineItemTotal		float,   
				Taxable					bit,   
				Rate					float,   
				Hours					float,   
				SortOrder				varchar(50), 
				Notes					varchar(300), 
				CapType					int, 
				Cap						real,
				DirectLineItemTotal				real,
				DiscountMarkupLineItemTotal		real 
			) 
 
			create TABLE #PreviousSubTotals   
			( 
				RateName				varchar(50),   
				FinalLineItemTotal		float,   
				Taxable					bit,   
				Rate					float,   
				Hours					float,   
				SortOrder				varchar(50), 
				Notes					varchar(300), 
				CapType					int, 
				Cap						real,
				DirectLineItemTotal				real,
				DiscountMarkupLineItemTotal		real 
			) 
			--insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'GESTF')
			INSERT INTO #CurrentSubTotals 
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion, @PdrOnly, @AddOnsOnly) 
				WHERE FinalLineItemTotal > 0  
				OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')	-- Get negative "other charges" lines 
			 --insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'GESTF')
			INSERT INTO #PreviousSubTotals 
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion - 1, @PdrOnly, @AddOnsOnly)  
				WHERE FinalLineItemTotal > 0  
				OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')	-- Get negative "other charges" lines 
 
 
			INSERT INTO #EstimateSubTotals 
			SELECT *,0,0 
			FROM  
			( 
				SELECT  
				  ISNULL(Base.RateName, Previous.RateName) AS RateName 
				, CASE  
					WHEN Previous.RateName IS NULL THEN 
						Base.FinalLineItemTotal 
					ELSE 
						CASE WHEN Base.FinalLineItemTotal <> 0 THEN Base.FinalLineItemTotal ELSE 0 END - ISNULL(Previous.FinalLineItemTotal, 0) 
					END 
				  AS FinalLineItemTotal 
				, ISNULL(Base.Taxable, Previous.Taxable) AS Taxable 
				, ISNULL(Base.Rate, Previous.Rate) AS Rate 
				, CASE  
					WHEN Previous.RateName IS NULL THEN 
						Base.Hours 
					ELSE 
						CASE WHEN Base.Hours > 0 THEN Base.Hours ELSE 0 END - ISNULL(Previous.Hours, 0) 
					END 
				  AS Hours 
				, ISNULL(Base.SortOrder, Previous.SortOrder) AS SortOrder 
				, ISNULL(Base.Notes, Previous.Notes) AS Notes 
				, ISNULL(Base.CapType, Previous.CapType) AS CapType 
				, ISNULL(Base.Cap, Previous.Cap) AS Cap 
				FROM #CurrentSubTotals AS Base 
				FULL OUTER JOIN #PreviousSubTotals AS Previous ON Previous.RateName = Base.RateName 
			 
			) AS Base 
			WHERE FinalLineItemTotal <> 0 OR Hours <> 0 
		END 

	-- Get the tax rates for the estimate  
	DECLARE @TaxRate REAL  
	DECLARE @TaxRate2 REAL  
	DECLARE @TaxRate2Start REAL  

	DECLARE @TaxSeparately BIT
	DECLARE @PartsTaxRate REAL
  
	SELECT  
		@TaxRate = ISNULL(CustomerProfilesMisc.TaxRate, 0),   
		@TaxRate2 = ISNULL(CustomerProfilesMisc.SecondTaxRate, 0),   
		@TaxRate2Start = ISNULL(CustomerProfilesMisc.SecondTaxRateStart, 0),
		@TaxSeparately = ISNULL(CustomerProfilesMisc.UseSepPartLaborTax, 0),
		@PartsTaxRate = ISNULL(CustomerProfilesMisc.PartTax, 0)
	FROM AdminInfo   
	INNER JOIN CustomerProfiles with(nolock) ON CustomerProfiles.id = AdminInfo.CustomerProfilesID       
	INNER JOIN CustomerProfilesMisc ON CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id 
	WHERE AdminInfo.ID = @AdminInfoID  
  
	-- Get the taxable and non taxable totals as variables for later use  
	DECLARE @TaxableTotal MONEY = 0
	DECLARE @PartsTaxableTotal MONEY = 0

	DECLARE @Tax MONEY = 0  
	DECLARE @TaxRateDescription VARCHAR(100) = ''  
	DECLARE @PartsTax MONEY = 0

	IF @TaxSeparately = 1
		BEGIN
			-- Per the fix on task 292 - Tax Setting on rate profile omitting Sublet charges - (ZD# 502)
			-- The total was set as follows and was changed to the next line in order to include Sublet lines
			--SET @TaxableTotal = (SELECT ISNULL(SUM(FinalLineItemTotal), 0) FROM #EstimateSubTotals WHERE Taxable = 1 AND (RateName LIKE '%Labor' OR RateName = 'PDR'))  
			SET @TaxableTotal = (SELECT ISNULL(SUM(FinalLineItemTotal), 0) FROM #EstimateSubTotals WHERE Taxable = 1 AND NOT (RateName LIKE '%Parts' OR RateName LIKE '%Supplies'))  

			SET @PartsTaxableTotal = (SELECT ISNULL(SUM(FinalLineItemTotal), 0) FROM #EstimateSubTotals WHERE Taxable = 1 AND (RateName LIKE '%Parts' OR RateName LIKE '%Supplies'))  

			SET @Tax = (@TaxRate / 100) * @TaxableTotal 
			SET @PartsTax = (@PartsTaxRate / 100) * @PartsTaxableTotal 
		END
	ELSE
		BEGIN
			SET @TaxableTotal = (SELECT ISNULL(SUM(FinalLineItemTotal), 0) FROM #EstimateSubTotals WHERE Taxable = 1)  

			IF @TaxRate2 > 0 AND @TaxRate2Start > 0 AND @TaxableTotal > @TaxRate2Start  
				BEGIN  
					SET @Tax = ((@TaxRate / 100) * @TaxRate2Start) + ((@TaxRate2 / 100) * (@TaxableTotal - @TaxRate2Start))  
					SET @TaxRateDescription = CAST(@TaxRate AS Varchar) + '% for first $' + dbo.FormatMoney(@TaxRate2Start) + ', ' + CAST(@TaxRate2 AS VARCHAR) + '% after'  
				END  
			ELSE  
				BEGIN  
					SET @Tax = (@TaxRate / 100) * @TaxableTotal  
					SET @TaxRateDescription = CAST(@TaxRate AS Varchar) + '%'  
				END

		END
	
	DECLARE @NonTaxableTotal MONEY = (SELECT ISNULL(SUM(FinalLineItemTotal), 0) FROM #EstimateSubTotals WHERE Taxable = 0)  
	DECLARE @Deductible MONEY = (SELECT ISNULL(Deductible, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID)  
  
	  
  
	------------------------------------------------------------------------------------------------------------------------------------- 
	-- Sum up the betterment total  
 
	DECLARE @ProcessedLines TABLE
	(
		[ID] [int] NOT NULL,
		[EstimateID] [int] NULL,
		[Supplement] [int] NULL,
		[LineItemID] [int] NULL,
		[ForSummary] [bit] NULL,
		[PartSourceModified] [bit] NULL,
		[ModifiedSupplementVersion] [int] NULL,
		[ModifiedID] [int] NULL,
		[OriginalID] [int] NULL,
		[Panel] [varchar](50) NULL,
		[Added] [bit] NULL,
		[Removed] [bit] NULL,
		[BettermentType] [varchar](20) NULL,
		[BettermentValue] [float] NULL,
		[BettermentAmount] [float] NULL,
		[BettermentNote] [varchar](8000) NULL,
		[Sublet] [bit] NULL,
		[Action] [varchar](20) NULL,
		[OperationDescription] [varchar](100) NULL,
		[Description] [varchar](200) NULL,
		[PartNumber] [varchar](50) NULL,
		[Price] [money] NULL,
		[PricePreview] [money] NULL,
		[Quantity] [int] NULL,
		[PartSource] [varchar](20) NULL,
		[RemovedPrice] [money] NULL,
		[RemovedPricePreview] [money] NULL,
		[RemovedQuantity] [int] NULL,
		[RemovedPartSource] [varchar](20) NULL,
		[PartSourceImage] [varchar](500) NULL,
		[LaborSummaryPaintPanelD] [varchar](200) NULL,
		[LaborSummaryPaintPanelH] [varchar](200) NULL,
		[LaborTotalPaintPanel] [float] NULL,
		[LaborSummaryPaintD] [varchar](200) NULL,
		[LaborSummaryPaintH] [varchar](200) NULL,
		[LaborTotalPaint] [float] NULL,
		[LaborTotalPaintDollars] [float] NULL,
		[LaborTotalClearcoat] [real] NULL,
		[ClearcoatIncluded] [bit] NULL,
		[ClearcoatSuppliesIncluded] [bit] NULL,
		[LaborSummaryExtraD] [varchar](200) NULL,
		[LaborSummaryExtraH] [varchar](200) NULL,
		[LaborTotalExtra] [float] NULL,
		[LaborTotalExtraOverlapped] [float] NULL,
		[ExtraLaborType] [varchar](20) NULL,
		[ExtraLaborIncludeIn] [varchar](20) NULL,
		[LaborExtraRemovedType] [varchar](50) NULL,
		[LaborExtraRemovedIncludeInType] [varchar](50) NULL,
		[LaborExtraRemovedHours] [float] NULL,
		[LaborExtraRemovedTotal] [float] NULL,
		[LaborIncluded] [bit] NULL,
		[OverlapSupplement] [int] NULL,
		[OtherCharges] [float] NULL,
		[OtherChargesPreview] [float] NULL,
		[OtherChargesLaborType] [int] NULL,
		[RemovedOtherCharges] [float] NULL,
		[RemovedOtherChargesLaborType] [int] NULL,
		[DeductionsMessage] [varchar](100) NULL,
		[AdjacentMessage] [varchar](20) NULL,
		[Notes] [varchar](5000) NULL,
		[NotesPNL] [varchar](5000) NULL,
		[LineNumber] [int] NULL,
		[SupplementVersion] [int] NULL,
		[DeletedBySupplement] [int] NULL,
		[SupplementDisplay] [int] NULL,
		[StepSupp] [varchar](500) NULL,
		[Sorter] [int] NULL,
		[GroupNumber] [int] NULL,
		[OversizedDescription] [varchar](200) NULL,
		[OversizedPrice] [money] NULL,
		[ModifierDescription] [varchar](200) NULL,
		[ModifierPrice] [money] NULL,
		[IsPartsQuantity] [bit] NULL,
		[IsLaborQuantity] [bit] NULL,
		[IsPaintQuantity] [bit] NULL,
		[PartsQuantity] [real] NULL,
		[LaborQuantity] [real] NULL,
		[PaintQuantity] [real] NULL,
		[IsOtherChargesQuantity] [bit] NULL,
		[OtherChargesQuantity] [real] NULL,
		[LaborSummaryPaintPanelPrintDescSubText] VARCHAR(200) NULL,
		[LaborSummaryExtraPrintDescSubText] VARCHAR(200) NULL
	) 

	INSERT INTO @ProcessedLines
	SELECT *
	FROM ProcessedLines 
	WHERE 
		EstimateID = @AdminInfoID
		AND SupplementVersion <= @SupplementVersion
		AND	((@PdrOnly = 1 AND Action IN ('PDR Matrix','PDR')) OR (@PdrOnly = 0 AND Action IN (Action)))

	--UPDATE A SET A.BettermentAmount = B.BettermentAmount
	--FROM @ProcessedLines A INNER JOIN dbo.GetProcessedLines(@AdminInfoID,@SupplementVersion,'',0,0) B ON A.LineItemID = B.LineItemID

	DECLARE @Betterment MONEY =   
	(  
		--SELECT SUM(ISNULL(Modified.BettermentAmount, 0) - ABS(ISNULL(Base.BettermentAmount, 0))) AS BettermentAmount  
		--FROM #ProcessedLines Base  
		--LEFT OUTER JOIN #ProcessedLines Modified ON Base.LineItemID = Modified.ModifiedID AND Modified.LineItemID <> Base.LineItemID  
		--WHERE Base.ForSummary = 1  

		SELECT SUM(ISNULL(Base.BettermentAmount, 0)) AS BettermentAmount  
		FROM @ProcessedLines Base  
		WHERE (@ForEOR = 1 AND Base.Supplement = @SupplementVersion) OR (@ForEOR = 0 AND Base.SupplementVersion = @SupplementVersion)
	)
	-- Calculate the grand and net totals  
	DECLARE @SubTotal MONEY = @TaxableTotal + @NonTaxableTotal + @Tax + @PartsTax + @PartsTaxableTotal
	DECLARE @NetTotal MONEY = @TaxableTotal + @NonTaxableTotal + @Tax + @PartsTaxableTotal + @PartsTax + (CASE WHEN @ForEOR = 1 THEN @Deductible * -1 ELSE 0 END) + @Betterment	  
  
	DECLARE @IsCreditCardFeeTaxed BIT
	DECLARE @CreditCardFeePct Decimal(18,2)
	DECLARE @CreditCardFeeCalcAmount MONEY
	DECLARE @CreditCardFeeTax Decimal(18,2)

	SELECT  
		@IsCreditCardFeeTaxed = TaxedCreditCardFee
		, @CreditCardFeePct = ISNULL(CreditCardFeePercentage, 0)
	FROM EstimationData 
	WHERE AdminInfoID = @AdminInfoID;

	SET @CreditCardFeeCalcAmount = (@SubTotal * @CreditCardFeePct) / 100
	SET @CreditCardFeeTax = CASE WHEN @IsCreditCardFeeTaxed = 1 THEN (@CreditCardFeeCalcAmount * ISNULL(@TaxRate, 0) / 100) ELSE 0 END

	DECLARE @GrandTotal MONEY = @SubTotal + @CreditCardFeeCalcAmount + @CreditCardFeeTax
	SET @NetTotal = @NetTotal + @CreditCardFeeCalcAmount + @CreditCardFeeTax

	-------------------------------------------------------------------------------------------------------------------------------  
	-- Union together the final table  
	-------------------------------------------------------------------------------------------------------------------------------  

	DECLARE @Results TABLE
	(
		[SortOrder]		INT,
		[Type]			VARCHAR(200),
		[Note]			VARCHAR(200),
		[Amount]		MONEY,
		[ShowTopLine]	BIT
	)

	INSERT INTO @Results
	SELECT * FROM (  
		SELECT   
			1 AS SortOrder,  
			CASE WHEN @TaxSeparately = 1 THEN 'Labor ' ELSE '' END + 'Taxable Amount' AS Type,  
			'' As Note,  
			@TaxableTotal AS Amount,
			CASE WHEN @TaxableTotal > 0 
				THEN 1 ELSE 0 END AS ShowTopLine
			--WHERE @TaxableTotal > 0
  
		UNION SELECT  
			2 AS SortOrder,  
			CASE WHEN @TaxSeparately = 1 THEN 'Labor ' ELSE '' END + 'Tax' AS Type,  
			CASE WHEN @TaxSeparately = 1 THEN CASE WHEN @TaxRate > 0 THEN CAST(@TaxRate AS VARCHAR) + '%' ELSE '' END ELSE @TaxRateDescription END AS Note,  
			@Tax AS Amount,
			CASE WHEN @TaxableTotal = 0 
						AND @Tax > 0 
				THEN 1 ELSE 0 END AS ShowTopLine
			--WHERE @Tax > 0

		UNION SELECT   
			3 AS SortOrder,  
			'Parts Taxable Amount' AS Type,  
			'' As Note,  
			@PartsTaxableTotal AS Amount,
			CASE WHEN @TaxableTotal = 0 
						AND @Tax = 0 
						AND (@TaxSeparately = 1 AND @PartsTax > 0) 
				THEN 1 ELSE 0 END AS ShowTopLine
			WHERE @TaxSeparately = 1 AND @PartsTax > 0
  
		UNION SELECT  
			4 AS SortOrder,  
			'Parts Tax' AS Type,  
			CASE WHEN @TaxSeparately = 1 THEN CASE WHEN @PartsTaxRate > 0 THEN CAST(@PartsTaxRate AS VARCHAR) + '%' ELSE '' END ELSE '' END AS Note, 
			@PartsTax AS Amount,
			0 AS ShowTopLine -- same condition as Parts Taxable Amount, so if shown there, will never show top line here
			WHERE @TaxSeparately = 1 AND @PartsTax > 0
  
		UNION SELECT   
			5 AS SortOrder,  
			'Nontaxable Amount' AS Type,  
			'' As Note,  
			@NonTaxableTotal AS Amount,
			CASE WHEN @TaxableTotal = 0 
						AND @Tax = 0 
						AND (@TaxSeparately = 0 OR @PartsTax = 0) 
						AND @NonTaxableTotal <> 0
				THEN 1 ELSE 0 END AS ShowTopLine
		WHERE @NonTaxableTotal <> 0  

		UNION SELECT   
			6 AS SortOrder,  
			'Sub Total' AS Type,  
			'' As Note,  
			@SubTotal AS Amount,
			CASE WHEN @TaxableTotal = 0 
						AND @Tax = 0 
						AND (@TaxSeparately = 0 OR @PartsTax = 0) 
						AND @NonTaxableTotal = 0
						AND @CreditCardFeePct > 0
				THEN 1 ELSE 0 END AS ShowTopLine
			WHERE @CreditCardFeePct > 0

		UNION SELECT   
			7 AS SortOrder,  
			'Processing Fee' AS Type,  
			CAST(CAST (@CreditCardFeePct AS FLOAT) AS VARCHAR) + '%' As Note,  
			@CreditCardFeeCalcAmount AS Amount,
			0 AS ShowTopLine -- same condition as Sub Total, so if shown there, will never show top line here
			WHERE @CreditCardFeePct > 0

		UNION SELECT   
			8 AS SortOrder,  
			'Processing Fee Tax' AS Type,  
			CAST(@TaxRate AS VARCHAR) + '%' AS Note,  
			@CreditCardFeeTax AS Amount,
			0 AS ShowTopLine -- same condition as Sub Total, so if shown there, will never show top line here
			WHERE @IsCreditCardFeeTaxed = 1 AND @CreditCardFeePct > 0

		UNION SELECT   
			9 AS SortOrder,  
			'Grand Total' AS Type,  
			'' As Note,  
			@GrandTotal AS Amount ,
			CASE WHEN @TaxableTotal = 0 
						AND @Tax = 0 
						AND (@TaxSeparately = 0 OR @PartsTax = 0) 
						AND @NonTaxableTotal = 0
						AND @CreditCardFeePct = 0
				THEN 1 ELSE 0 END AS ShowTopLine

		UNION SELECT   
			11 AS SortOrder,  
			'Less Deductible' AS Type,  
			'' AS Note,  
			@Deductible * -1 AS Amount,
			CASE WHEN @ForEOR = 1 AND ISNULL(@Deductible, 0) > 0
				THEN 1 ELSE 0 END AS ShowTopLine
		WHERE @ForEOR = 1 AND ISNULL(@Deductible, 0) > 0  
  
		UNION SELECT  
			12 AS SortOrder,  
			'Betterment' AS Type,  
			'' AS Note,  
			@Betterment AS Amount,
			CASE WHEN (@ForEOR = 0 OR ISNULL(@Deductible, 0) = 0)
						AND @Betterment <> 0
				THEN 1 ELSE 0 END AS ShowTopLine
		WHERE @Betterment <> 0  
		
		UNION SELECT   
			13 AS SortOrder,  
			'Net Total' AS Type,  
			'' As Note,  
			@NetTotal AS Amount,
			CASE WHEN (@ForEOR = 0 OR ISNULL(@Deductible, 0) = 0)
						AND @Betterment = 0
						AND (@AlwaysIncludeGT = 1 OR @GrandTotal <> @NetTotal)
				THEN 1 ELSE 0 END AS ShowTopLine
		WHERE @AlwaysIncludeGT = 1 OR @GrandTotal <> @NetTotal
	) Base  
  
	IF (@IncludeShowTopLineField = 1)
	BEGIN
		SELECT		[SortOrder],
					[Type],
					[Note],
					[Amount],
					[ShowTopLine]
		FROM @Results
		ORDER BY	SortOrder 
	END
	ELSE
	BEGIN
		SELECT		[SortOrder],
					[Type],
					[Note],
					[Amount]
		FROM @Results
		ORDER BY	SortOrder 
	END

END
GO
