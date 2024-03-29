USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[InsertOrUpdateOrganizationInfo] 
	@ID					int, 
	@OrgInfoContactsID  int = 0, 
	@FederalTaxID		varchar(15) = '', 
	@RegistrationNumber	varchar(25) = '', 
	@CompanyType		varchar(20) = '', 
	@PaidContract		bit = 0, 
	@ExpireDate			datetime, 
	@GraphicsExpireDate	datetime, 
	@ManualExpireDate	datetime, 
	@LoginsID			varchar(20) = '', 
	@Disabled			bit = 0, 
	@ShowRepairShopProfiles		bit, 
	@AllowAlternateIdentities	bit, 
	@AllowRWofInspAssignDates	bit, 
	@Appraiser			bit, 
	@LastEstimateNumber int, 
	@LastReportNumber	int, 
	@LastWorkOrderNumber	int, 
	@DateCreated		datetime, 
	@DelPWRequired		bit, 
	@DelPW				varchar(30) = '', 
	@ShowLaborTimeWO	bit, 
	@Language_ID		int, 
	@CompanyName		varchar(50) = '', 
	@LicenseNumber		varchar(50) = '', 
	@PrintLicenseNumber	bit, 
	@BarNumber			varchar(50) = '', 
	@PrintBarNumber		bit, 
	@PrintFederalTaxID	bit, 
	@CompanyID			varchar(50) = '', 
	@PrintRegistrationNumber	bit, 
	@UseTaxID			bit, 
	@UseDefaultRateProfile	bit, 
	@UseDefaultPDRRateProfile	bit, 
	@LogoImageType		varchar(5) = '', 
	@ProfileLocked		bit, 
	@OverlapAdmin		bit 
AS 
BEGIN 
 
	IF (EXISTS(SELECT * FROM OrganizationInfo WHERE ID = @ID)) 
		BEGIN 
		UPDATE OrganizationInfo 
		   SET  
				OrgInfoContactsID = @OrgInfoContactsID, 
		  		FederalTaxID = @FederalTaxID, 
		  		RegistrationNumber = @RegistrationNumber, 
		  		CompanyType = @CompanyType, 
		  		PaidContract = @PaidContract, 
		  		ExpireDate = @ExpireDate, 
		  		GraphicsExpireDate = @GraphicsExpireDate, 
		  		ManualExpireDate = @ManualExpireDate, 
		  		Disabled = @Disabled, 
		  		ShowRepairShopProfiles = @ShowRepairShopProfiles,  
		  		AllowAlternateIdentities = @AllowAlternateIdentities,  
		  		AllowRWofInspAssignDates = @AllowRWofInspAssignDates, 
		  		Appraiser = @Appraiser,  
		  		LastEstimateNumber = @LastEstimateNumber,  
		  		LastReportNumber = @LastReportNumber,  
		  		LastWorkOrderNumber = @LastWorkOrderNumber, 
		  		DateCreated = @DateCreated,  
		  		DelPWRequired = @DelPWRequired,  
		  		DelPW = @DelPW, 
		  		ShowLaborTimeWO = @ShowLaborTimeWO, 
		  		Language_ID = @Language_ID, 
		  		CompanyName = @CompanyName, 
				LicenseNumber = @LicenseNumber, 
				PrintLicenseNumber = @PrintLicenseNumber, 
				BarNumber = @BarNumber, 
				PrintBarNumber = @PrintBarNumber, 
				PrintFederalTaxID = @PrintFederalTaxID, 
		  		CompanyID = @CompanyID, 
		  		PrintRegistrationNumber = @PrintRegistrationNumber, 
		  		UseTaxID = @UseTaxID, 
				UseDefaultRateProfile = @UseDefaultRateProfile, 
				UseDefaultPDRRateProfile = @UseDefaultPDRRateProfile, 
				LogoImageType = @LogoImageType, 
				ProfileLocked = @ProfileLocked, 
				OverlapAdmin = @OverlapAdmin 
		 WHERE ID = @ID 
 
		 SELECT @ID 
	END 
	ELSE 
	BEGIN 
		INSERT INTO OrganizationInfo  
		( 
			OrgInfoContactsID, 
  			FederalTaxID, 
  			RegistrationNumber, 
  			CompanyType, 
  			PaidContract, 
  			ExpireDate, 
  			GraphicsExpireDate, 
  			ManualExpireDate, 
  			Disabled, 
  			ShowRepairShopProfiles,  
  			AllowAlternateIdentities, 
  			AllowRWofInspAssignDates, 
  			Appraiser,  
  			LastEstimateNumber,  
  			LastReportNumber,  
  			LastWorkOrderNumber, 
  			DateCreated,  
  			DelPWRequired,  
  			DelPW, 
  			ShowLaborTimeWO, 
  			Language_ID, 
  			CustomerNumber, 
  			CompanyName, 
  			LicenseNumber, 
  			PrintLicenseNumber, 
  			BarNumber, 
  			PrintBarNumber, 
  			PrintFederalTaxID, 
  			CompanyID, 
  			PrintRegistrationNumber, 
  			UseTaxID, 
			UseDefaultRateProfile, 
			UseDefaultPDRRateProfile, 
			LogoImageType, 
			ProfileLocked, 
			OverlapAdmin 
		) 
		VALUES 
		( 
			@OrgInfoContactsID, 
	  		@FederalTaxID, 
	  		@RegistrationNumber, 
	  		@CompanyType, 
	  		@PaidContract, 
	  		@ExpireDate, 
	  		@GraphicsExpireDate, 
	  		@ManualExpireDate, 
	  		@Disabled, 
	  		@ShowRepairShopProfiles,  
	  		@AllowAlternateIdentities,  
	  		@AllowRWofInspAssignDates, 
	  		@Appraiser,  
	  		@LastEstimateNumber,  
	  		@LastReportNumber,  
	  		@LastWorkOrderNumber, 
	  		@DateCreated,  
	  		@DelPWRequired,  
	  		@DelPW, 
	  		@ShowLaborTimeWO, 
	  		@Language_ID, 
	  		@LoginsID, 
	  		@CompanyName, 
  			@LicenseNumber, 
  			@PrintLicenseNumber, 
  			@BarNumber, 
  			@PrintBarNumber, 
  			@PrintFederalTaxID, 
  			@CompanyID, 
  			@PrintRegistrationNumber, 
  			@UseTaxID, 
			@UseDefaultRateProfile, 
			@UseDefaultPDRRateProfile, 
			@LogoImageType, 
			@ProfileLocked, 
			@OverlapAdmin 
		) 
 
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	END  
END 
 
GO
