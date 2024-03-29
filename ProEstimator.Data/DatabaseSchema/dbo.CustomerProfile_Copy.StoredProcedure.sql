USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[CustomerProfile_Copy] 
	  @ProfileToCopy		int 
	, @LoginID				int 
	, @AdminInfoID			int = 0 
	, @NewProfileName		varchar(50) = null 
	, @NewProfileDescription  varchar(500) = null 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    -- Copy the base customer profile record 
	INSERT INTO [dbo].[CustomerProfiles] 
    ( 
		 [OwnerID] 
        ,[OwnerType] 
        ,[ProfileName] 
        ,[Description] 
        ,[DefaultFlag] 
        ,[CreationDate] 
        ,[CreatedBy] 
        ,[OriginalID] 
        ,[AdminInfoID] 
        ,[DefaultPreset] 
        ,[Deleted] 
        ,[CapRatesAfterInclude] 
        ,[CapSuppliesAfterInclude]
	 ) 
	 SELECT 
		@LoginID 
		,OwnerType 
		,ISNULL(@NewProfileName, ProfileName) 
		,ISNULL(@NewProfileDescription, Description) 
		,0 
		,GETDATE() 
		,@LoginID 
		,id 
		,@AdminInfoID 
		,0 
		,0 
		,CapRatesAfterInclude 
		,CapSuppliesAfterInclude
    FROM CustomerProfiles   
    WHERE ID = @ProfileToCopy  
 
	DECLARE @NewProfileID INT = CAST(SCOPE_IDENTITY() AS INT)  
 
	-- Copy the Presets 
	EXECUTE DuplicatePresets @FromID = @ProfileToCopy, @ToID = @NewProfileID  
 
	-- Copy the Print table 
	INSERT INTO [dbo].[CustomerProfilePrint] 
    ( 
		 [CustomerProfilesID] 
        ,[GraphicsQuality] 
        ,[NoHeaderLogo] 
        ,[NoInsuranceSection] 
        ,[NoPhotos] 
        ,[FooterText] 
        ,[PrintPrivateNotes] 
        ,[PrintPublicNotes] 
        ,[ContactOption] 
        ,[SupplementOption] 
        ,[OrderBy] 
        ,[UseBigLetters] 
        ,[SeparateLabor] 
        ,[EstimateNumber] 
        ,[AdminInfoID] 
        ,[Dollars] 
        ,[GreyBars] 
        ,[NoVehicleAccessories] 
        ,[PrintPaymentInfo] 
        ,[PrintEstimator] 
		,[PrintVendors]
		,[NoFooterDateTimeStamp]
	 ) 
     SELECT 
         @NewProfileID 
        ,GraphicsQuality 
        ,NoHeaderLogo 
        ,NoInsuranceSection 
        ,NoPhotos 
        ,FooterText 
        ,PrintPrivateNotes 
        ,PrintPublicNotes 
        ,ContactOption 
        ,SupplementOption 
        ,OrderBy 
        ,UseBigLetters 
        ,SeparateLabor 
        ,EstimateNumber 
        ,AdminInfoID 
        ,Dollars 
        ,GreyBars 
        ,NoVehicleAccessories 
        ,PrintPaymentInfo 
        ,PrintEstimator 
		,PrintVendors 
		,NoFooterDateTimeStamp
	FROM CustomerProfilePrint 
	WHERE CustomerProfilesID = @ProfileToCopy 
 
	-- Copy the Rates 
	INSERT INTO [dbo].[CustomerProfileRates] 
    ( 
		 [CustomerProfilesID] 
        ,[RateType] 
        ,[Rate] 
        ,[CapType] 
        ,[Cap] 
        ,[Taxable] 
        ,[DiscountMarkup] 
        ,[IncludeIn] 
	) 
    SELECT 
         @NewProfileID 
        ,RateType 
        ,Rate 
        ,CapType 
        ,Cap 
        ,Taxable 
        ,DiscountMarkup 
        ,IncludeIn 
	FROM CustomerProfileRates 
	WHERE CustomerProfilesID = @ProfileToCopy 
 
	-- Copy the Misc table 
	INSERT INTO [dbo].[CustomerProfilesMisc] 
    ( 
		 [CustomerProfilesID] 
        ,[TaxRate] 
        ,[SecondTaxRateStart] 
        ,[SecondTaxRate] 
        ,[ChargeForRadiatorRefilling] 
        ,[RadiatorChargeAmount] 
        ,[ChargeForACRefilling] 
        ,[ACChargeHow] 
        ,[ACChargeAmount] 
        ,[LKQText] 
        ,[TotalLossPerc] 
        ,[IncludeStructureInBody] 
        ,[ChargeForAimingHeadlights] 
        ,[ChargeForPowerUnits] 
        ,[ChargeForRefrigRecovery] 
        ,[SuppressAddRelatedPrompt] 
        ,[ChargeSuppliesOnAllOperations] 
        ,[SuppLevel] 
        ,[DoNotMarkChanges] 
        ,[UseSepPartLaborTax] 
        ,[PartTax] 
        ,[LaborTax] 
	) 
    SELECT 
         @NewProfileID 
        ,TaxRate 
        ,SecondTaxRateStart 
        ,SecondTaxRate 
        ,ChargeForRadiatorRefilling 
        ,RadiatorChargeAmount 
        ,ChargeForACRefilling 
        ,ACChargeHow 
        ,ACChargeAmount 
        ,LKQText 
        ,TotalLossPerc 
        ,IncludeStructureInBody 
        ,ChargeForAimingHeadlights 
        ,ChargeForPowerUnits 
        ,ChargeForRefrigRecovery 
        ,SuppressAddRelatedPrompt 
        ,ChargeSuppliesOnAllOperations 
        ,SuppLevel 
        ,DoNotMarkChanges 
        ,UseSepPartLaborTax 
        ,PartTax 
        ,LaborTax 
	FROM CustomerProfilesMisc 
	WHERE CustomerPRofilesID = @ProfileToCopy 
 
	-- Copy the Paint table 
	INSERT INTO [dbo].[CustomerProfilesPaint] 
    ( 
		 [CustomerProfilesID] 
        ,[AllowDeductions] 
        ,[AdjacentDeduction] 
        ,[NonAdjacentDeduction] 
        ,[EdgeInteriorTimes] 
        ,[MajorClearCoat] 
        ,[MajorThreeStage] 
        ,[MajorTwoTone] 
        ,[OverlapClearCoat] 
        ,[OverlapThreeStage] 
        ,[OverLapTwoTone] 
        ,[ClearCoatCap] 
        ,[DeductFinishOverlap] 
        ,[TotalClearcoatWithPaint] 
        ,[NoClearcoatCap] 
        ,[ThreeStageInner] 
        ,[ThreeStagePillars] 
        ,[ThreeStateInterior] 
        ,[TwoToneInner] 
        ,[TwoTonePillars] 
        ,[TwoToneInterior] 
        ,[Blend] 
        ,[ThreeTwoBlend] 
        ,[Underside] 
        ,[Edging] 
        ,[ManualPaintOverlap] 
        ,[AutomaticOverlap] 
	) 
    SELECT 
         @NewProfileID 
        ,AllowDeductions 
        ,AdjacentDeduction 
        ,NonAdjacentDeduction 
        ,EdgeInteriorTimes 
        ,MajorClearCoat 
        ,MajorThreeStage 
        ,MajorTwoTone 
        ,OverlapClearCoat 
        ,OverlapThreeStage 
        ,OverLapTwoTone 
        ,ClearCoatCap 
        ,DeductFinishOverlap 
        ,TotalClearcoatWithPaint 
        ,NoClearcoatCap 
        ,ThreeStageInner 
        ,ThreeStagePillars 
        ,ThreeStateInterior 
        ,TwoToneInner 
        ,TwoTonePillars 
        ,TwoToneInterior 
        ,Blend 
        ,ThreeTwoBlend 
        ,Underside 
        ,Edging 
        ,ManualPaintOverlap 
        ,AutomaticOverlap 
	FROM CustomerProfilesPaint 
	WHERE CustomerPRofilesID = @ProfileToCopy 
 
	SELECT @NewProfileID 
END 
GO
