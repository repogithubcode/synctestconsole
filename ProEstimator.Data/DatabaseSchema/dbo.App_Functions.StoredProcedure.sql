USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

--App_Functions-02.sql
CREATE PROCEDURE [dbo].[App_Functions]   
	@Function VarChar(50),   
	@LoginsID Int = NULL,   
	@Description VarChar(255) =NULL,   
	@SessionID VarChar(100) =NULL,   
	@AdminInfoID Int = NULL,   
	@GroupNumber Int = NULL,   
	@PartReference VarChar(20) = NULL,   
	@CommunicationsID Int = NULL,   
	@Action VarChar(10) = NULL,   
	@Qty Int = NULL,   
	@Labor Real = Null,   
	@Order VarChar(10) = NULL,   
	@VehicleInfoID Int = NULL,   
	@Reference Int = NULL,   
	@ProfileItemCode Int = NULL,   
   
	@NamePrefixCode VarChar(15) = NULL,   
	@FirstName VarChar(25) = NULL,   
	@MiddleName VarChar(25) = NULL,    
	@LastName VarChar(60) = NULL,   
	@NameSuffixCode VarChar(15) = NULL,   
	@EntryType VarChar(10) = NULL,   
	@ContactType VarChar(20) = NULL,   
   
	@CommQualifier VarChar(50)  = NULL,   
	@PhoneNumber VarChar(25) = NULL,   
	@EmailAddress VarChar(50) = NULL,   
	@Address1 VarChar(55) = NULL,   
	@Address2 VarChar(55) = NULL,   
	@City VarChar(30) = NULL,   
	@StateProvinceCode VarChar(20) = NULL,   
	@PostalCode VarChar(15) = NULL,   
	@CountryCode VarChar(20) = NULL,   
	@PreferredInd Bit = 0,   
	@OwnerInd Bit = 0,   
	@StepID Int = NULL,   
	@PolicyNum VarChar(50) = NULL,   
	@SortDirection VarChar(4) = NULL,   
	@SortField VarChar(100) = NULL,   
	@SubModelID Int = NULL,   
	@Vin VarChar(25) = NULL,   
	@ProductionDate Varchar(15) = NULL,   
	   
	@PartNumber VarChar(50) = NULL,   
	@PartSource VarChar(20) = NULL,   
	@SourcePartNumber VarChar(50) = NULL,   
	@OtherPartsID Int = NULL,   
	@Price Money = NULL,   
	@Operation VarChar(20) = NULL,   
	@OpDesc VarChar(80) = NULL,   
	@PaintTime Real = NULL,   
	@PaintType Int = NULL,   
	@LaborType Int = NULL,   
	@LaborTime Real = NULL,   
	@LaborIncluded Bit = 0,
	@OtherCharge Real = NULL,   
	@Other Int = NULL,   
	@ClearcoatTime Real = NULL,   
	@BlendTime Real = NULL,   
	@Allowance Real = NULL,   
	@EdgingType Int = NULL,   
	@EdgingTime Real = NULL,   
	@UndersideTime Real = NULL,   
	@InternalNotes VarChar(5000) = NULL,   
	@ExternalNotes VarChar(5000) = NULL,   
	@Overhaul Bit = NULL,   
   
	@ExtColor VarChar(100) = NULL,   
	@ExteriorColorCode VarChar(20) = NULL,   
	@MileageIn Decimal(9,1) =NULL,   
	@MileageOut Decimal(9,1) =NULL,   
	@License VarChar(20) = NULL,   
	@State VarChar(20) = NULL,   
	@VehicleCondition VarChar(100) = NULL,   
	@IntColor VarChar(100) = NULL,   
	@InteriorColorCode VarChar(20) = NULL,   
	@TrimOption VarChar(20) = NULL,   
	@SelectedString VarChar(1000) = NULL,   
	@ProfileID Int = NULL,   
	@EstimationLineItemsID Int = NULL,   
   
	@ErrorText VarChar(2000) = NULL,   
	@SessionInfo VarChar(2000) = NULL,   
	@AddBlank Bit = 0,   
   
	@RegionText VarChar(50) = NULL,    
	@ManufacturerText VarChar(50) = NULL,    
	@MakeText VarChar(50) = NULL,    
	@ModelText VarChar(50) = NULL,    
	@ModelYearText VarChar(50) = NULL,    
	@SubModelText VarChar(50) = NULL,    
	@CustomerPrice Money = NULL,   
	@BettermentValue Real = NULL,   
	@BettermentType CHAR(1) = '',		-- Blank for none, P for percent D for dollar   
	@VehiclePosition VarChar(5) = '',   
   
	@VendorID Int = 0,   
	@OperationSublet Bit = 0,   
	@MaterialsBetterment Bit = 0,   
	@MaterialsSublet Bit = 0,   
	@PartBetterment Bit = 0,   
	@PartSublet Bit = 0,   
	@LaborBetterment Bit = 0,   
	@LaborSublet Bit = 0,   
	@PaintBetterment Bit = 0,   
	@PaintSublet Bit = 0,   
	@VinDecode VarChar(2000) = NULL,   
	@OwnPage Bit = 0,   
	@AdjacentDeduction TinyInt = 0,   
	@MajorPanel Bit = 0,   
	@FilterOption TinyInt = 5,   
	@BodyType Int = NULL,   
	@StepsID Int = NULL,   
	@Side VarChar(10) = NULL,   
	@GraphicsViewer TinyInt = 0,   
	@GraphicsVersion TinyInt = 0,   
	@InfoString VarChar(2000) = NULL,   
	@PartID Int = NULL,   
	@SectionID Int = NULL,   
	@FilterYear VarChar(50) = NULL,   
	@FilterMake VarChar(50) = NULL,   
	@FilterModel VarChar(50) = NULL,   
	@barcode VarChar(10)= NULL,   
	@nHeader Int = NULL,   
	@nSection Int = NULL,   
	@nPart Int = NULL,   
	@dbPrice Money = NULL,   
	@dbLaborTime Real = NULL,   
	@DefaultPaintType Int = 0,	   
	@UpdateString VarChar(4000) = NULL,   
	@AdjacentDeductionLock Bit = 0,   
	@DriveType Int = NULL,   
	@IDsChecked VarChar(1000) = NULL,   
	@NoOverlapCheck Bit = 0,   
 	@LockAllowance Bit = 0,   
	@LockClearcoat Bit = 0,   
	@LockBlend Bit = 0,   
	@LockEdging Bit = 0,   
	@LockUnderside Bit = 0,	   
	@IncludeAllowance Bit = 0,   
	@IncludeClearcoat Bit = 0,   
	@IncludeBlend Bit = 0,   
	@IncludeEdging Bit = 0,   
	@IncludeUnderside Bit = 0,	   
	@ViewAll Bit = 0,   
	@App VarChar(25) = NULL,   
	@AutomaticCharge Bit = 0,   
	@AutoAddBarcodeParent VarChar(10) = NULL,   
	@AutoPaint VarChar(5) = NULL,   
	@PageNumber Int = 1,   
	@RowsPerPage Int = 50,   
	@SearchText VarChar(50) = NULL,   
	@SubModel varchar(10) = null,   
	@Year_ID varchar(4) = null,   
	@Make_ID varchar(10) = null,   
	@Model_ID varchar(10) = null,   
	@SubModel_ID varchar(10) = null,   
	@EngineType varchar(10) = null,   
	@TransmissionType varchar(10) = null,   
	@ServiceBarcode varchar(10) = null,   
	@Language_ID Int = Null,   
	   
	@UpdateBaseRecord BIT = false,  
	@IsPartsQuantity BIT = false,  
	@IsLaborQuantity BIT = false,  
	@IsPaintQuantity BIT = false,  
	@IsOtherCharges BIT = false,  
  
	@BarcodeRequired Bit = NULL OUTPUT,   
	@VehicleValue Real = NULL OUTPUT,   
	@OwnerID Int = NULL OUTPUT,   
	@ClaimantID Int = NULL OUTPUT,   
	@PolicyHolderID Int = NULL OUTPUT,   
	@ReturnText VarChar(100) = NULL OUTPUT,	   
	@AccessoriesSelected VarChar(1000) = NULL OUTPUT,    
	@VehicleID Int = NULL OUTPUT,   
	@Return Int = NULL OUTPUT,   
	@Return2 Int = NULL OUTPUT,   
	@ModelYearID Int = NULL OUTPUT,   
	@ModelID Int = NULL OUTPUT,   
	@MakeID Int = NULL OUTPUT,   
	@ManufacturerID Int = NULL OUTPUT,   
	@RegionID Int = NULL OUTPUT,   
	@PartyID Int = NULL OUTPUT,   
	@MitchellServiceID Int = NULL OUTPUT  
	   
AS   
   
SET NOCOUNT ON   
   
DECLARE @id Int   
DECLARE @PersonInfoID Int   
DECLARE @PersonNameID Int   
DECLARE @AddressID Int   
DECLARE @EstimationDataID Int   
DECLARE @StrSql VarChar(8000)   
DECLARE @nStrSql nVarChar(4000)   
DECLARE @SortSQL VarCHar(1000)   
DECLARE @LastLineNumber SmallInt   
DECLARE @LockedID Int   
DECLARE @SupplementVersion Int   
DECLARE @Status VarChar(25)   
DECLARE @ModifiesID Int   
DECLARE @AcdCode Char(1)   
DECLARE @NewID Int   
DECLARE @LineNumber Int   
DECLARE @LineNumberModified Int   
DECLARE @NextSeq Int   
DECLARE @GraphicsVehicleID Int   
DECLARE @NoteID Int   
DECLARE @VehicleOpTimesVehicleID Int   
DECLARE @TempDate DateTime   
DECLARE @ContactsID Int   
DECLARE @Appraiser Bit   
DECLARE @CustomerProfilesID Int   
DECLARE @CurrentProfileAdminInfoID Int   
DECLARE @EstimateProfileID Int   
DECLARE @EstimateNumber Int   
DECLARE @AdjacentInfo VarChar(200)   
DECLARE @Service_Barcode Varchar(10)   
DECLARE @AppraiserFlag Bit   
DECLARE @RowCount Int   
DECLARE @ClearcoatTimeTemp Real   
DECLARE @RepEstimationLineItemsID Int   
DECLARE @VehicleName VarChar(100)   
DECLARE @Archived Bit   
DECLARE @LineItemLaborTime	REAL = 0			-- For web-est imported estimates, this is used for the EstimationLineItems.LaborTime field value which is the overlap calculated in web-est and needs to be copied to new lines for supplements 
   
IF 	@AdminInfoID IS NOT NULL AND   
	@VehicleID IS NULL   
BEGIN   
	SELECT @VehicleID = ABS(VehicleInfo.VehicleID)   
	FROM AdminInfo   with(nolock)
	INNER JOIN EstimationData  ON   
		(EstimationData.AdminInfoID = AdminInfo.id)   
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataId = EstimationData.id)   
	WHERE AdminInfo.id = @AdminInfoID   
END   
   
IF ISNULL(@OperationSublet,0) <> 0    
BEGIN   
	SELECT	@PartSublet = 1,   
		@LaborSublet = 1,   
		@PaintSublet = 1   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetSections' AND   
	@VehicleID IS NOT NULL AND   
	@Order IN ('F2B','B2F','Grouped','Forwards','Backwards') AND   
	@VehicleID IS NOT NULL   
BEGIN   
	/*IF @VehicleID = 35826   
		EXECUTE App_Functions_Honda    
			@Function = @Function,   
			@VehicleID = @VehicleID,   
			@Order = @Order   
	ELSE  */   
		EXECUTE GetVehicleStepsTree @VehicleID, @BodyType  --, @Order = @Order   
   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetWork'   
BEGIN   
	EXECUTE GetWork2    
		@AdminInfoID = @AdminInfoID,    
		@GroupNumber = @GroupNumber   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
--IF @Function = 'GetStartMenu' AND   
--	@LoginsID IS NOT NULL AND   
--	@SessionID IS NOT NULL   
--BEGIN   
	--SELECT  DISTINCT   
	--	StartMenu.id,   
	--	StartMenu.ParentID,   
	--	StartMenu.ItemOrder,   
	--	CASE	WHEN 	CHARINDEX('Estimate',StartMenu.DisplayText) > 0 AND   
	--			ISNULL(OrganizationInfo.Appraiser,0) = 1 THEN REPLACE(StartMenu.DisplayText,'estimate','Appraisal')   
	--		ELSE	StartMenu.DisplayText   
	--	END 'DisplayText',   
	--	StartMenu.ClickAction,   
	--	StartMenu.ShowWhenNoEstimate,   
	--	StartMenu.ShowOnlyWhenNoEstimate,   
	--	StartMenu.AdminRequired,   
	--	StartMenu.FWAdminRequired,   
	--	StartMenu.CSS,   
	--	StartMenu.CSSOver,   
	--	StartMenu.CSSSelected,   
	--	StartMenu.ProfileRequired   
	--FROM StartMenu    
	--INNER JOIN Logins  ON   
	--	(Logins.ID = @LoginsID)   
	--INNER JOIN Sessions  ON   
	--	(Sessions.SessionID = @SessionID)   
	--LEFT JOIN AdminInfo  ON   
	--	(AdminInfo.id = Sessions.AdminInfoID)   
	--LEFT JOIN CustomerProfiles CustomerProfilesDefault  ON   
	--	(CustomerProfilesDefault.OwnerID = Logins.ID AND   
	--	 CustomerProfilesDefault.DefaultFlag <> 0)   
	--LEFT JOIN CustomerProfiles CustomerProfilesSet  ON   
	--	(CustomerProfilesSet.ID = AdminInfo.CustomerProfilesID)   
	--LEFT JOIN OrganizationInfo OrganizationInfo  ON	   
	--	(OrganizationInfo.id = Logins.OrganizationID)   
	--WHERE 	--(ISNULL(Logins.OrgAdminRights,0) = 1 or (ISNULL(Logins.OrgAdminRights,0) = 0 and StartMenu.AdminRequired = 0)) AND   
	--	(ISNULL(Logins.FWAdminRights,0) = 1 or (ISNULL(Logins.FWAdminRights,0) = 0 and StartMenu.FWAdminRequired = 0)) AND   
	--	( (Sessions.AdminInfoID IS NULL AND (StartMenu.ShowWhenNoEstimate = 1 OR StartMenu.ShowOnlyWhenNoEstimate = 1)) OR   
	--	  (Sessions.AdminInfoID IS NOT NULL AND StartMenu.ShowOnlyWhenNoEstimate = 0)  ) AND   
	--	( StartMenu.ProfileRequired = 0 OR    
	--	  (StartMenu.ProfileRequired <> 0 AND    
	--		(CustomerProfilesDefault.id IS NOT NULL OR CustomerProfilesSet.id IS NOT NULL) ) ) AND   
	--	--Do Options here   
	--	( ( StartMenu.ClickAction = 'RepairFacilityProfiles' AND ISNULL(OrganizationInfo.ShowRepairShopProfiles,0) = 1 ) OR   
	--	  ( StartMenu.ClickAction <> 'RepairFacilityProfiles') ) AND   
	--	--Alternate Identities Check   
	--	( ( StartMenu.ClickAction = 'AltIdent' AND ISNULL(OrganizationInfo.AllowAlternateIdentities,0) = 1 ) OR   
	--	  ( StartMenu.ClickAction <> 'AltIdent') ) AND   
	--	--Extimate Info On Own Page Check   
	--	( ( StartMenu.ClickAction = 'EstInfo' AND ISNULL(Logins.EstInfoOwnPage,0) = 1 AND ISNULL(Logins.UseTabsOnContactInfo,0) = 0) OR   
	--	  ( StartMenu.ClickAction <> 'EstInfo') ) AND   
	--	--ContactInfo on tabs page   
	--	( ( StartMenu.ClickAction = 'ContactTabs' AND ISNULL(Logins.UseTabsOnContactInfo,0) = 1) OR   
	--	  ( StartMenu.ClickAction <> 'ContactTabs') ) AND   
	--	--ContactInfo NOT on tabs page   
	--	( ( StartMenu.ClickAction = 'AllContactInformation' AND ISNULL(Logins.UseTabsOnContactInfo,0) = 0) OR   
	--	  ( StartMenu.ClickAction <> 'AllContactInformation') ) AND   
	--	--Verify has access to graphics   
	--	( ( StartMenu.ClickAction = 'Viewer' AND ISNULL(OrganizationInfo.GraphicsExpireDate,'2099/12/31') > GetDate() ) OR   
	--	  ( StartMenu.ClickAction <> 'Viewer' ) ) AND   
	--	--Verify has access to Manualentry   
	--	( ( StartMenu.ClickAction = 'Manual' AND ISNULL(OrganizationInfo.ManualExpireDate,'2099/12/31') > GetDate() ) OR   
	--	  ( StartMenu.ClickAction <> 'Manual' ) )   
   
	--ORDER BY StartMenu.ParentID, StartMenu.ItemOrder   
--END   
/**********************************************************************************************************/   
   
IF @Function = 'CloseEstimate' AND   
	@SessionID IS NOT NULL   
BEGIN   
	UPDATE Sessions    
	SET	AdminInfoID = NULL   
	WHERE SessionID = @SessionID   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'UpdateVehicle' AND   
	@VehicleID IS NOT NULL AND   
	@AdminInfoID IS NOT NULL AND   
	@LoginsID IS NOT NULL   
BEGIN print 'aaa'   
	IF @VehicleID = -1 SELECT @VehicleID = NULL   
	IF (   
		SELECT AdminInfoID    
		FROM Sessions    
		WHERE SessionID = @SessionID) <> @AdminInfoID   
	BEGIN   
		IF ( 	   
			SELECT Count(*)   
			FROM Sessions    
			WHERE AdminInfoID = @AdminInfoID AND   
				SessionID <> @SessionID) = 0   
		BEGIN   
			UPDATE Sessions    
			SET AdminInfoID = @AdminInfoID   
			FROM Sessions    
			WHERE SessionID = @SessionID   
		END   
		ELSE	   
   
		BEGIN   
			RAISERROR( 'Someone else is editing that estimation record.  Changes can not be made.', 16, 1)   
			RETURN 0   
		END   
	END   
	   
	IF NOT EXISTS (	SELECT ID   
			FROM EstimationData    
			WHERE AdminInfoID = @AdminInfoID)   
   
	BEGIN   
		INSERT INTO EstimationData  (AdminInfoID) VALUES (@AdminInfoID)   
	END   
	declare @veh int   
	IF EXISTS (	SELECT VehicleInfo.id   
			FROM VehicleInfo 	   
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataID)   
			WHERE EstimationData.AdminInfoID = @AdminInfoID	)   
	BEGIN   
		SELECT @veh = VehicleInfo.vehicleid   
			FROM VehicleInfo 	   
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataID)   
			WHERE EstimationData.AdminInfoID = @AdminInfoID	   
			   
		if @AdminInfoID >  1583497 and @SubModel_ID = 0 and @VehicleID is not null    
		BEGIN   
				print 'III'   
			UPDATE VehicleInfo    
			SET VehicleID = ABS(@VehicleID)   
				,Year = x.VinYear   
				,MakeID = x.MakeID   
				,ModelID = x.ModelID   
				,SubModelID = x.SubModelID   
				,BodyType = b.BodyID   
				,DriveType = D.DriveID   
				,EngineType = x.EngineId   
				,TransmissionType =  x.TransmissionID   
				,Service_Barcode = x.Service_Barcode   
			FROM VehicleInfo   with(nolock) 
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataID)   
			INNER JOIN Vinn.dbo.Vehicle_Service_Xref as x    
				on @VehicleID = x.VehicleID   
			INNER JOIN Vinn.dbo.VinToBody as vb   
				on x.MakeID = vb.makeid   
				and x.ModelID = vb.modelid   
			inner join Vinn.dbo.bodys As b    
				on vb.BodyID = b.BodyID   
			inner join vinn.dbo.VinToDrive As vd    
				on vd.MakeID = x.MakeID   
				and vd.ModelID = x.ModelID   
			inner join vinn.dbo.Drives AS D    
				on d.DriveID = vd.DriveID   
			inner join Vinn.dbo.VinToEngine as ve    
				on ve.MakeID = x.MakeID   
				and ve.ModelID = x.ModelID   
			inner join Vinn.dbo.Engines As e   
				on e.EngineID = ve.EngineID   
			inner join vinn.dbo.VinToTransmission AS vt   
				on vt.MakeID = x.MakeID   
				and vt.ModelID = x.ModelID   
			inner join vinn.dbo.Transmissions As T   
				on t.TransmissionsID = vt.TransmissionID   
			WHERE EstimationData.AdminInfoID = @AdminInfoID   
		    
		end   
		else   
		begin print 'hhh'   
			if @Service_Barcode is null   
			begin print 'eee'   
				select top 1 @Service_Barcode = service_barcode from Vinn.dbo.Vehicle_Service_Xref where vehicleid = @vehicleid   
			end    
			UPDATE VehicleInfo    
			SET VehicleID = ABS(@VehicleID)   
				,Year = @Year_ID   
				,MakeID = @Make_ID   
				,ModelID = @Model_ID   
				,SubModelID = @SubModel_ID   
				,BodyType = @BodyType   
				,DriveType = @DriveType   
				,EngineType = @EngineType   
				,TransmissionType =  @TransmissionType   
				,service_barcode = @Service_Barcode   
			FROM VehicleInfo    
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataID)   
			WHERE EstimationData.AdminInfoID = @AdminInfoID   
		end   
	END   
	ELSE   
	BEGIN   
		print 'ddd'   
		if @Service_Barcode is null   
		begin print 'eee'   
			select top 1 @Service_Barcode = service_barcode from Vinn.dbo.Vehicle_Service_Xref where vehicleid = @vehicleid   
		end    
		print @Service_Barcode   
		print 'ggg'   
		INSERT INTO VehicleInfo  (EstimationDataID, VehicleID,Year,MakeID,ModelID,SubModelID,BodyType,DriveType)   
		SELECT EstimationData.ID, ABS(@VehicleID),@Year_ID,@Make_ID,@Model_ID,@SubModel_ID,@BodyType,@DriveType   
		FROM EstimationData    
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
	IF EXISTS (	SELECT VehicleInfo.VehicleID   
			FROM VehicleInfo 	   
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataID)   
			WHERE EstimationData.AdminInfoID = @AdminInfoID	AND   
				ISNULL(VehicleInfo.VehicleValue,0) = 0	)   
	BEGIN print 'eee'   
		--Get the vehicle value and set it in the vehicleinfo if there is not already a value in it   
		SELECT TOP 1 @VehicleValue = VehicleValues.VehicleValue   
		FROM VehicleInfo    
		INNER JOIN VehicleValues  ON   
			(VehicleValues.VehicleID = VehicleInfo.VehicleID)   
		INNER JOIN EstimationData  ON   
			(EstimationData.id = VehicleInfo.EstimationDataID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
		ORDER BY VehicleValues.VehicleID, VehicleValues.EstimateDate DESC   
   
		UPDATE VehicleInfo    
		SET VehicleValue = @VehicleValue   
		FROM VehicleInfo    
		INNER JOIN EstimationData  ON   
			(EstimationData.id = VehicleInfo.EstimationDataID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
END   
/**********************************************************************************************************/   
   
IF @Function = 'GetVehicleValue' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT @VehicleValue = VehicleInfo.VehicleValue   
	FROM VehicleInfo    
	INNER JOIN EstimationData  ON   
		(EstimationData.id = VehicleInfo.EstimationDataID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetVehicleInfo'   
BEGIN   
	SELECT @ModelYearID = NULL,   
		@ModelID = NULL,   
		@MakeID = NULL,   
		@ManufacturerID = NULL,   
		@RegionID = NULL,   
		@GraphicsVehicleID = NULL   
   
	SELECT @ModelYearID = ModelYearID   
	FROM Vehicles.Dbo.SubModel    
	WHERE id = Abs(@VehicleID)   
   
	SELECT @ModelID = ModelID   
	FROM Vehicles.Dbo.ModelYear    
	WHERE id = @ModelYearID   
   
	SELECT @MakeID = MakeID   
	FROM Vehicles.Dbo.Model    
	WHERE id = @ModelID   
   
	SELECT @ManufacturerID = ManufacturerID   
	FROM Vehicles.Dbo.Make    
	WHERE id = @MakeID   
   
	SELECT @RegionID = RegionID   
	FROM Vehicles.Dbo.Manufacturer    
	WHERE id = @ManufacturerID   
   
	SELECT @GraphicsVehicleID = GraphicsVehicleID   
	FROM Vehicles.dbo.GraphicsAvailable GraphicsAvailable    
	WHERE GraphicsAvailable.VehicleID = @VehicleID   
   
	SELECT @MitchellServiceID = NULL   
   
	SELECT @GraphicsVehicleID = ISNULL(@GraphicsVehicleID,@VehicleID)   
   
	SELECT @Status = 	CASE 	WHEN ISNULL(EstimationData.LockLevel,0) = 0 THEN 'OPEN'   
					WHEN EstimationData.LockLevel = 99 THEN 'CLOSED'   
					ELSE 'Supplement ' + CONVERT(VarChar(2),EstimationData.LockLevel)   
				END   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @CurrentProfileAdminInfoID =  CustomerProfiles.AdminInfoID,   
		@EstimateProfileID = AdminInfo.CustomerProfilesID,   
		@Archived = AdminInfo.Archived   
	FROM FocusWrite.dbo.AdminInfo AdminInfo    
	LEFT JOIN FocusWrite.dbo.CustomerProfiles CustomerProfiles  ON   
		(CustomerProfiles.id = AdminInfo.CustomerProfilesID)   
   
	WHERE AdminInfo.ID = @AdminInfoID   
   
	SELECT -1 'RegionID',   
		ISNULL(@ManufacturerID,-1) 'ManufacturerID',   
		ISNULL(@MakeID,-1) 'MakeID',   
		ISNULL(@ModelID,-1) 'ModelID',   
		ISNULL(@ModelYearID,-1) 'ModelYearID',   
		ISNULL(@VehicleID,-1) 'VehicleID',   
		@Status 'Status',   
		@GraphicsVehicleID 'GraphicsVehicleID',   
		@BodyType 'BodyTypeID',   
		@DriveType 'DriveTypeID',   
		@MitchellServiceID 'MitchellServiceID',   
		@CurrentProfileAdminInfoID 'CurrentProfileAdminInfoID',   
		@EstimateProfileID 'EstimateProfileID',   
		ISNULL(@Archived,0) 'Archived'   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                       **/   
/**********************************************************************************************************/   
IF @Function = 'GetExpandList' AND   
	@GroupNumber IS NOT NULL   
BEGIN   
	/*IF @VehicleID = 35826   
		EXECUTE GetExpandList    
			@VehicleID = @VehicleID,   
			@GroupNumber = @GroupNumber   
	ELSE*/   
		EXECUTE Mitchell3.dbo.GetExpandListMitchell    
			@VehicleID = @VehicleID,   
			@GroupNumber = @GroupNumber,    
			@BodyType = @BodyType   
   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetCodes'    
BEGIN   
	-- States    
   
	SELECT '' 'Code','' 'Description'   
	UNION   
	SELECT *    
	FROM (   
		SELECT TOP 100 Code, Description   
		FROM CodeList    
		WHERE MasterCodeListTabsID = 48 AND   
			EntryType = 'USState'   
		ORDER BY Description) T   
   
	-- Phones    
	SELECT '' 'Code','' 'Description'   
	UNION   
	SELECT *    
	FROM (   
		SELECT TOP 100 Code, Description   
		FROM CodeList    
		WHERE MasterCodeListTabsID = 11 And   
			EntryType = 'Phone'   
		ORDER BY Description) T   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetCodeList' AND   
	@GroupNumber IS NOT NULL AND   
	@EntryType IS NULL   
BEGIN   
	SELECT '' 'Code','' 'Description'   
	UNION   
	SELECT *    
	FROM (   
		SELECT TOP 100 Code, Description   
		FROM CodeList    
		WHERE MasterCodeListTabsID = @GroupNumber   
		ORDER BY Description) T   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetCodeList' AND   
	@GroupNumber IS NOT NULL AND   
	@EntryType IS NOT NULL   
BEGIN   
	SELECT '' 'Code','' 'Description'   
	UNION   
	SELECT *    
	FROM (   
		SELECT TOP 100 Code, Description   
		FROM CodeList    
		WHERE MasterCodeListTabsID = @GroupNumber AND   
			EntryType = @EntryType   
		ORDER BY Description) T   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
/*IF @Function = 'GetSectionParts' AND   
	@SectionID IS NOT NULL AND   
	@AdminInfoID IS NOT NULL AND   
	@VehicleID IS NOT NULL AND   
	@BodyType IS NOT NULL   
BEGIN   
	IF (	SELECT VehicleInfo.VehicleID    
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId= EstimationData.ID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID	) = 35826   
	BEGIN   
		EXECUTE App_Functions_Honda    
			@Function = 'GetSectionParts',   
			@StepID = @SectionID,   
			@AdminInfoID = @AdminInfoID,   
			@VehicleID = @VehicleID,   
			@SortField = @SortField,   
			@SortDirection = @SortDirection   
	END   
END*/   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'CreateEstimateReport'    
BEGIN   
	SELECT @EstimationDataID = NULL   
	SELECT @EstimationDataID = ID   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	INSERT INTO EstimationReportList  (EstimationDataID)   
				VALUES (@EstimationDataID)   
   
	SELECT @Return = @@IDENTITY   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetActionList'   
BEGIN	   
	SELECT *   
	FROM ActionList    
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetCustomerProfileList' AND   
	@LoginsID IS NOT NULL AND   
	@SessionID IS NOT NULL   
BEGIN	   
	SELECT @Return = NULL   
	SELECT @AppraiserFlag = 0   
	   
	SELECT @AppraiserFlag = ISNULL(Logins.Appraiser,0)   
	FROM AdminInfo    
	INNER JOIN Logins  ON   
		(Logins.id = AdminInfo.CreatorID)   
	WHERE AdminInfo.ID = @AdminInfoID   
   
	SELECT TOP 1 @Return = CustomerProfiles.ID    
	FROM CustomerProfiles    
	WHERE OwnerType = 0 AND   
		OwnerID = @LoginsID AND   
		DefaultFlag = 1   
   
	SELECT @Return = ISNULL(@Return,-1)   
   
	SELECT @Return2 = NULL   
	SELECT @Return2 = AdminInfo.CustomerProfilesID   
	FROM AdminInfo    
	INNER JOIN Sessions  ON   
		(Sessions.AdminInfoID = AdminInfo.ID)   
	WHERE SessionID = @SessionID   
   
	IF @Return2 IS NULL and @Return <> -1   
	BEGIN   
		UPDATE AdminInfo    
		SET CustomerProfilesID = @Return   
		FROM AdminInfo    
		INNER JOIN Sessions  ON   
			(Sessions.AdminInfoID = AdminInfo.ID)   
		WHERE SessionID = @SessionID   
   
		SELECT @Return2 = @Return   
	END   
   
	SELECT @Return2 'Selected', @Return 'Default'   
   
	IF @AddBlank <> 0    
		SELECT -1 'ID',   
			CONVERT(VarChar(50),'') 'ProfileName',   
			CONVERT(VarChar(500),'') 'Description',   
			CONVERT(Bit,0) 'DefaultFlag',   
			CONVERT(Bit,0) 'PresetDefaultFlag',   
			CONVERT(DateTime,NULL) 'CreationDate',   
			CONVERT(VarChar(50),'') 'CurrentProfile'   
		UNION   
		SELECT CustomerProfiles.id,    
			REPLACE(CustomerProfiles.ProfileName,'&nbsp;','') 'ProfileName',    
			REPLACE(CustomerProfiles.Description,'&nbsp;','') 'Description',    
			CustomerProfiles.DefaultFlag,    
			ISNULL(CustomerProfiles.DefaultPreset,0) 'PresetDefaultFlag',    
			CustomerProfiles.CreationDate,   
			CASE	WHEN CustomerProfiles.id = @Return2 AND @AppraiserFlag = 0 AND @Return2 <> @Return THEN '<BR><B><I>Current Estimate''s Profile</I></B>'   
				WHEN CustomerProfiles.id = @Return2 AND @AppraiserFlag <> 0 AND @Return2 <> @Return THEN '<BR><B><I>Current Appraisal''s Profile</I></B>'   
				ELSE ''   
			END 'CurrentProfile'   
		FROM CustomerProfiles    
		WHERE OwnerType = 0 AND   
			OwnerID = @LoginsID AND   
			(CustomerProfiles.AdminInfoID IS NULL OR CustomerProfiles.id = @Return2)   
	ELSE   
		SELECT CustomerProfiles.id,    
			REPLACE(CustomerProfiles.ProfileName,'&nbsp;','') 'ProfileName',    
			REPLACE(CustomerProfiles.Description,'&nbsp;','') 'Description',    
			CustomerProfiles.DefaultFlag,    
			ISNULL(CustomerProfiles.DefaultPreset,0) 'PresetDefaultFlag',    
			CustomerProfiles.CreationDate,   
			CASE	WHEN CustomerProfiles.id = @Return2 AND @AppraiserFlag = 0 AND @Return2 <> @Return THEN '<BR><B><I>Current Estimate''s Profile</I></B>'   
				WHEN CustomerProfiles.id = @Return2 AND @AppraiserFlag <> 0 AND @Return2 <> @Return THEN '<BR><B><I>Current Appraisal''s Profile</I></B>'   
				ELSE ''   
			END 'CurrentProfile'   
		FROM CustomerProfiles    
		WHERE OwnerType = 0 AND   
			OwnerID = @LoginsID AND   
			(CustomerProfiles.AdminInfoID IS NULL OR CustomerProfiles.id = @Return2)   
END   
/**********************************************************************************************************/   
   
IF @Function = 'UpdateChosenProfile' AND   
	@AdminInfoID IS NOT NULL AND    
	@ProfileID IS NOT NULL   
BEGIN	   
	UPDATE AdminInfo    
	SET CustomerProfilesID = @ProfileID   
	FROM AdminInfo    
	WHERE AdminInfo.ID = @AdminInfoID   
END   
	   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
if @Function in ('AddEstimateLineItem', 'UpdateEstimateLineItem') AND   
	ISNULL(@OtherPartsID, 0) <> 0    
BEGIN   
	SELECT @SourcePartNumber = OtherParts.SourcePartNumber,   
		@VendorID = OtherParts.SourceID    
	FROM Parts.dbo.OtherParts OtherParts   
	WHERE OtherParts.id = @OtherPartsID   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'AddEstimateLineItem' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT @ModifiesID = -1,	   
		@AcdCode = 'A',   
		@SupplementVersion = NULL   
   
	SELECT @EstimationDataID = EstimationData.ID,   
		@SupplementVersion = EstimationData.LockLevel,   
		@NextSeq = EstimationData.NextUniqueSequenceNumber   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @LastLineNumber = ISNULL(@LastLineNumber,0),   
		@Function = 'AddTheLineItem'   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                     **/   
/**********************************************************************************************************/   
IF @Function = 'RemoveEstimateLineItem'   
BEGIN   
	--PRINT 'IN RemoveEstimateLineItem'   
	IF @EstimationLineItemsID IS NOT NULL   
		SELECT @Function = 'DeleteEstimateLineItem'   
	ELSE   
	BEGIN   
		IF LEFT(@PartNumber,2)='**'   
			SELECT @EstimationLineItemsID = Max(EstimationLineITems.id),   
				@Function = 'DeleteEstimateLineItem'   
			FROM EstimationData    
			INNER JOIN EstimationLineITems  ON   
				(EstimationLineITems.EstimationDataID = EstimationData.id)   
			WHERE AdminInfoID = @AdminInfoID AND   
				EstimationLineITems.PartNumber = @PartNumber   
		--PRINT @EstimationLineItemsID   
	END   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'UpdateEstimateLineItem' AND   
	@EstimationLineItemsID IS NOT NULL   
   
BEGIN   
   
	SELECT @PartNumber = LTRIM(RTRIM(REPLACE(@PartNumber,'GM PART','')))   
   
	SELECT @LockedID = NULL,   
		@SupplementVersion = NULL,   
		@Return = @EstimationLineItemsID   
   
	SELECT @LockedID = EstimationData.EstimationLineItemIDLocked,   
		@SupplementVersion = EstimationData.LockLevel,   
		@AdminInfoID = EstimationData.AdminInfoID    
	FROM EstimationLineItems   
	INNER JOIN EstimationData ON   
		(EstimationData.id = EstimationLineItems.EstimationDataID)   
	WHERE EstimationLineItems.id = @EstimationLineItemsID    
   
	SELECT @NextSeq = EstimationData.NextUniqueSequenceNumber   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @LockedID = ISNULL(@LockedID,-1),   
		@SupplementVersion = ISNULL(@SupplementVersion,0)   
	IF @EstimationLineItemsID > @LockedID OR @UpdateBaseRecord = 1  
	BEGIN   
		UPDATE EstimationLineItems    
		SET	StepID = @StepID,    
			SectionID = @SectionID,    
			PartSource = @PartSource,    
			ActionCode = REPLACE(@Operation,'&','+'),    
			ActionDescription = @OpDesc,    
			PartNumber = @PartNumber,    
			Description = LEFT(@Description,80),    
			Price = @Price,    
			PartOfOverhaul = @Overhaul,    
			Qty = @Qty,   
			SubletOperationFlag = @OperationSublet,    
			SubletPartsFlag = @PartSublet,   
			PartSourceVendorID = @VendorID,   
			SupplementVersion = @SupplementVersion,   
			CustomerPrice  = @CustomerPrice,   
			BettermentType = @BettermentType,   
			BettermentValue = @BettermentValue,   
			BettermentParts = @PartBetterment,   
			BettermentMaterials = @MaterialsBetterment,   
			VehiclePosition = @VehiclePosition,   
			Barcode = @Barcode,   
			dbPrice = @dbPrice,   
			SourcePartNumber = @SourcePartNumber,   
			@AutomaticCharge = ISNULL(AutomaticCharge,@AutomaticCharge),   
			@AutoAddBarcodeParent = ISNULL(AutoAddBarcodeParent,@AutoAddBarcodeParent),  
			IsPartsQuantity = @IsPartsQuantity,  
			IsLaborQuantity = @IsLaborQuantity,  
			IsPaintQuantity = @IsPaintQuantity,
			IsOtherCharges = @IsOtherCharges,
			LaborIncluded = @LaborIncluded
		WHERE ID = @EstimationLineItemsID   
	   
		UPDATE EstimationLineItems    
		SET Qty = @Qty - dbo.GetLineItemQuantity(ModifiesID)   
		WHERE id = @EstimationLineItemsID AND SupplementVersion > 0   
   
		IF @ExternalNotes <> ''   
		BEGIN   
			UPDATE EstimationNotes   
			SET	Printed = 1,   
				Notes = @ExternalNotes   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				Printed = 1   
   
			IF @@ROWCOUNT = 0    
				INSERT INTO EstimationNotes (EstimationLineItemsID, Printed, Notes)   
					VALUES (@EstimationLineItemsID, 1, @ExternalNotes)   
		END   
		ELSE   
			DELETE FROM EstimationNotes    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				Printed = 1   
   
		IF @InternalNotes <> ''   
		BEGIN   
			UPDATE EstimationNotes    
			SET	Printed = 0,   
				Notes = @InternalNotes   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				Printed = 0   
   
			IF @@ROWCOUNT = 0    
				INSERT INTO EstimationNotes (EstimationLineItemsID, Printed, Notes)   
						VALUES (@EstimationLineItemsID, 0, @InternalNotes)   
		END   
		ELSE   
			DELETE FROM EstimationNotes    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				Printed = 0   
		   
		--DELETE FROM EstimationLineLabor   
		--WHERE EstimationLineItemsID = @EstimationLineItemsID   
	   
		/****** Labor ******/   
		IF @LaborType in (1,2,3,4,5,6,8,24,25,32)  AND abs(@LaborTime) >= 0.01   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = @LaborType,    
				LaborSubType = 0,    
				LaborTime = @LaborTime,    
				LaborCost = NULL,    
				BettermentFlag = @LaborBetterment,    
				SubletFlag = @LaborSublet,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (1,2,3,4,5,6,8,24,25,32)   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType in (1,2,3,4,5,6,8,24,25,32)   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, @LaborType, 0, @LaborTime, NULL, @LaborBetterment, @LaborSublet, @NextSeq, @dbLaborTime,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (1,2,3,4,5,6,8,24,25,32)   
		/****** End Labor ******/   
   
		/****** Paint ******/   
		IF @PaintType in (9,16,18,19,29)    -- Ezra 6/27 paint type can be set without an allowance and we still want it to save -- AND abs(ISNULL(@PaintTime,0)) >= 0.01   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = @PaintType,    
				LaborSubType = 0,    
				LaborTime = @PaintTime,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				AdjacentDeduction = @AdjacentDeduction,   
				MajorPanel = @MajorPanel,   
				AdjacentDeductionLock = @AdjacentDeductionLock,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (9,16,18,19,29)   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType in (9,16,18,19,29)   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, AdjacentDeduction, MajorPanel, UniqueSequenceNumber, dbLaborTime, AdjacentDeductionLock,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, @PaintType, 0, @PaintTime, NULL, @PaintBetterment, @PaintSublet, @AdjacentDeduction, @MajorPanel, @NextSeq, @dbLaborTime, @AdjacentDeductionLock,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (9,16,18,19,29)   
		/****** End Paint ******/   
   
		/****** Other ******/	   
		IF @Other in (13,14,15,30,31)  AND abs(@OtherCharge) <> 0   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = @Other,    
				LaborSubType = 0,    
				LaborTime = NULL,    
				LaborCost = @OtherCharge,    
				BettermentFlag = @LaborBetterment,    
				SubletFlag = @LaborSublet,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (13,14,15,30,31)   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType in (13,14,15,30,31)   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, @Other, 0, NULL, @OtherCharge, @LaborBetterment, @LaborSublet, @NextSeq, @dbLaborTime,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType in (13,14,15,30,31)   
		/****** End Other ******/   
   
		/****** Clearcoat ******/	   
		IF abs(@ClearcoatTime) >= 0.01 OR @LockClearcoat <> 0   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = 20,    
				LaborSubType = 0,    
				LaborTime = @ClearcoatTime,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockClearcoat,    
				Include = @IncludeClearcoat,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 20   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor   
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 20   
   
				INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 20, 0, @ClearcoatTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockClearcoat, @IncludeClearcoat,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
		BEGIN   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 20   
			SELECT @RowCount = @@ROWCOUNT    
		END   
		/****** End Clearcoat ******/   
   
		/****** Blend ******/	   
		IF abs(@BlendTime) >= 0.01 OR @LockBlend <> 0   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = 26,    
				LaborSubType = 0,    
				LaborTime = @BlendTime,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockBlend,   
				Include = @IncludeBlend,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 26   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 26   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 26, 0, @BlendTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockBlend, @IncludeBlend,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 26   
		/****** End Blend ******/   
   
		/****** 2 Tone Allowance ******/			   
		IF (abs(@Allowance) >= 0.01 OR @LockAllowance <> 0) AND @PaintType = 29	--2 Tone Allowance   
		BEGIN   
			UPDATE EstimationLineLabor   
			SET	LaborType = 28,    
				LaborSubType = 0,    
				LaborTime = @Allowance,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockAllowance,   
				Include = @IncludeAllowance,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 28   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 28   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 28, 0, @Allowance, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockAllowance, @IncludeAllowance,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 28   
		/****** End 2 Tone Allowance ******/   
   
		/****** 3 Stage Allowance ******/			   
		IF (abs(@Allowance) >= 0.01 OR @LockAllowance <> 0) AND @PaintType = 18	--3 Stage Allowance   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = 27,    
				LaborSubType = 0,    
				LaborTime = @Allowance,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockAllowance,   
				Include = @IncludeAllowance,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 27   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 27   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 27, 0, @Allowance, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockAllowance, @IncludeAllowance,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 27   
		/****** End 3 Stage Allowance ******/   
   
		/****** Edging ******/   
		IF abs(@EdgingTime) >= 0.01 OR @LockEdging <> 0   
		BEGIN   
			UPDATE EstimationLineLabor   
			SET	LaborType = 21,    
				LaborSubType = 0,    
				LaborTime = @EdgingTime,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockEdging,   
				Include = @IncludeEdging,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 21   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 21   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 21, 0, @EdgingTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockEdging, @IncludeEdging,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 21   
		/****** End Edging ******/   
   
		/****** Underside ******/   
		IF abs(@UndersideTime) >= 0.01 OR @LockUnderside <> 0   
		BEGIN   
			UPDATE EstimationLineLabor    
			SET	LaborType = 22,    
				LaborSubType = 0,    
				LaborTime = @UndersideTime,    
				LaborCost = NULL,    
				BettermentFlag = @PaintBetterment,    
				SubletFlag = @PaintSublet,   
				Lock = @LockUnderside,   
				Include = @IncludeUnderside,
				BettermentPercentage = @BettermentValue,
				BettermentType = @BettermentType   
			FROM EstimationLineLabor   
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 22   
   
			IF @@ROWCOUNT = 0   
			BEGIN   
				DELETE FROM EstimationLineLabor    
				WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
					LaborType = 22   
   
				INSERT INTO EstimationLineLabor (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Lock, Include,BettermentPercentage,BettermentType)   
					SELECT @EstimationLineItemsID, 22, 0, @UndersideTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @LockUnderside, @IncludeUnderside,@BettermentValue, @BettermentType   
				SELECT @NextSeq = @NextSeq + 1   
			END   
		END   
		ELSE   
			DELETE FROM EstimationLineLabor    
			WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
				LaborType = 22   
		/****** End Underside ******/   
   
		--PRINT @NextSeq   
		UPDATE EstimationData    
		SET NextUniqueSequenceNumber = @NextSeq   
		WHERE AdminInfoID = @AdminInfoID   
   
		--TODO:  Overlaps need recalculated whenever the operation time has changed.   
		--EXECUTE FixAdjacencies @AdminInfoID   
		--EXECUTE ShowHideAutoPaints @AdminInfoID   
		 
		--EXECUTE UpdateAdminTotals @AdminInfoID = @AdminInfoID   
		--IF @NoOverlapCheck = 0   
		--BEGIN   
		--EXECUTE Mitchell3.dbo.GetOverlapChanges @AdminInfoID--, @AutoPaint, @PaintType   
		--END   
	END   
	ELSE   
	BEGIN   
		-- Here we add a changed record   
		SELECT @ModifiesID = @EstimationLineItemsID,	   
			@AcdCode = 'C',   
			@SupplementVersion = NULL   
   
		SELECT @SupplementVersion = EstimationData.LockLevel,   
			@EstimationDataID = EstimationData.id,   
			@LineNumberModified = EstimationLineItems.LineNumber ,   
			@AdminInfoID = EstimationData.AdminInfoID, 
			@LineItemLaborTime = EstimationLineItems.LaborTime 
		FROM EstimationData    
		INNER JOIN EstimationLineItems ON   
			(EstimationLineItems.EstimationDataID = EstimationData.ID)   
		WHERE EstimationLineItems.ID = @EstimationLineItemsID   
   
		SELECT @Function = 'AddTheLineItem'  --,   
			-- @OpDesc = 'Modifies line #  ' + CONVERT(VarChar(12),@LineNumberModified) + '. ' + ISNULL(@OpDesc,'')   
	END   
	--Do overlap processing for tim only   
END   
/**********************************************************************************************************/   
   
IF @Function = 'DeleteBarcodeRep' AND   
	@barcode IS NOT NULL   
BEGIN   
	SELECT @EstimationLineItemsID = NULL   
	SELECT @EstimationLineItemsID = EstimationLineItems.id   
	FROM EstimationLineItems    
	INNER JOIN EstimationData  ON   
		(EstimationData.id = EstimationLineItems.EstimationDataID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID AND   
		EstimationLineItems.Barcode = @barcode   
	SELECT @Function = 'DeleteEstimateLineItem'   
   
END   
   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'DeleteEstimateLineItem' AND   
	@EstimationLineItemsID IS NOT NULL   
BEGIN   
	SELECT @PartNumber = LTRIM(RTRIM(REPLACE(@PartNumber,'GM PART','')))   
	SELECT @LockedID = NULL   
   
	SELECT @LockedID = EstimationData.EstimationLineItemIDLocked,   
		@SupplementVersion = EstimationData.LockLevel,   
		@NextSeq = EstimationData.NextUniqueSequenceNumber,   
		@barcode = EstimationLineItems.Barcode,   
		@AdminInfoID = EstimationData.AdminInfoID   
	FROM EstimationLineItems    
	INNER JOIN EstimationData  ON   
		(EstimationData.id = EstimationLineItems.EstimationDataID)   
	WHERE EstimationLineItems.id = @EstimationLineItemsID    
   
	SELECT @LockedID = ISNULL(@LockedID,-1)   
	IF @EstimationLineItemsID > @LockedID	   
	BEGIN   
		SELECT @AdminInfoID = EstimationData.AdminInfoID   
		FROM EstimationData    
		INNER JOIN EstimationLineItems  ON   
			(EstimationLineItems.EstimationDataID = EstimationData.ID)   
		WHERE EstimationLineItems.ID = @EstimationLineItemsID   
   
		DELETE FROM EstimationLineLabor    
		WHERE EstimationLineItemsID = @EstimationLineItemsID   
	   
		DELETE FROM EstimationNotes    
		WHERE EstimationLineItemsID = @EstimationLineItemsID   
   
		UPDATE EstimationLineItems    
		SET PartOfOverhaul = 0   
		FROM EstimationLineItems    
		INNER JOIN (	SELECT EstimationLineItems2.id   
				FROM EstimationLineItems    
				INNER JOIN EstimationOverlap  ON   
					(EstimationOverlap.EstimationLineItemsID2 = EstimationLineItems.id)   
				INNER JOIN EstimationLineItems EstimationLineItems2  ON   
					(EstimationLineItems2.id = EstimationOverlap.EstimationLineItemsID1)   
				WHERE EstimationLineItems.id = @id AND   
					EstimationLineItems.ActionCode = 'Over'	) T ON   
			(T.id = EstimationLineItems.id)   
	   
		DELETE FROM  EstimationLineItems    
		WHERE ID = @EstimationLineItemsID   
		SELECT @Function = 'RedoLineNumbers'   
   
   
		-- EstimationLineItemsID1   
		DELETE FROM EstimationOverlap   
		WHERE 	EstimationLineItemsID1 = @EstimationLineItemsID   
		   
		-- EstimationLineItemsID2   
		DELETE FROM EstimationOverlap   
		WHERE 	EstimationLineItemsID2 = @EstimationLineItemsID   
   
   
		-- ActionID   
		DELETE FROM EstimationOverlapProposed   
		WHERE 	ActionID = @EstimationLineItemsID   
		   
		-- ResultID   
		DELETE FROM EstimationOverlapProposed   
		WHERE 	ResultID = @EstimationLineItemsID   

		-- Delete Add Ons
		DELETE FROM EstimationLineItems
		WHERE ParentLineID = @EstimationLineItemsID
   
   
		--EXECUTE CheckForAutoincludesToDelete @AdminInfoID   
		--EXECUTE FixAdjacencies @AdminInfoID Commented By Josh 8/31   
		--EXECUTE ShowHideAutoPaints @AdminInfoID   
		--EXECUTE UpdateAdminTotals @AdminInfoID = @AdminInfoID   
	END   
	ELSE   
	BEGIN   
		SELECT @AdminInfoID = EstimationData.AdminInfoID   
		FROM EstimationData    
		INNER JOIN EstimationLineItems  ON   
			(EstimationLineItems.EstimationDataID = EstimationData.ID)   
		WHERE EstimationLineItems.ID = @EstimationLineItemsID   
   
		-- Here we add a Delete record   
		SELECT @SupplementVersion = EstimationData.LockLevel,   
			@EstimationDataID = EstimationData.id,   
			@LineNumberModified = EstimationLineItems.LineNumber,   
			@AdminInfoID = EstimationData.AdminInfoID   
		FROM EstimationData    
		INNER JOIN EstimationLineItems  ON   
			(EstimationLineItems.EstimationDataID = EstimationData.ID)   
		WHERE EstimationLineItems.ID = @EstimationLineItemsID   
/*   
		UPDATE EstimationOverlap    
		SET	UserAccepted = 0   
		FROM EstimationOverlap   
		WHERE 	EstimationLineItemsID1 = @EstimationLineItemsID OR   
			EstimationLineItemsID2 = @EstimationLineItemsID   
*/   
   
		--EXECUTE CheckForAutoincludesToDelete @AdminInfoID		**** This needs to mark the autoincludes as deleted   
		SELECT @ModifiesID = @EstimationLineItemsID,	   
			@AcdCode = 'D',   
			@OpDesc = 'Delete Line # ' + CONVERT(VarChar(4),@LineNumberModified)   
		SELECT @Function = 'AddTheLineItem'   
		--EXECUTE FixAdjacencies @AdminInfoID   
		--EXECUTE ShowHideAutoPaints @AdminInfoID   
		--EXECUTE UpdateAdminTotals @AdminInfoID = @AdminInfoID   
	END   
   
	SELECT @LockedID = EstimationData.EstimationLineItemIDLocked,   
		@SupplementVersion = EstimationData.LockLevel,   
		@NextSeq = EstimationData.NextUniqueSequenceNumber,   
		@barcode = EstimationLineItems.Barcode   
	FROM EstimationLineItems    
	INNER JOIN EstimationData  ON   
		(EstimationData.id = EstimationLineItems.EstimationDataID)   
	WHERE EstimationLineItems.id = @EstimationLineItemsID    
   
	IF @barcode NOT LIKE '%REP'   
	BEGIN   
		SELECT @RepEstimationLineItemsID = NULL   
		SELECT 	@RepEstimationLineItemsID = EstimationLineItems.ID		   
		FROM EstimationLineItems    
		INNER JOIN EstimationData  ON   
			(EstimationData.id = EstimationLineItems.EstimationDataID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID AND   
			EstimationLineItems.barcode = @barcode+'REP'   
		IF @RepEstimationLineItemsID IS NOT NULL   
		BEGIN   
			EXECUTE App_Functions   
				@Function = 'DeleteEstimateLineItem',   
				@EstimationLineItemsID = @RepEstimationLineItemsID   
		END   
	END   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'AddTheLineItem'   
BEGIN   
	--Here we actually add the record  
	  
	   
	/*  
	-- Ezra - 4/16/2018 - commented this section out.  Not sure why it's here but it prevents supplement data from saving.  
	IF @PartNumber LIKE '**%'   
	BEGIN   
		IF EXISTS (	SELECT EstimationLineItems.PartNumber   
				FROM EstimationData    
				INNER JOIN EstimationLineItems  ON   
					(EstimationLineItems.EstimationDataID = EstimationData.id)   
				WHERE EstimationLineItems.PartNumber = @PartNumber AND   
					ISNULL(EstimationLineItems.SupplementVersion,0) = 0 AND   
					EstimationData.AdminInfoID = @AdminInfoID	)   
			RETURN   
	END   
	*/  
  
--PRINT 'SELECT @Service_Barcode = '   
	SELECT @Service_Barcode = FocusWrite.Dbo.GetServiceBarcode(@AdmininfoID)   
--PRINT 'Max(EstimationLineItems.LineNumber)'   
	SELECT @LastLineNumber = dbo.GetLastLineNumber(@AdminInfoID)  
   
	SELECT	@NextSeq = EstimationData.NextUniqueSequenceNumber,   
		@VehicleID = VehicleID   
	FROM EstimationData    
	LEFT JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataId = EstimationData.id)   
	WHERE AdminInfoID = @AdminInfoID   
   
    declare @x DATETIME   
	   
	  -- Ezra - 5/6/2019 - On manual entry pages we were getting a bug where sometimes the entry wouldn't save.  I believe this was the cause.  I commented this out and changed the front end pages to  
	  -- disable the save button until getting a response from the server 
 
	--SELECT @X = isnull(Date_Entered,DATEADD(ss,-4,GETDATE()))   
	--FROM EstimationLineItems WITH(NOLOCK)   
	--WHERE EstimationDataID = @EstimationDataID   
	--AND StepID = @StepID   
	--AND SectionID = @SectionID    
	--AND PartNumber =  @PartNumber   
	--AND Barcode = @Barcode   
    
	if (@X is null or datediff(ss,@x,getdate()) >=3)   
	begin   
   
	INSERT INTO EstimationLineItems  (EstimationDataID, StepID, SectionID, PartSource, ActionCode, ActionDescription,    
							PartNumber, Description, Price, PartOfOverhaul, Qty,    
							SubletOperationFlag, SubletPartsFlag,    
							PartSourceVendorID, LineNumber, ACDCode, SupplementVersion, ModifiesID,    
							CustomerPrice, BettermentValue, BettermentType, UniqueSequenceNumber, VehiclePosition,    
							Barcode, dbPrice, SourcePartNumber, AutomaticCharge, AutoAddBarcodeParent, LaborTime,
							IsPartsQuantity, IsLaborQuantity, IsPaintQuantity, IsOtherCharges, LaborIncluded,BettermentParts,BettermentMaterials)   
				VALUES (@EstimationDataID, @StepID, @SectionID, @PartSource, REPLACE(@Operation,'&','+'), @OpDesc,    
							@PartNumber, LEFT(@Description,80), @Price, @Overhaul, @Qty,    
							@OperationSublet, @PartSublet,    
							@VendorID, @LastLineNumber+1, @AcdCode, @SupplementVersion, @ModifiesID,    
							@CustomerPrice, @BettermentValue, @BettermentType, @NextSeq, @VehiclePosition, @Barcode, @dbPrice,    
							@SourcePartNumber, @AutomaticCharge, @AutoAddBarcodeParent, @LineItemLaborTime,
							@IsPartsQuantity,@IsLaborQuantity,@IsPaintQuantity, @IsOtherCharges, @LaborIncluded,@PartBetterment, @MaterialsBetterment)   
   
	DECLARE @originalLineItemID INT = @EstimationLineItemsID   
	SELECT @EstimationLineItemsID = @@IDENTITY   
	SELECT @Return = @EstimationLineItemsID   
	SELECT @NextSeq = @NextSeq + 1   
   
	-- if a new line was added, update the quantity value.  This is the difference between the passed value adn the current value   
	UPDATE EstimationLineItems    
	SET Qty = @Qty - dbo.GetLineItemQuantity(ModifiesID)   
	WHERE id = @EstimationLineItemsID AND SupplementVersion > 0   
   
	IF @ExternalNotes <> ''   
		INSERT INTO EstimationNotes  (EstimationLineItemsID, Printed, Notes)   
			VALUES (@EstimationLineItemsID, 1, @ExternalNotes)   
	IF @InternalNotes <> ''   
		INSERT INTO EstimationNotes  (EstimationLineItemsID, Printed, Notes)   
				VALUES (@EstimationLineItemsID, 0, @InternalNotes)   
	   
	IF @LaborType IS NOT NULL  AND abs(@LaborTime) >= 0.01   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, AdjacentDeductionLock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, @LaborType, 0, @LaborTime, NULL, @LaborBetterment, @LaborSublet, @NextSeq, @dbLaborTime, @AdjacentDeductionLock,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
--PRINT '@PaintTime='+ISNULL(CONVERT(VarChar(20),@PaintTime),'NULL')   
--PRINT '@PaintType='+ISNULL(CONVERT(VarChar(20),@PaintType),'NULL')   
   
	-- Ezra - 1/25/17 - for blend we need to be able to add no paint time but still have a paint type   
	--IF abs(@PaintTime) >= 0.01 AND @PaintType IS NOT NULL   
	--BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, AdjacentDeduction, MajorPanel, UniqueSequenceNumber, dbLaborTime, AdjacentDeductionLock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, ISNULL(@PaintType, 0), 0, @PaintTime, NULL, @LaborBetterment, @LaborSublet, @AdjacentDeduction, @MajorPanel, @NextSeq, @dbLaborTime, @AdjacentDeductionLock,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	--END   
   
	IF @Other IS NOT NULL  AND abs(@OtherCharge) > 0 
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime)   
			SELECT @EstimationLineItemsID, @Other, 0, NULL, @OtherCharge, 0,0, @NextSeq, @dbLaborTime   
		SELECT @NextSeq = @NextSeq + 1   
	END   
   
	IF abs(@ClearcoatTime) >= 0.01   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 20, 0, @ClearcoatTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeClearcoat, @LockClearcoat,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
   
	IF abs(@BlendTime) >= 0.01   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 26, 0, @BlendTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeBlend, @LockBlend,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
		   
	IF abs(@Allowance) >= 0.01 AND @PaintType = 29	--2 Tone Allowance   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 28, 0, @Allowance, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeAllowance, @LockAllowance,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
		   
	IF abs(@Allowance) >= 0.01 AND @PaintType = 18	--3 Stage Allowance   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 27, 0, @Allowance, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeAllowance, @LockAllowance,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
		   
	IF abs(@EdgingTime) >= 0.01   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 21, 0, @EdgingTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeEdging, @LockEdging,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
		   
	IF abs(@UndersideTime) >= 0.01   
	BEGIN   
		INSERT INTO EstimationLineLabor  (EstimationLineItemsID, LaborType, LaborSubType, LaborTime, LaborCost, BettermentFlag, SubletFlag, UniqueSequenceNumber, dbLaborTime, Include, Lock,BettermentPercentage,BettermentType)   
			SELECT @EstimationLineItemsID, 22, 0, @UndersideTime, NULL, @PaintBetterment, @PaintSublet, @NextSeq, @dbLaborTime, @IncludeUnderside, @LockUnderside,@BettermentValue, @BettermentType   
		SELECT @NextSeq = @NextSeq + 1   
	END   
   
	UPDATE EstimationData    
	SET NextUniqueSequenceNumber = @NextSeq   
	WHERE AdminInfoID = @AdminInfoID   
   
	IF @NoOverlapCheck = 0   
	BEGIN   
		--PRINT 'AddOverlapInfo'   
		EXECUTE AddOverlapInfo	@EstimationLineItemsID, @VehicleID, @AdminInfoID, @SectionID, @StepID   
	END   
   
	--PRINT 'UpdateAdminTotals'   
	--EXECUTE UpdateAdminTotals @AdminInfoID = @AdminInfoID   
	--PRINT 'FixAdjacencies'   
	--EXECUTE FixAdjacencies @AdminInfoID Commented By Josh 8/31   
	--PRINT 'ShowHideAutoPaints'   
	--EXECUTE ShowHideAutoPaints @AdminInfoID   
       
	-- Ezra - Overlap charges are super slow, done now in a background thread after this sp returns.   
--	IF @NoOverlapCheck = 0   
--	BEGIN   
--		PRINT 'GetOverlapChanges'   
--		 EXECUTE Mitchell3.dbo.PE_GetOverlapChanges @AdminInfoID--, @AutoPaint, @PaintType   
--	END   
	end   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetAccessoriesSelected' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT @AccessoriesSelected = '|'   
	SELECT @AccessoriesSelected = @AccessoriesSelected + CONVERT(VarChar(12), VehicleOptionsID) + '|'   
	FROM VehicleSelectedOptions    
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.ID = VehicleSelectedOptions.VehicleInfoID)   
	INNER JOIN EstimationData  ON   
		(EstimationData.ID = VehicleInfo.EstimationDataID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'SaveVehicleInfo' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	IF @VehicleID = -1 SELECT @VehicleID = NULL   
   
	SELECT @EstimationDataID = EstimationData.ID   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @VehicleInfoID = VehicleInfo.ID   
	FROM VehicleInfo    
	WHERE EstimationDataID = @EstimationDataID   
	   
	    
	    
	IF @ServiceBarcode is null    
	begin    
		select @ServiceBarcode = service_barcode from vinn.dbo.Vehicle_Service_Xref where VehicleID = @VehicleID   
	end      
	IF EXISTS (	SELECT ID FROM VehicleInfo  WHERE ID = @VehicleInfoID )   
	BEGIN    
		print 'a'   
		UPDATE VehicleInfo    
		SET	VehicleID = ABS(@VehicleID),    
			ExtColor = @ExtColor,    
			ExtColorCode = @ExteriorColorCode,    
			MilesIn = @MileageIn,    
			MilesOut = @MileageOut,    
			License = UPPER(@License),    
			State = @State,    
			Condition = @VehicleCondition,    
			IntColor = @IntColor,    
			IntColorCode = @InteriorColorCode,    
			TrimLevel = @TrimOption,   
			Vin = @Vin,   
			ProductionDate = @ProductionDate,   
			DefaultPaintType = @DefaultPaintType,   
			BodyType = ISNULL(@BodyType, BodyType),   
			DriveType = ISNULL(@DriveType, DriveType),    
			VehicleValue = @VehicleValue,   
			Service_Barcode = @ServiceBarcode   
		WHERE VehicleInfo.ID = @VehicleInfoID   
		   
		print @VehicleInfoID   
		print @ServiceBarcode   
		   
	END   
	ELSE   
	BEGIN print 'b'   
		INSERT INTO  VehicleInfo  (EstimationDataId, VehicleID, ExtColor, ExtColorCode, MilesIn, MilesOut, License, State, Condition, IntColor, IntColorCode, TrimLevel, Vin, ProductionDate, DefaultPaintType, BodyType, DriveType, VehicleValue,Service_barcode)   
		SELECT @EstimationDataID, ABS(@VehicleID), @ExtColor, @ExteriorColorCode, @MileageIn, @MileageOut, UPPER(@License), @State, @VehicleCondition, @IntColor, @InteriorColorCode, @TrimOption, @Vin, @ProductionDate, @DefaultPaintType, @BodyType, @DriveType, @VehicleValue,@ServiceBarcode   
		SELECT @VehicleInfoID = @@IDENTITY   
	END   
	DECLARE @VehicleOptionsIDTemp VarChar(200)   
	DECLARE @ElementNumber Int   
	DECLARE @VehicleOptionsID Int   
   
	/* Now update Options Selected */   
	DELETE FROM VehicleSelectedOptions     
	WHERE VehicleInfoID = @VehicleInfoID   
   
	SELECT @ElementNumber = 0   
	SELECT @VehicleOptionsIDTemp = FocusWrite.Dbo.Extract(@SelectedString, @ElementNumber, '|')   
	--PRINT @VehicleOptionsIDTemp   
	WHILE @VehicleOptionsIDTemp <> ''   
	BEGIN   
		SELECT @VehicleOptionsID = CONVERT(Int, @VehicleOptionsIDTemp)   
		INSERT INTO VehicleSelectedOptions  (VehicleInfoID, VehicleOptionsID) VALUES (@VehicleInfoID,@VehicleOptionsID)   
		SELECT @ElementNumber = @ElementNumber + 1   
		SELECT @VehicleOptionsIDTemp = FocusWrite.Dbo.Extract(@SelectedString, @ElementNumber, '|')	   
		--PRINT @VehicleOptionsIDTemp   
	END   
   
END   
/**********************************************************************************************************/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetEstimateVehicleInfo' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT TOP 1   
		EstimationData.*,    
		VehicleInfo.*,   
		ServicesToVehicleID.BodyTypesID 'BodyCount',   
		ServicesToVehicleID.VinRequired   
	FROM EstimationData    
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataID = EstimationData.ID)   
	LEFT JOIN Mitchell3.Dbo.ServicesToVehicleID ServicesToVehicleID  ON   
		(ServicesToVehicleID.VehicleID = VehicleInfo.VehicleID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'LogError'   
BEGIN   
	INSERT INTO Errors  (LoginID, AdminInfoID, ErrorText, SessionVars, App)   
		VALUES (@LoginsID, @AdminInfoID, @ErrorText, @SessionInfo, @App)   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'LogException'   
BEGIN   
	INSERT INTO Exceptions  (LoginsID, AdminInfoID, ExceptionInfo)   
		VALUES (@LoginsID, @AdminInfoID, @ErrorText)   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'DeleteEstimate' AND   
	@AdminInfoID IS NOT NULL AND   
	@LoginsID IS NOT NULL   
BEGIN   
	--EXECUTE App_ImportExport   
	--	@Function = 'DeleteImportInfo',   
	--	@AdminInfoID = @AdminInfoID   
   
	--Link contacts to login so they can be used later   
	INSERT INTO LoginsToContacts  (LoginsID, COntactsID)   
		SELECT @LoginsID, AdminInfoTOContacts.ContactsID   
		FROM AdminInfo    
		INNER JOIN AdminInfoTOContacts  ON   
			(AdminInfoTOContacts.AdminInfoID = AdminInfo.ID)   
		INNER JOIN Contacts  ON   
			(COntacts.id = AdminInfoTOContacts.ContactsID)   
		WHERE AdminInfo.id = @AdminInfoID   
	   
	update AdminInfo set deleted = 1 where id = @AdminInfoID   
END   
/**********************************************************************************************************/   
   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'SetVehicleID' AND   
	@AdminInfoID IS NOT NULL AND   
	@VehicleID IS NOT NULL   
BEGIN   
	IF EXISTS (	SELECT ID FROM AdminInfo  WHERE ID = @AdminInfoID)   
	BEGIN   
		IF NOT EXISTS (	SELECT ID FROM EstimationData  WHERE AdminInfoID = @AdminInfoID)   
			INSERT INTO EstimationData  (AdminInfoID) VALUES (@AdminInfoID)   
		IF EXISTS (	SELECT VehicleInfo.ID    
				FROM VehicleInfo    
				INNER JOIN EstimationData  ON   
					(EstimationData.id = VehicleInfo.EstimationDataId)   
				WHERE EstimationData.AdminInfoID = @AdminInfoID)   
			UPDATE VehicleInfo    
			SET	VehicleID = ABS(@VehicleID)   
			FROM VehicleInfo    
			INNER JOIN EstimationData  ON   
				(EstimationData.id = VehicleInfo.EstimationDataId)   
			WHERE EstimationData.AdminInfoID = @AdminInfoID   
		ELSE   
			INSERT INTO VehicleInfo  (EstimationDataId, VehicleID)   
				SELECT Id, ABS(@VehicleID)   
				FROM EstimationData     
				WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
END   
/**********************************************************************************************************/   
   
IF @Function = 'CommitEstimate'  AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	IF (	SELECT Count(*)   
		FROM EstimationData   
		INNER JOIN EstimationLineItems ON   
			(EstimationLineItems.EstimationDataID = EstimationData.ID)   
		WHERE AdminInfoID = @AdminInfoID AND   
			ISNULL(EstimationLineItems.SupplementVersion, 0) = ISNULL(EstimationData.LockLevel, 0)	) > 0   
	BEGIN   
		UPDATE FocusWrite.dbo.EstimationData    
		SET 	LockLevel = ISNULL(EstimationData.LockLevel,0) + 1,   
			EstimationLineItemIDLocked = (	SELECT Max(EstimationLineItems.ID)   
							FROM EstimationLineItems    
							INNER JOIN EstimationData  ON   
								(EstimationData.id = EstimationLineItems.EstimationDataID) )   
		FROM FocusWrite.dbo.EstimationData EstimationData    
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
	ELSE   
	BEGIN   
		SELECT @SupplementVersion = ISNULL(LockLevel, 0)   
		FROM EstimationData    
		WHERE AdminInfoID = @AdminInfoID   
   
		IF @SupplementVersion = 0   
			SELECT 1 -- 'Estimate not committed since there are no items on the estimatate.' 'ErrorReturn'   
		ELSE   
			SELECT 2 --'Estimate not committed since there are no items on the current supplement level.' 'ErrorReturn'   
	END   
   
	SELECT 0   
END   
   
IF @Function = 'CloseAndLock'  AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	UPDATE FocusWrite.dbo.EstimationData    
	SET 	LockLevel = 99,   
		EstimationLineItemIDLocked = (	SELECT Max(EstimationLineItems.ID)   
						FROM EstimationLineItems    
						INNER JOIN EstimationData  ON   
							(EstimationData.id = EstimationLineItems.EstimationDataID) )   
	FROM FocusWrite.dbo.EstimationData EstimationData    
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END   
   
IF @Function = 'TieVin' AND   
	@VinDecode IS NOT NULL AND   
	--LEN(@VinDecode) > 100 AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	Declare @theCode float   
	set @theCode = convert(float,@barcode)   
	Declare @theYear int   
	set @theYear = convert(int, @ModelYearText)   
	select @SubModel = left(@SubModel,3)   
	EXECUTE TieVin    
		@VinDecode = @VinDecode,   
		@Vin = @Vin,   
		@AdminInfoID = @AdminInfoID,   
		@barcode = @theCode,   
		@year = @theYear,   
		@SubModel = @SubModel   
	   
END   
   
IF @Function = 'RedoLineNumbers' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	 EXECUTE RedoLineNumbers   
		@AdminInfoID = @AdminInfoID   
END	   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
--IF @Function = 'ChangeEstInfoOwnPage' AND   
--	@LoginsID IS NOT NULL AND   
--	@OwnPage IS NOT NULL   
--BEGIN   
--	UPDATE Logins    
--	SET EstInfoOwnPage = @OwnPage   
--	WHERE Logins.id = @LoginsID   
--END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
--IF @Function = 'GetShowEstInfoFlag' AND   
--	@LoginsID IS NOT NULL   
--BEGIN   
--	SELECT @Return = 	CASE	WHEN ISNULL(Logins.EstInfoOwnPage,0) <> 0 THEN 0    
--					ELSE 1   
--				END	   
--	FROM Logins    
--	WHERE id = @LoginsID   
--END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetPartsListByPartNumber' AND   
	@PartNumber IS NOT NULL AND   
	@LoginsID IS NOT NULL   
BEGIN   
	EXECUTE GetPartsListByPartNumber   
		@PartNumber = @PartNumber,   
		@LoginsID = @LoginsID   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetPartList' AND   
	@VehicleID IS NOT NULL AND   
	@GroupNumber IS NOT NULL --AND   
	--@PartNumber IS NOT NULL AND   
	--@VehicleID <> 35826   
BEGIN   
	EXECUTE GetPartsList2   
		@VehicleID = @VehicleID,   
		@GroupNumber = @GroupNumber,   
		@PartNumber = @PartNumber,   
		@Side = @Side,   
		@GraphicsViewer = @GraphicsVersion,   
		@AdminInfoID = @AdminInfoID,   
		@Reference = @Reference   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
IF @Function = 'GetPartInfo' AND   
	@PartID IS NOT NULL AND   
	@LoginsID IS NOT NULL   
BEGIN   
	EXECUTE GetPartInfo @PartID = @PartID, @LoginsID = @LoginsID   
END   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
/*IF @Function = 'GetPartList' AND   
	@VehicleID IS NOT NULL AND   
	@GroupNumber IS NOT NULL AND   
	@Reference IS NOT NULL AND   
	@AdminInfoID IS NOT NULL AND   
	@VehicleID = 35826   
BEGIN   
	IF @VehicleID = 35826   
	BEGIN	--PRINT 'App_Functions_Honda'   
		EXECUTE App_Functions_Honda   
			@Function = @Function,   
			@AdminInfoID = @AdminInfoID,   
			@VehicleID = @VehicleID,   
			@Reference = @Reference,   
			@GroupNumber = @GroupNumber   
	END   
END*/   
   
/**********************************************************************************************************/   
/**                                                                                                      **/   
/**********************************************************************************************************/   
/*   
IF	@Function = 'GetPartPriceList' AND   
	@ProfileItemCode IS NOT NULL AND   
	@LoginsID IS NOT NULL AND   
	@VehicleID IS NOT NULL   
BEGIN   
	IF (	SELECT VehicleInfo.VehicleID    
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId= EstimationData.ID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID	) = 35826   
	BEGIN   
		EXECUTE App_Functions_Honda   
			@Function = @Function,   
			@ProfileItemCode = @ProfileItemCode,   
			@VehicleID = @VehicleID,   
			@LoginsID = @LoginsID   
	END   
END*/   
   
IF @Function = 'GetVehicleStepParts' AND   
	@VehicleID IS NOT NULL AND   
	@StepsID IS NOT NULL AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	EXECUTE GetVehicleStepParts @VehicleID, @StepsID, @AdminInfoID   
END   
   
IF @Function = 'AddUpdateEstimateParts' AND   
	@AdminInfoID IS NOT NULL AND   
	@InfoString IS NOT NULL   
BEGIN   
	EXECUTE ProcessEstimateItemString @AdminInfoID, @InfoString   
END   
   
IF @Function = 'GetVehicleStatus'    
BEGIN   
	SELECT VehicleSteps.VehicleID,   
		FocusWrite.dbo.GetVehicleName(VehicleSteps.VehicleID) 'Vehicle Name',   
		RegionText.TextValue 'Region',   
		ManufacturerText.TextValue 'Manufacturer',   
		MakeText.TextValue 'Make',   
		ModelText.TextValue 'Model',   
		ModelYear.ModelYear 'Year',   
		SubModelText.TextValue 'Submodel',   
		Count(*) 'ItemCount'	   
	FROM Vehicles.Dbo.VehicleSteps VehicleSteps    
	INNER JOIN Vehicles.Dbo.StepList StepList  ON   
		(StepList.VehicleStepsID =  VehicleSteps.id)   
	INNER JOIN Vehicles.Dbo.StepListToElements StepListToElements  ON    
		(StepListToElements.StepListID = StepList.id)   
	INNER JOIN Vehicles.dbo.Elements VehElements  ON   
		(VehElements.id = StepListToElements.ElementsID)   
   
	INNER JOIN Vehicles.dbo.SubModel SubModel  ON   
		(SubModel.ID = VehicleSteps.VehicleID)   
	INNER JOIN Vehicles.dbo.TextValues SubModelText  ON   
		(SubModelText.id = SubModel.TextValuesID)   
   
	INNER JOIN Vehicles.dbo.ModelYear ModelYear  ON   
		(ModelYear.ID = SubModel.ModelYearID)   
   
	INNER JOIN Vehicles.dbo.Model Model  ON   
		(Model.ID = ModelYear.ModelID)   
	INNER JOIN Vehicles.dbo.TextValues ModelText  ON   
		(ModelText.id = Model.TextValuesID)   
   
	INNER JOIN Vehicles.dbo.Make Make  ON   
		(Make.ID = Model.MakeID)   
	INNER JOIN Vehicles.dbo.TextValues MakeText  ON   
		(MakeText.id = Make.TextValuesID)   
   
	INNER JOIN Vehicles.dbo.Manufacturer Manufacturer  ON   
		(Manufacturer.id = Make.ManufacturerID)   
	INNER JOIN Vehicles.dbo.TextValues ManufacturerText  ON   
		(ManufacturerText.id = Manufacturer.TextValuesID)   
   
	INNER JOIN Vehicles.dbo.Region Region  ON   
		(Region.id = Manufacturer.Regionid)   
   
   
	INNER JOIN Vehicles.dbo.TextValues RegionText  ON   
		(RegionText.id = Region.TextValuesID)   
   
	GROUP BY VehicleSteps.VehicleID,   
		FocusWrite.dbo.GetVehicleName(VehicleSteps.VehicleID),   
		RegionText.TextValue,   
		ManufacturerText.TextValue,   
		MakeText.TextValue,   
		ModelText.TextValue,   
		ModelYear.ModelYear,   
		SubModelText.TextValue    
	ORDER BY    
		   
		MakeText.TextValue,   
		ModelText.TextValue,   
		SubmodelText.TextValue,   
		ModelYear.ModelYear,   
		FocusWrite.dbo.GetVehicleName(VehicleSteps.VehicleID)   
END   
   
   
IF @Function = 'GetEstimateNotes' AND   
	@EstimationLineItemsID IS NOT NULL   
BEGIN   
	SELECT EstimationNotesInternal.Notes 'Internal',   
		EstimationNotesExternal.Notes 'External'   
	FROM EstimationLineItems    
	LEFT JOIN EstimationNotes EstimationNotesInternal  ON   
		(EstimationNotesInternal.EstimationLineItemsID = EstimationLineItems.ID AND   
		 EstimationNotesInternal.Printed = 0)   
	LEFT JOIN EstimationNotes EstimationNotesExternal  ON   
		(EstimationNotesExternal.EstimationLineItemsID = EstimationLineItems.ID AND   
		 EstimationNotesExternal.Printed = 1)   
	WHERE EstimationLineItems.ID = @EstimationLineItemsID   
END   
   
IF @Function = 'SaveEstimateNotes' AND   
	@EstimationLineItemsID IS NOT NULL   
BEGIN   
	IF ISNULL(LTRIM(RTRIM(@InternalNotes)),'') = ''   
		DELETE FROM EstimationNotes    
		FROM EstimationNotes    
		WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
			Printed = 0   
	ELSE   
	BEGIN   
		DELETE FROM EstimationNotes    
		FROM EstimationNotes    
		WHERE  EstimationLineItemsID = @EstimationLineItemsID AND   
			Printed = 0   
   
		INSERT INTO EstimationNotes  (EstimationLineItemsID,Printed,Notes)   
				VALUES	(@EstimationLineItemsID,0,@InternalNotes)   
	END   
   
	IF ISNULL(LTRIM(RTRIM(@ExternalNotes)),'') = ''   
		DELETE FROM EstimationNotes    
		FROM EstimationNotes    
		WHERE EstimationLineItemsID = @EstimationLineItemsID AND   
			Printed = 1   
	ELSE   
	BEGIN   
		DELETE FROM EstimationNotes    
		FROM EstimationNotes    
		WHERE  EstimationLineItemsID = @EstimationLineItemsID AND   
			Printed = 1   
   
		INSERT INTO EstimationNotes  (EstimationLineItemsID,Printed,Notes)   
				VALUES	(@EstimationLineItemsID,1,@ExternalNotes)   
	   
	END   
END   
   
IF @Function = 'GetRevisionHistory'   
BEGIN   
	SELECT 	ChangesHeader.Type,   
		ChangesHeader.UpdateDate,   
		ChangesData.ChangesInfo   
	FROM ChangesHeader    
	INNER JOIN ChangesData  ON    
		(ChangesData.ChangesHeaderID = ChangesHeader.ID)   
	WHERE ChangesHeader.Type = 'F'   
	ORDER BY ChangesHeader.UpdateDate,   
		ChangesData.ChangesInfo   
   
	SELECT 	ChangesHeader.Type,   
		ChangesHeader.UpdateDate,   
		ChangesHeader.VersionNumber,   
		ChangesData.ChangesInfo   
	FROM ChangesHeader    
	INNER JOIN ChangesData  ON    
		(ChangesData.ChangesHeaderID = ChangesHeader.ID)   
	WHERE ChangesHeader.Type = 'P' AND   
		(@ViewAll <> 0 OR   
		 (@ViewAll = 0 AND DateDiff(d,REPLACE(ChangesHeader.UpdateDate,'ET',''),GetDate()) < 60) )   
	ORDER BY RIGHT('00000'+FocusWrite.dbo.Extract(VersionNumber,0,'.'),5)+'.'+   
		RIGHT('00000'+FocusWrite.dbo.Extract(VersionNumber,1,'.'),5)+'.'+   
		RIGHT('00000'+FocusWrite.dbo.Extract(VersionNumber,2,'.'),5)+'.'+   
		RIGHT('00000'+FocusWrite.dbo.Extract(VersionNumber,3,'.'),5) DESC,   
		ChangesData.ChangesInfo   
END	   
   
IF @Function = 'SaveVinDecodeInfo'   
BEGIN   
	INSERT INTO VinDecodeHistory 	(Vin, VinDecode, LoginsID)   
		VALUES			(@Vin, @VinDecode, @LoginsID)   
END   
   
IF @Function = 'SaveBodyType' AND   
	@AdminInfoID IS NOT NULL AND   
	@BodyType IS NOT NULL   
BEGIN   
	IF @AdminInfoID > 1583497   
	BEGIN   
		exec UpdateVehicleInfo @AdminInfoID = @AdminInfoID,@BodyId = @BodyType   
	END   
	ELSE   
	BEGIN   
		UPDATE VehicleInfo    
		SET BodyType = @BodyType    
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId = EstimationData.id)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
	   
END   
   
IF @Function = 'SaveDriveType' AND   
	@AdminInfoID IS NOT NULL AND   
	@DriveType IS NOT NULL   
BEGIN   
	IF @AdminInfoID > 1583497   
	BEGIN print @DriveType   
		exec UpdateVehicleInfo @AdminInfoID = @AdminInfoID,@DriveID = @DriveType   
	END   
	ELSE   
	BEGIN   
		UPDATE VehicleInfo    
		SET DriveType = @DriveType    
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId = EstimationData.id)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END   
END   
   
IF @Function = 'SaveEngineType' AND   
	@AdminInfoID IS NOT NULL AND   
	@EngineType IS NOT NULL   
BEGIN   
	IF @AdminInfoID > 1583497   
	BEGIN   
		exec UpdateVehicleInfo @AdminInfoID = @AdminInfoID,@EngineID = @EngineType   
	END   
	ELSE   
	BEGIN   
		UPDATE VehicleInfo    
		SET EngineType = @EngineType    
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId = EstimationData.id)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END	   
END   
IF @Function = 'SaveTransmissionType' AND   
	@AdminInfoID IS NOT NULL AND   
	@TransmissionType IS NOT NULL   
BEGIN   
	IF @AdminInfoID > 1583497   
	BEGIN   
		exec UpdateVehicleInfo @AdminInfoID = @AdminInfoID,@TransmissionID = @TransmissionType   
	END   
	ELSE   
	BEGIN   
		UPDATE VehicleInfo    
		SET TransmissionType = @TransmissionType   
		FROM EstimationData    
		INNER JOIN VehicleInfo  ON   
			(VehicleInfo.EstimationDataId = EstimationData.id)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
	END	   
	   
END   
   
--IF @Function = 'GetAnnouncements'   
--BEGIN   
--	SELECT @TempDate = GetDate()   
--	SELECT *   
--	FROM Announcements    
--	LEFT JOIN Logins  ON   
--		(Logins.id = @LoginsID)   
--	WHERE Announcements.Language_ID = @Language_ID   
--		AND @TempDate >= ISNULL(StartDate, @TempDate) AND   
--		@TempDate <= ISNULL(EndDate, @TempDate) AND   
--		(ISNULL(Logins.id,-1) = ISNULL(Announcements.LoginID,-1) OR Announcements.LoginID IS NULL) AND   
--		(ISNULL(Logins.OrganizationID,-1) = ISNULL(Announcements.OrganizationID,-1) OR Announcements.OrganizationID IS NULL)   
--END   
   
IF @Function = 'GetVehiclesAdded'   
BEGIN   
	SELECT @StrSql =    
		'SELECT id, VehicleGroup ' +    
		'FROM Mitchell.Dbo.VehiclesAdded  ' +   
		CASE 	WHEN @FilterYear IS NOT NULL OR @FilterMake IS NOT NULL OR @FilterModel IS NOT NULL THEN 'WHERE ' +   
				CASE 	WHEN @FilterYear IS NOT NULL    
						THEN '( @FilterYear BETWEEN YearStart AND YearEnd) '   
					ELSE ''   
				END +   
   
				CASE	WHEN @FilterMake IS NOT NULL THEN   
						CASE	WHEN @FilterYear IS NOT NULL THEN ' AND '    
							ELSE ''   
						END +   
						'(Make LIKE ''%'' + @FilterMake + ''%'') '   
					ELSE ''   
				END +   
   
				CASE	WHEN @FilterModel IS NOT NULL THEN   
						CASE	WHEN @FilterYear IS NOT NULL OR @FilterMake IS NOT NULL THEN ' AND '    
							ELSE ''    
						END +   
						'(Model LIKE ''%'' + @FilterModel + ''%'') '   
					ELSE ''   
				END    
			ELSE ''   
		END   
	   
	IF @SortField = 'Year'   
		SELECT @StrSql = @StrSql + 'ORDER BY YearStart, Make, Model'   
	ELSE IF @SortField = 'Make'   
		SELECT @StrSql = @StrSql + 'ORDER BY Make, Model, YearStart'   
	ELSE IF @SortField = 'Model'   
		SELECT @StrSql = @StrSql + 'ORDER BY  Model, YearStart, Make'   
   
	SELECT @SortSQL =    
		CASE	WHEN @FilterYear IS NOT NULL THEN '@FilterYear Int'    
			ELSE ''   
		END +   
   
		CASE 	WHEN @FilterMake IS NOT NULL THEN    
			CASE 	WHEN @FilterYear IS NOT NULL THEN ', '    
				ELSE ''    
			END +    
			'@FilterMake Int'    
		ELSE ''    
		END +   
   
		CASE WHEN @FilterModel IS NOT NULL THEN    
			CASE 	WHEN @FilterYear IS NOT NULL OR @FilterMake IS NOT NULL THEN ', '    
				ELSE ''    
			END +    
			'@FilterModel Int'   
			ELSE ''     
		END   
   
	SELECT @nStrSql = CONVERT(nVarChar(4000), @StrSql)   
	EXECUTE sp_executesql @nStrSql,   
		N'@FilterYear Int, @FilterMake VarChar(50), @FilterModel VarChar(50)',    
		@FilterYear, @FilterMake, @FilterModel   
END   
   
IF @Function = 'GetSectionList'   
BEGIN   
	/*IF @VehicleID = 35826   
		SELECT CONVERT(Int,-1) 'SubsectionID', CONVERT(VarChar(500),'') 'SectionName'   
		UNION ALL   
		SELECT CONVERT(Int, NULL) 'SubsectionID',   
			CONVERT(VarChar(255),NULL) 'SectionName'   
		WHERE 1=0   
	ELSE  */   
		EXECUTE Mitchell3.dbo.GetSectionsList2   
			@VehicleID  = @VehicleID,   
			@BodyTypesID = @BodyType,   
			@AdminInfoID = @AdminInfoID,   
			@AddBlank = 1   
   
END   
   
IF @Function = 'GetVehicleInfoText' AND   
	@VehicleID IS NOT NULL   
BEGIN	   
	SELECT VehicleList.FullModelText   
	FROM Vehicles.dbo.VehicleList VehicleList    
	WHERE VehicleList.VehicleID = @VehicleID   
END   
   
IF @Function = 'GetAccessories' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT Accessories.*,   
		CASE WHEN VehicleAccessories.ID is NULL THEN 0 ELSE 1 END 'Selected'   
	FROM Accessories    
	LEFT JOIN EstimationData  ON    
		(EstimationData.AdminInfoID = @AdminInfoID)   
	LEFT JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataID = EstimationData.ID)   
	LEFT JOIN VehicleAccessories  ON   
		(VehicleAccessories.VehicleInfoID = VehicleInfo.ID AND   
		 VehicleAccessories.AccessoriesID = Accessories.ID)    
END   
   
IF @Function = 'SaveAccessories' AND   
	@UpdateString NOT LIKE '%''%' AND   
	LEN(LTRIM(@UpdateString))>0 AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT @UpdateString = SUBSTRING(@UpdateString,1,LEN(@UpdateString)-1)   
	SELECT @StrSql = '   
	DELETE FROM VehicleAccessories    
	FROM EstimationData    
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataID = EstimationData.ID)   
	INNER JOIN VehicleAccessories  ON    
		(VehicleAccessories.VehicleInfoID = VehicleInfo.ID)   
	WHERE VehicleAccessories.AccessoriesID NOT IN ('+ @UpdateString + ') AND   
		EstimationData.AdminInfoID = ' + CONVERT(VarChar(12),@AdminInfoID)   
   
	EXECUTE (@StrSql)   
   
	SELECT @StrSql = '   
	INSERT INTO VehicleAccessories  (VehicleInfoID, AccessoriesID)   
	SELECT VehicleInfo.ID, Accessories.ID   
	FROM Accessories 	   
	INNER JOIN EstimationData  ON   
		(EstimationData.AdminInfoID = ' + CONVERT(VarChar(12),@AdminInfoID) + ')   
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataID = EstimationData.ID)   
	LEFT JOIN VehicleAccessories  ON    
		(VehicleAccessories.AccessoriesID = Accessories.ID AND VehicleInfo.ID = VehicleAccessories.VehicleInfoID)   
	WHERE 	Accessories.ID IN ('+ @UpdateString + ') AND   
		EstimationData.AdminInfoID = ' + CONVERT(VarChar(12),@AdminInfoID) +' AND   
		VehicleAccessories.ID IS NULL'    
   
	EXECUTE (@StrSql)   
END   
   
IF @Function = 'AcceptAllOverlaps' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	EXECUTE AddCheckedAutoIncludes   
		@AdminInfoID = @AdminInfoID,    
		@IDsChecked = '',   
		@AddAll = 1   
   
	INSERT INTO EstimationOverlap  (	EstimationLineItemsID1,EstimationLineItemsID2,OverlapAdjacentFlag,Amount,Minimum,UserOverride,UserAccepted,UserResponded)   
	SELECT 	ActionID,   
		ISNULL(EOP.ResultID,EstimationLineItems.id),   
		CASE WHEN EOP.ActionCode = 9 THEN 'S' ELSE 'O' END,   
		ISNULL(OverlapAmount,0),MinimumLabor,0,1,1   
	FROM EstimationOverlapProposed EOP    
	INNER JOIN EstimationData  ON   
		(EstimationData.AdminInfoID = EOP.AdminInfoID)   
	LEFT JOIN EstimationLineItems  ON   
		(EstimationLineItems.EstimationDataID = EstimationData.ID AND   
		 EstimationLineItems.Barcode = EOP.BarcodeInc)   
	LEFT JOIN EstimationOverlap EO  ON   
		(EO.EstimationLineItemsID1 = ActionID AND   
		 EO.EstimationLineItemsID2 = ISNULL(EOP.ResultID,EstimationLineItems.id) )   
	WHERE EOP.AdminInfoID = @AdminInfoID AND   
		Reason <> 'Added Because of'  AND   
		EOP.ActionID IS NOT NULL AND   
		EO.id IS NULL AND   
		ISNULL(EOP.ResultID,EstimationLineItems.id) IS NOT NULL   
   
	UPDATE EstimationLineItems   
	SET PartOfOverhaul = 1   
	FROM EstimationLineItems   
	INNER JOIN  (   
		SELECT EstimationLineItems1.ID   
		FROM EstimationOverlap EO    
		INNER JOIN EstimationLineItems EstimationLineItems1  ON   
			(EstimationLineItems1.id = EO.EstimationLineItemsID1)   
		INNER JOIN EstimationLineItems EstimationLineItems2  ON   
			(EstimationLineItems2.id = EO.EstimationLineItemsID2)   
		INNER JOIN EstimationData  ON   
			(EstimationData.id = EstimationLineItems1.EstimationDataID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID AND   
			EstimationLineItems2.ActionCode = 'Over'	) T ON   
		(T.ID = EstimationLineItems.id)   
   
	--Add extras as appropriate for paint type (clear and allowances)   
   
	SELECT @Function = 'DeleteOverlapProposals'   
END   
   
IF @Function = 'AcceptCheckedOverlaps' AND   
	@AdminInfoID IS NOT NULL AND   
	@IDsChecked IS NOT NULL   
BEGIN   
	SELECT 	@SupplementVersion = EstimationData.LockLevel   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @SupplementVersion = ISNULL(@SupplementVersion,0)   
   
	SELECT @IDsChecked = ',' + @IDsChecked + ','   
   
	EXECUTE AddCheckedAutoIncludes   
		@AdminInfoID = @AdminInfoID,    
		@IDsChecked = @IDsChecked,   
		@AddAll = 0   
   
	INSERT INTO EstimationOverlap  (	EstimationLineItemsID1,EstimationLineItemsID2,OverlapAdjacentFlag,Amount,Minimum,UserOverride,UserAccepted,UserResponded,SupplementLevel)   
	SELECT EOP.ActionID,   
		ISNULL(EOP.ResultID,EstimationLineItems.id),   
		CASE WHEN EOP.ActionCode = 9 THEN 'S' ELSE 'O' END,EOP.OverlapAmount,EOP.MinimumLabor,0,   
		CASE WHEN T.ID IS NOT NULL THEN 1 ELSE 0 END,1,   
		@SupplementVersion   
	FROM EstimationOverlapProposed EOP    
	INNER JOIN EstimationData  ON   
		(EstimationData.AdminInfoID = EOP.AdminInfoID)   
	LEFT JOIN EstimationLineItems  ON   
		(EstimationLineItems.EstimationDataID = EstimationData.ID AND   
		 EstimationLineItems.Barcode = EOP.BarcodeInc)   
	LEFT JOIN FocusWrite.dbo.EstimationOverlap EO  ON   
		(EO.EstimationLineItemsID1 = EOP.ActionID AND   
		 EO.EstimationLineItemsID2 = ISNULL(EOP.ResultID,EstimationLineItems.id))   
	LEFT JOIN (	SELECT EOP1.id   
			FROM EstimationOverlapProposed EOP1    
			WHERE CHARINDEX(',' + CONVERT(VarChar(10),EOP1.ID) + ',', @IDsChecked) > 0   
			UNION   
			SELECT EOP2.id   
			FROM EstimationOverlapProposed EOP1    
			INNER JOIN EstimationOverlapProposed EOP2  ON   
				(EOP1.ActionPart = EOP2.ActionPart)   
			WHERE CHARINDEX(',' + CONVERT(VarChar(10),EOP1.ID) + ',', @IDsChecked) > 0	) T ON   
		(T.id = EOP.ID)   
	WHERE EOP.AdminInfoID = @AdminInfoID AND   
		EO.EstimationLineItemsID1 IS NULL AND   
		EOP.Reason <> 'Added Because of'  AND   
		EOP.ActionID IS NOT NULL AND   
		ISNULL(EOP.ResultID,EstimationLineItems.id) IS NOT NULL   
   
	UPDATE EstimationLineItems   
	SET PartOfOverhaul = 1   
	FROM EstimationLineItems   
	INNER JOIN  (   
		SELECT EstimationLineItems1.ID   
		FROM EstimationOverlap EO    
		INNER JOIN EstimationLineItems EstimationLineItems1  ON   
			(EstimationLineItems1.id = EO.EstimationLineItemsID1)   
		INNER JOIN EstimationLineItems EstimationLineItems2  ON   
			(EstimationLineItems2.id = EO.EstimationLineItemsID2)   
		INNER JOIN EstimationData  ON   
			(EstimationData.id = EstimationLineItems1.EstimationDataID)   
		WHERE EstimationData.AdminInfoID = @AdminInfoID AND   
			EstimationLineItems2.ActionCode = 'Over'	) T ON   
		(T.ID = EstimationLineItems.id)   
   
	--Add extras as appropriate for paint type (clear and allowances)   
   
	SELECT @Function = 'DeleteOverlapProposals'   
END   
   
IF @Function = 'RejectOverlapProposals' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT 	@SupplementVersion = EstimationData.LockLevel   
	FROM EstimationData    
	WHERE AdminInfoID = @AdminInfoID   
   
	SELECT @SupplementVersion = ISNULL(@SupplementVersion,0)   
   
	INSERT INTO EstimationOverlap  (	EstimationLineItemsID1,EstimationLineItemsID2,OverlapAdjacentFlag,Amount,Minimum,UserOverride,UserAccepted,UserResponded,SupplementLevel)   
	SELECT DISTINCT EOP.ActionID,   
		ISNULL(EOP.ResultID,EstimationLineItems.id),   
		CASE WHEN EOP.ActionCode = 9 THEN 'S' ELSE 'O' END,EOP.OverlapAmount,EOP.MinimumLabor,0,0,1,   
		@SupplementVersion   
	FROM EstimationOverlapProposed EOP    
	LEFT JOIN FocusWrite.dbo.EstimationOverlap EO  ON   
		(EO.EstimationLineItemsID1 = EOP.ActionID AND   
		 EO.EstimationLineItemsID2 = EOP.ResultID)   
	INNER JOIN EstimationData  ON   
		(EstimationData.AdminInfoID = EOP.AdminInfoID)   
	LEFT JOIN EstimationLineItems  ON   
		(EstimationLineItems.EstimationDataID = EstimationData.ID AND   
		 EstimationLineItems.Barcode = EOP.BarcodeInc)   
	WHERE EOP.AdminInfoID = @AdminInfoID AND   
		EO.EstimationLineItemsID1 IS NULL AND   
		EOP.ActionID IS NOT NULL AND   
		ISNULL(EOP.ResultID,EstimationLineItems.id) IS NOT NULL   
   
	SELECT @Function = 'DeleteOverlapProposals'   
END   
   
IF @Function = 'DeleteOverlapProposals' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	DELETE FROM EstimationOverlapProposed    
	WHERE AdminInfoID = @AdminInfoID   
END   
   
IF @Function = 'NoOverlapPrompting' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	UPDATE CustomerProfilesMisc    
	SET SuppressAddRelatedPrompt = 1   
	FROM CustomerProfilesMisc    
	INNER JOIN AdminInfo  ON   
		(AdminInfo.CustomerProfilesID = CustomerProfilesMisc.CustomerProfilesID)   
	WHERE AdminInfo.ID = @AdminInfoID   
END   
   
IF @Function = 'OverlapProposals' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	SELECT DISTINCT Min(ID) 'ID', ActionPart, Reason, BecauseOf, OverlapAmount, MinimumLabor   
	FROM EstimationOverlapProposed    
	WHERE AdminInfoID = @AdminInfoID   
	GROUP BY ActionPart, Reason, BecauseOf, OverlapAmount, MinimumLabor   
END   
   
SELECT @Return = ISNULL(@Return,-1)   
   
IF @Function = 'GetBarcodes'    
BEGIN   
/*   
	IF ISNULL(@VehicleID,0)>0 AND   
		(	SELECT COUNT(*)   
			FROM Mitchell3.dbo.ServicesToVehicleID 	ServicesToVehicleID   
			INNER JOIN Mitchell3.dbo.Service Service ON   
				(Service.Service_Barcode = ServicesToVehicleID.barcode)   
			WHERE ServicesToVehicleID.VehicleID = @VehicleID) > 0   
	BEGIN   
		SELECT '' 'Barcode',   
			'' 'ModelInfo',   
			'' 'Make',   
			'' 'Model',   
			0 'From_Year',   
			0 'To_Year'   
		UNION ALL   
		SELECT DISTINCT    
			CONVERT(Varchar(10),ServicesToVehicleID.barcode) 'Barcode',    
			CASE 	WHEN Service.To_Year IS NULL 			THEN CONVERT(VarCHar(4),Service.From_Year) + ' '   
				WHEN Service.From_Year = Service.To_Year 	THEN CONVERT(VarCHar(4),Service.From_Year) + ' '   
				ELSE 	ISNULL(CONVERT(VarCHar(4),Service.From_Year) + '-', '') +    
					ISNULL(CONVERT(VarCHar(4),Service.To_Year) + ' ','')   
			END +    
			CASE 	WHEN CHARINDEX(Service.Make,Service.Model) = 0 THEN ISNULL(Service.Make + ' ','') + ISNULL(Service.Model,'')    
				ELSE ISNULL(Service.Model,'')   
			END 'ModelInfo',    
			Service.Make,   
			Service.Model,   
			Service.From_Year,    
			Service.To_Year   
		FROM Mitchell3.dbo.ServicesToVehicleID 	ServicesToVehicleID   
		INNER JOIN Mitchell3.dbo.Service Service ON   
			(Service.Service_Barcode = ServicesToVehicleID.barcode)   
		WHERE ServicesToVehicleID.VehicleID = @VehicleID   
		ORDER BY Make,   
			Model,    
			To_Year,   
			From_Year   
	END   
	ELSE   
	BEGIN   
*/   
	SELECT @VehicleName = FocusWrite.dbo.GetVehicleName(@VehicleID)   
   
	SELECT '' 'Barcode',   
		'' 'ModelInfo',   
		'' 'Make',   
		'' 'Model',   
		0 'From_Year',   
		0 'To_Year',   
		99 'Rank'   
	UNION ALL   
	SELECT DISTINCT    
		CONVERT(Varchar(10),ServicesToVehicleID.barcode) 'Barcode',    
		CASE 	WHEN Service.To_Year IS NULL 			THEN CONVERT(VarCHar(4),Service.From_Year) + ' '   
			WHEN Service.From_Year = Service.To_Year 	THEN CONVERT(VarCHar(4),Service.From_Year) + ' '   
			ELSE 	ISNULL(CONVERT(VarCHar(4),Service.From_Year) + '-', '') +    
				ISNULL(CONVERT(VarCHar(4),Service.To_Year) + ' ','')   
		END +    
		CASE 	WHEN CHARINDEX(Service.Make,Service.Model) = 0 THEN ISNULL(Service.Make + ' ','') + ISNULL(Service.Model,'')    
			ELSE ISNULL(Service.Model,'')   
		END 'ModelInfo',    
		Service.Make,   
		Service.Model,   
		Service.From_Year,    
		Service.To_Year,   
		CASE WHEN CHARINDEX(Service.Make,@VehicleName)>0 THEN 2 ELSE 0 END +   
		CASE WHEN CHARINDEX(Service.Model,@VehicleName)>0 THEN 3 ELSE 0 END +   
		CASE WHEN CONVERT(Int,LEFT(@VehicleName,4)) BETWEEN Service.To_Year AND ISNULL(Service.From_Year,Service.To_Year) THEN 1 ELSE 0 END 'Rank'   
				   
	FROM Mitchell3.dbo.ServicesToVehicleID 	ServicesToVehicleID    
	INNER JOIN Mitchell3.dbo.ServiceTemp Service  ON   
	 	(Service.Barcode = ServicesToVehicleID.barcode)		   
	WHERE 	CASE WHEN CHARINDEX(Service.Make,@VehicleName)>0 THEN 2 ELSE 0 END +   
		CASE WHEN CHARINDEX(Service.Model,@VehicleName)>0 THEN 3 ELSE 0 END +   
		CASE WHEN CONVERT(Int,LEFT(@VehicleName,4)) BETWEEN Service.To_Year AND ISNULL(Service.From_Year,Service.To_Year) THEN 1 ELSE 0 END > 0 OR   
		@VehicleID <= 0   
	ORDER BY    
		Rank DESC,   
		Make,   
		Model,    
		To_Year DESC,   
		From_Year DESC   
--	END   
	SELECT @BarcodeRequired = 0   
END   
   
IF @Function = 'SaveBarcode' AND   
	@AdminInfoID IS NOT NULL   
BEGIN   
	IF @barcode = '' SELECT @barcode = NULL   
   
	UPDATE VehicleInfo   
	SET Service_Barcode = @barcode   
	FROM EstimationData    
	INNER JOIN VehicleInfo ON   
		(VehicleInfo.EstimationDataId = EstimationData.ID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END   
   
IF @Function = 'GetBarcode' AND   
	@AdminInfoID IS NOT NULL   
   
   
BEGIN   
	SELECT @barcode = Service_Barcode   
	FROM EstimationData    
	INNER JOIN VehicleInfo  ON   
		(VehicleInfo.EstimationDataId = EstimationData.ID)   
	WHERE EstimationData.AdminInfoID = @AdminInfoID   
END
GO
