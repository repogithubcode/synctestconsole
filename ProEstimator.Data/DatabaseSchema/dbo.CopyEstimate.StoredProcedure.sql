USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--exec CopyEstimate_Rajiv @AdminInfoID=10200123,@EstimateName=N' copy',@CopyInsurance=1,@CopyClaimant=1,@CopyVehicle=1,@CopyImages=1,@CopyLineItems=1,@LatestInfoOnly=1;

CREATE PROCEDURE [dbo].[CopyEstimate] 
	@AdminInfoID Int, 
	@EstimateName VarChar(100), 
	@CopyInsurance Bit, 
	@CopyClaimant Bit, 
	@CopyVehicle Bit, 
	@CopyImages Bit, 
	@CopyLineItems Bit, 
	@LatestInfoOnly Bit = 0
AS 
	DECLARE @Return INT

	DECLARE @LoginsID Int 
	DECLARE @EstimateNumber Int 

	DECLARE @EstimationDataId Int 
	DECLARE @MaxSupplementVersion Int 
	 
	--------------------------------------------------------------------------------------------------------------------------------------------------------------- 
	-- Line items and labor are queried a lot here, cache the data into a small table to speed this up 
	--------------------------------------------------------------------------------------------------------------------------------------------------------------- 
	Create TABLE #LineItems      
	( 
		  id	int 
		, EstimationDataID	int 
		, StepID	int 
		, PartNumber	varchar(50) 
		, PartSource	varchar(10) 
		, ActionCode	varchar(20) 
		, Description	varchar(80) 
		, Price	money 
		, Qty	int 
		, LaborTime	real 
		, PaintTime	real 
		, Other	real 
		, ImageID	int 
		, ActionDescription	varchar(80) 
		, PartOfOverhaul	bit 
		, PartSourceVendorID	int 
		, BettermentParts	bit 
		, SubletPartsFlag	bit 
		, BettermentMaterials	bit 
		, SubletOperationFlag	bit 
		, SupplementVersion	tinyint 
		, LineNumber	int 
		, UniqueSequenceNumber	int 
		, ModifiesID	int 
		, ACDCode	char(1) 
		, CustomerPrice	real 
		, AutomaticCharge	bit 
		, SourcePartNumber	varchar(25) 
		, SectionID	int 
		, VehiclePosition	varchar(5) 
		, Barcode	varchar(10) 
		, dbPrice	money 
		, AutoAdd	bit 
		, Suppress	bit 
		, AutoAddBarcodeParent	varchar(10) 
		, Date_Entered	datetime 
		, BettermentType	char(10) 
		, BettermentValue	float 
	) 
 
	INSERT INTO #LineItems 
	SELECT  
		  EstimationLineItems.id 
		, EstimationDataID 
		, StepID 
		, PartNumber 
		, PartSource 
		, ActionCode 
		, Description 
		, Price 
		, Qty 
		, LaborTime 
		, PaintTime 
		, Other 
		, ImageID 
		, ActionDescription 
		, PartOfOverhaul 
		, PartSourceVendorID 
		, BettermentParts 
		, SubletPartsFlag 
		, BettermentMaterials 
		, SubletOperationFlag 
		, EstimationLineItems.SupplementVersion 
		, LineNumber 
		, UniqueSequenceNumber 
		, ModifiesID 
		, ACDCode 
		, CustomerPrice 
		, AutomaticCharge 
		, SourcePartNumber 
		, SectionID 
		, VehiclePosition 
		, Barcode 
		, dbPrice 
		, AutoAdd 
		, Suppress 
		, AutoAddBarcodeParent 
		, Date_Entered 
		, BettermentType 
		, BettermentValue 
	FROM EstimationLineItems 
	INNER JOIN EstimationData ON EstimationData.ID = EstimationLineItems.EstimationDataid 
	WHERE EstimationData.AdminInfoID = @AdminInfoID 
 
	Create TABLE #LaborLines  
	( 
		  id	int 
		, EstimationLineItemsID	int 
		, LaborType	tinyint 
		, LaborSubType	smallint 
		, LaborTime	real 
		, LaborCost	money 
		, BettermentFlag	bit 
		, SubletFlag	bit 
		, UniqueSequenceNumber	int 
		, ModifiesID	int 
		, AdjacentDeduction	tinyint 
		, MajorPanel	bit 
		, BettermentPercentage	real 
		, dbLaborTime	real 
		, AdjacentDeductionLock	bit 
		, barcode	varchar(10) 
		, Lock	bit 
		, Include	bit 
		, WebEstID	int 
	) 
 
	INSERT INTO #LaborLines 
	SELECT  
		  EstimationLineLabor.id 
		, EstimationLineItemsID 
		, LaborType 
		, LaborSubType 
		, EstimationLineLabor.LaborTime 
		, LaborCost 
		, BettermentFlag 
		, SubletFlag 
		, EstimationLineLabor.UniqueSequenceNumber 
		, EstimationLineLabor.ModifiesID 
		, AdjacentDeduction 
		, MajorPanel 
		, BettermentPercentage 
		, dbLaborTime 
		, AdjacentDeductionLock 
		, EstimationLineLabor.barcode 
		, Lock 
		, Include 
		, WebEstID 
	FROM #LineItems LineItems 
	LEFT OUTER JOIN EstimationLineLabor ON LineItems.ID = EstimationLineLabor.EstimationLineItemsID 
	--------------------------------------------------------------------------------------------------------------------------------------------------------------- 
	--------------------------------------------------------------------------------------------------------------------------------------------------------------- 

	-- Get the current Estimate number (need to get the LoginID first for the estimate)
	SELECT @LoginsID = CreatorID 
	FROM AdminInfo
	WHERE ID = @AdminInfoID 
 
	EXECUTE GetEstimateNumber @LoginsID, @EstimateNumber OUTPUT 


	-- Create new AdminInfo Entry 
	INSERT INTO AdminInfo
	(
	      Description
		, CreatorID
		, CustomerProfilesID
		, GrandTotal
		, BettermentTotal
		, EstimateNumber
		, CustomerID
		, AddOnProfileID
	) 
	SELECT  
		  @EstimateName 
		, CreatorID 
		, CustomerProfilesID 
		, CASE WHEN @CopyLineItems = 1 THEN GrandTotal ELSE '0' END 
		, BettermentTotal 
		, @EstimateNumber 
		, CustomerID 
		, AddOnProfileID
	FROM AdminInfo 
	WHERE ID = @AdminInfoID 

	SELECT @Return = @@IDENTITY 
 
	-- Find the max supplement level used in the estimate
	SELECT @MaxSupplementVersion = ISNULL(Max(LineItems.SupplementVersion), 0) 
	FROM #LineItems LineItems;
 
	DECLARE @MaxPDRSupplementVersion INT
	SELECT @MaxPDRSupplementVersion = MAX(SupplementVersion)
	FROM PDR_EstimateDataPanel
	LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange ON PDR_EstimateDataPanel.ID = PDR_EstimateDataPanelSupplementChange.EstimateDataPanelID
	WHERE PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID

	IF (@MaxPDRSupplementVersion > @MaxSupplementVersion)
		BEGIN
			SET @MaxSupplementVersion = @MaxPDRSupplementVersion
		END
  
	-- Create Estimation Data Header 
	INSERT INTO EstimationData
	(	
		  AdminInfoID
		, EstimationDate
		, DateOfLoss
		, CoverageType
		, EstimateLocation
		, TransactionLevel
		, LockLevel
		, LastLineNumber
		, EstimationLineItemIDLocked
		, Note
		, PrintNote
		, AssignmentDate
		, ReportTextHeader
		, AlternateIdentitiesID
		, SupplementVersion
		, NextUniqueSequenceNumber
		, ClaimNumber
		, PolicyNumber
		, Deductible
		, InsuranceCompanyName
		, ClaimantSameAsOwner
		, InsuredSameAsOwner
		, EstimatorID
	) 
	SELECT 
		@Return 'AdminInfoID',  
		GetDate() 'EstimationDate',  
		CASE WHEN @CopyInsurance <> 0 THEN DateOfLoss ELSE NULL END 'DateOfLoss',  
		CASE WHEN @CopyInsurance <> 0 THEN CoverageType ELSE NULL END 'CoverageType', 
		EstimateLocation, 
		TransactionLevel, 
		CASE WHEN @CopyLineItems = 1 AND @LatestInfoOnly = 0 THEN @MaxSupplementVersion ELSE NULL END 'LockLevel', 
		LastLineNumber, 
		CASE WHEN @LatestInfoOnly = 0 THEN EstimationLineItemIDLocked ELSE NULL END 'EstimationLineItemIDLocked', 
		Note, 
		PrintNote, 
		AssignmentDate, 
		ReportTextHeader, 
		AlternateIdentitiesID, 
		0, 
		NextUniqueSequenceNumber,
		EstimationData.ClaimNumber,
		PolicyNumber,
		Deductible,
		InsuranceCompanyName,
		ClaimantSameAsOwner, 
		InsuredSameAsOwner,
		ISNULL(EstimatorID, 0)
	FROM AdminInfo 
	INNER JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.id
	WHERE AdminInfo.ID = @AdminInfoID 
 
	SELECT @EstimationDataId = @@Identity 



	-- Copy/Create Vehicle Info 
	IF @CopyVehicle = 1 
		BEGIN 
			INSERT INTO VehicleInfo   
			( 
				  EstimationDataId 
				, VehicleID 
				, ExtColor 
				, ExtColorCode 
				, MilesIn 
				, MilesOut 
				, License 
				, State 
				, Condition 
				, IntColor 
				, IntColorCode 
				, TrimLevel 
				, Vin 
				, VinDecode 
				, InspectionDate 
				, ProductionDate 
				, BodyType 
				, Service_Barcode 
				, DefaultPaintType 
				, DriveType 
				, VehicleValue 
				, Year 
				, MakeID 
				, ModelID 
				, SubModelID 
				, EngineType 
				, TransmissionType 
				, paintcode 
				, StockNumber
				, POILabel1 
				, POIOption1 
				, POICustom1 
				, POILabel2 
				, POIOption2 
				, POICustom2 
			) 
			SELECT  
				@EstimationDataId, 
				VehicleID, 
				ExtColor, 
				ExtColorCode, 
				MilesIn, 
				MilesOut, 
				License, 
				State, 
				Condition, 
				IntColor, 
				IntColorCode, 
				TrimLevel, 
				Vin, 
				VinDecode, 
				InspectionDate,  
				ProductionDate,  
				BodyType,  
				Service_Barcode,  
				DefaultPaintType, 
				DriveType,  
				VehicleValue,  
				Year,  
				MakeID,  
				ModelID,  
				SubModelID,  
				EngineType,  
				TransmissionType,  
				paintcode,  
				StockNumber, 
				POILabel1, 
				POIOption1, 
				POICustom1, 
				POILabel2, 
				POIOption2, 
				POICustom2 
			FROM AdminInfo 
			INNER JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.id
			INNER JOIN VehicleInfo ON VehicleInfo.EstimationDataId = EstimationData.Id
			WHERE AdminInfo.ID = @AdminInfoID 
 
			DECLARE @VehicleInfoID INT = (SELECT CAST(SCOPE_IDENTITY() AS INT)) 
 
			INSERT INTO VehicleInfoManual 
			( 
				  VehicleInfoID 
				, Country 
				, Manufacturer 
				, Make 
				, Model 
				, ModelYear 
				, SubModel 
				, Service_Barcode 
				, ManualSelection 
			) 
			SELECT 
				  @VehicleInfoID 
				, VehicleInfoManual.Country 
				, VehicleInfoManual.Manufacturer 
				, VehicleInfoManual.Make 
				, VehicleInfoManual.Model 
				, VehicleInfoManual.ModelYear 
				, VehicleInfoManual.SubModel 
				, VehicleInfoManual.Service_Barcode 
				, VehicleInfoManual.ManualSelection 
			FROM AdminInfo 
			INNER JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.id 
			INNER JOIN VehicleInfo ON VehicleInfo.EstimationDataId = EstimationData.Id 
			INNER JOIN VehicleInfoManual ON VehicleInfo.id = VehicleInfoManual.VehicleInfoID 
			WHERE AdminInfo.ID = @AdminInfoID 

			INSERT INTO VehicleSelectedOptions
			(
				  VehicleInfoID
				, VehicleOptionsID
			)
			SELECT	
				  @VehicleInfoID
				, VehicleOptionsID
			FROM AdminInfo 
			INNER JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.id 
			INNER JOIN VehicleInfo ON VehicleInfo.EstimationDataId = EstimationData.Id 
			INNER JOIN VehicleSelectedOptions ON VehicleInfo.id = VehicleSelectedOptions.VehicleInfoID 
			WHERE AdminInfo.ID = @AdminInfoID 
		END 
	ELSE 
		INSERT INTO VehicleInfo (EstimationDataID, VehicleID) VALUES (@EstimationDataId,-1) 
 

  
	-- Copy LineItems 
	IF @CopyLineItems = 1
		BEGIN 
			INSERT INTO EstimationLineItems 
			(
					EstimationDataID
				, StepID
				, PartSource
				, ActionCode
				, ActionDescription
				, PartNumber
				, Description
				, Price
				, PartOfOverhaul
				, Qty
				, LaborTime
				, PaintTime
				, Other
				, ImageID
				, PartSourceVendorID
				, BettermentParts
				, SubletPartsFlag
				, BettermentType
				, SubletOperationFlag
				, SupplementVersion
				, LineNumber
				, UniqueSequenceNumber
				, ModifiesID
				, ACDCode
				, CustomerPrice
				, BettermentValue
				, AutomaticCharge
				, SourcePartNumber
				, SectionID
				, Barcode
				, dbPrice
			)
			SELECT 
				@EstimationDataId,  
				LineItems.StepID,  
				LineItems.PartSource,  
				LineItems.ActionCode,  
				LineItems.ActionDescription,  
				LineItems.PartNumber,  
				LineItems.Description,  
				LineItems.Price,  
				LineItems.PartOfOverhaul,  
				LineItems.Qty,  
				LineItems.LaborTime,  
				LineItems.PaintTime,  
				LineItems.Other, 
				--LineItems.ImageID,  
				LineItems.ID,
				LineItems.PartSourceVendorID,  
				LineItems.BettermentParts,  
				LineItems.SubletPartsFlag,  
				LineItems.BettermentType,  
				LineItems.SubletOperationFlag,  
				CASE WHEN @LatestInfoOnly = 1 THEN 0 ELSE LineItems.SupplementVersion END,  
				LineItems.LineNumber, 
				LineItems.UniqueSequenceNumber,  
				CASE WHEN @LatestInfoOnly = 0 THEN LineItems.ModifiesID ELSE NULL END,  
				CASE WHEN @LatestInfoOnly = 0 THEN LineItems.ACDCode ELSE NULL END,  
				LineItems.CustomerPrice,  
				LineItems.BettermentValue,  
				LineItems.AutomaticCharge,  
				LineItems.SourcePartNumber,  
				LineItems.SectionID,
				LineItems.Barcode,  
				LineItems.dbPrice 
			FROM #LineItems LineItems 
			WHERE 
				@LatestInfoOnly = 0 
				OR LineItems.SupplementVersion = @MaxSupplementVersion
			ORDER BY LineItems.ID 

			UPDATE EstimationLineItems
			SET EstimationLineItems.ModifiesID = EstimationLineItemsMod.id
			FROM EstimationLineItems
			LEFT OUTER JOIN EstimationLineItems EstimationLineItemsMod ON EstimationLineItems.ModifiesID = EstimationLineItemsMod.ImageID AND EstimationLineItemsMod.EstimationDataID = @EstimationDataId
			WHERE EstimationLineItems.EstimationDataID = @EstimationDataId
	
			--Labor Items 
			INSERT INTO EstimationLineLabor 
			( 
				  EstimationLineItemsID
				, LaborType
				, LaborSubType
				, LaborTime
				, LaborCost
				, BettermentFlag
				, SubletFlag
				, UniqueSequenceNumber
				, ModifiesID
				, AdjacentDeduction
				, MajorPanel
				, BettermentPercentage
				, dbLaborTime
				, Include
			) 
			SELECT  
				  EstimationLineItems.ID
				, LaborType
				, LaborSubType
				, LaborLines.LaborTime
				, LaborCost
				, BettermentFlag
				, SubletFlag
				, LaborLines.UniqueSequenceNumber
				, LaborLines.ModifiesID
				, AdjacentDeduction
				, MajorPanel
				, BettermentPercentage
				, dbLaborTime
				, Include 
			FROM #LaborLines LaborLines 
			JOIN EstimationLineItems ON LaborLines.EstimationLineItemsID = EstimationLineItems.ImageID AND EstimationLineItems.EstimationDataID = @EstimationDataId

			--Item Notes 
			INSERT INTO EstimationNotes
			( 
				EstimationLineItemsID
				, Printed
				, Notes
			) 
			SELECT 
				  EstimationLineItems.ID
				, EstimationNotes.Printed
				, EstimationNotes.Notes
			FROM EstimationLineItems
			LEFT OUTER JOIN EstimationNotes ON EstimationLineItems.ImageID = EstimationNotes.EstimationLineItemsID
			WHERE EstimationLineitems.EstimationDataID = @EstimationDataId

			-- Copy Overlap data
			INSERT INTO EstimationOverlap
			(
				EstimationLineItemsID1
				, EstimationLineItemsID2
				, OverlapAdjacentFlag
				, Amount
				, SectionOverlapsID
				, Minimum
				, UserOverride
				, UserAccepted
				, UserResponded
				, SupplementLevel
			)
			SELECT
				  EstimationLineItemsNew1.id
				, EstimationLineItemsNew2.id
				, EstimationOverlap.OverlapAdjacentFlag
				, EstimationOverlap.Amount
				, EstimationOverlap.SectionOverlapsID
				, EstimationOverlap.Minimum
				, EstimationOverlap.UserOverride
				, EstimationOverlap.UserAccepted
				, EstimationOverlap.UserResponded
				, EstimationOverlap.SupplementLevel
			FROM EstimationData 
			LEFT OUTER JOIN EstimationLineItems ON EstimationData.id = EstimationLineItems.EstimationDataID
			LEFT OUTER JOIN EstimationOverlap ON EstimationLineItems.ImageID = EstimationOverlap.EstimationLineItemsID1
			LEFT OUTER JOIN EstimationLineItems AS EstimationLineItemsNew1 ON EstimationOverlap.EstimationLineItemsID1 = EstimationLineItemsNew1.ImageID
			LEFT OUTER JOIN EstimationLineItems AS EstimationLineItemsNew2 ON EstimationOverlap.EstimationLineItemsID2 = EstimationLineItemsNew2.ImageID
			WHERE EstimationData.AdminInfoID = @Return AND EstimationOverlap.ID IS NOT NULL

			IF @LatestInfoOnly = 1 
				BEGIN
					EXEC UpdateAdminTotals @AdminInfoID = @Return
				END
		END 
 
	IF (@CopyClaimant <> 0) 
		BEGIN 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 3	-- Claimant 
		END 
 
	IF (@CopyInsurance <> 0) 
		BEGIN 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 7	-- Insurance Agent 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 11	-- Insurance Adjuster 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 20	-- Insurance Adjuster 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 3	-- Insurance Claimant 
			EXEC CopyContactAndAddress @adminInfoID = @AdminInfoID, @newAdminInfoID = @Return, @contactTypeID = 1, @contactSubTypeID = 2	-- Insured 
		END 
 
	-- Fix EstimationLineItemIDLocked 
	IF (SELECT ISNULL(EstimationLineItemIDLocked, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID) > 0
		BEGIN
			UPDATE EstimationData
			SET EstimationLineItemIDLocked = 
			(
				SELECT ID
				FROM EstimationLineItems
				WHERE 
					EstimationDataID = @EstimationDataId 
					AND ImageID = EstimationData.EstimationLineItemIDLocked 
			) 
			FROM EstimationData
			WHERE EstimationData.ID = @EstimationDataId
		END
	
	EXECUTE RedoLineNumbers @AdminInfoID = @Return 
	SELECT @Return	
GO
