USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EstimateReport_GetHeader] 
	  @AdminInfoID			INT 
	, @ContentDirectory		VARCHAR(100) = ''   
	, @IncludeVehicleOptions	BIT = 0 
	, @HideCustomerData			BIT = 0 
	, @ForceShowInsurance		BIT = 0
	, @HideVendorFBLRInfo		BIT = 1
	, @PrintInspectionDate		BIT = 0
	, @PrintDaysToRepair		BIT = 0
	, @SupplementVersion		INT = null
AS
BEGIN    
   
	 -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.    
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component    
	 IF 1=0 BEGIN    
       SET FMTONLY OFF    
     END    
      
	 DECLARE @VehicleOptions VARCHAR(2000) = ''
	 DECLARE @DaysToRepair Table (col varchar(50) null, col2 varchar(50) null)
	 DECLARE @RepairDays varchar(50)

	 if(isnull(@PrintDaysToRepair,0) = 1)
	 Begin
		SELECT @RepairDays = convert(varchar, RepairDays)
		FROM EstimationData with(nolock) 
		WHERE EstimationData.AdminInfoID = @AdminInfoID
		if(@RepairDays is null And @SupplementVersion is not null)
		Begin
			Insert into @DaysToRepair EXEC GetDaysToRepair @AdminInfoID  = @AdminInfoID, @SupplementVersion = @SupplementVersion
			SELECT @RepairDays = col2 from @DaysToRepair
		End
	End
	
	SELECT @VehicleOptions = @VehicleOptions + Accessory + ', '
	FROM EstimationData with(nolock) 
	LEFT JOIN VehicleInfo  with(nolock) ON VehicleInfo.EstimationDataId = EstimationData.id 
	LEFT JOIN VehicleSelectedOptions  with(nolock) ON VehicleSelectedOptions.VehicleInfoID = VehicleInfo.id 
	LEFT JOIN Accessories  with(nolock) ON Accessories.id = VehicleSelectedOptions.VehicleOptionsID 
	WHERE EstimationData.AdminInfoID = @AdminInfoID 
   
	IF @VehicleOptions <> ''  
		SET @VehicleOptions = SUBSTRING(@VehicleOptions, 0, LEN(@VehicleOptions))  
 
	SELECT TOP 1 
		CASE WHEN ISNULL(ContactOwner.FirstName, '') <> '' OR ISNULL(ContactOwner.LastName, '') <> '' THEN    
			dbo.HtmlEncode(ISNULL(ContactOwner.FirstName, '')) + CASE WHEN ISNULL(ContactOwner.LastName, '') <> '' THEN ' ' + dbo.HtmlEncode(ContactOwner.LastName) ELSE '' END	   
		ELSE '' END   
		+ CASE WHEN ISNULL(ContactOwner.BusinessName, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ContactOwner.BusinessName) ELSE '' END   
		+ CASE WHEN ISNULL(ContactOwner.Phone1, '') <> '' THEN '<br/>' + (CASE WHEN ContactOwner.PhoneNumberType1 <> '' THEN dbo.GetPhoneType(ContactOwner.PhoneNumberType1, '') + ': ' ELSE '' END) + dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone1, '')) + ' ' + ISNULL(ContactOwner.Extension1, '') ELSE '' END    
		+ CASE WHEN ISNULL(ContactOwner.Phone2, '') <> '' THEN '<br/>' + (CASE WHEN ContactOwner.PhoneNumberType2 <> '' THEN dbo.GetPhoneType(ContactOwner.PhoneNumberType2, '') + ': ' ELSE '' END) + dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone2, '')) + ' ' + ISNULL(ContactOwner.Extension2, '') ELSE '' END    
		+ CASE WHEN ISNULL(ContactOwner.Phone3, '') <> '' THEN '<br/>' + (CASE WHEN ContactOwner.PhoneNumberType3 <> '' THEN dbo.GetPhoneType(ContactOwner.PhoneNumberType3, '') + ': ' ELSE '' END) + dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone3, '')) + ' ' + ISNULL(ContactOwner.Extension3, '') ELSE '' END    
		+ CASE WHEN ISNULL(ContactOwner.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ContactOwner.Email) ELSE '' END   
		+ CASE WHEN ISNULL(ContactOwner.SecondaryEmail, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ContactOwner.SecondaryEmail) ELSE '' END   
		+ CASE WHEN ISNULL(OwnerAddress.Address1, '') <> '' THEN '<br/>' + dbo.HtmlEncode(OwnerAddress.Address1) ELSE '' END   
		+ CASE WHEN ISNULL(OwnerAddress.Address2, '') <> '' THEN '<br/>' + dbo.HtmlEncode(OwnerAddress.Address2) ELSE '' END    
		+ CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ISNULL(OwnerAddress.City, '') + CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(OwnerAddress.State, '') + CASE WHEN ISNULL(OwnerAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(OwnerAddress.zip, '')) ELSE '' END    
		AS OwnerInfo   
   
		, dbo.HtmlEncode(ISNULL(ContactClaimRep.FirstName, '') + ' ' + ISNULL(ContactClaimRep.LastName, ''))    
		+ CASE WHEN ISNULL(ContactClaimRep.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactClaimRep.Phone1) + CASE WHEN ISNULL(ContactClaimRep.Extension1, '') <> '' THEN ' x' + ContactClaimRep.Extension1 ELSE '' END ELSE '' END   
		+ CASE WHEN ISNULL(ContactClaimRep.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ContactClaimRep.Email) ELSE '' END   
		+ CASE WHEN ISNULL(ContactClaimRep.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactClaimRep.FaxNumber) ELSE '' END   
		AS ClaimRepInfo   
   
		, dbo.HtmlEncode(ISNULL(ContactInsuranceAgent.FirstName, '') + ' ' + ISNULL(ContactInsuranceAgent.LastName, ''))   
		+ CASE WHEN ISNULL(ContactInsuranceAgent.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsuranceAgent.Phone1) + CASE WHEN ISNULL(ContactInsuranceAgent.Extension1, '') <> '' THEN ' x' + ContactInsuranceAgent.Extension1 ELSE '' END ELSE '' END   
		+ CASE WHEN ISNULL(ContactInsuranceAgent.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ContactInsuranceAgent.Email) ELSE '' END   
		+ CASE WHEN ISNULL(ContactInsuranceAgent.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactInsuranceAgent.FaxNumber) ELSE '' END   
		AS InsuranceAgentInfo   
   
		, dbo.HtmlEncode(ISNULL(ContactInsuranceAdjuster.FirstName + ' ' + ContactInsuranceAdjuster.LastName, ' '))     
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsuranceAdjuster.Phone1) + CASE WHEN ISNULL(ContactInsuranceAdjuster.Extension1, '') <> '' THEN ' x' + ContactInsuranceAdjuster.Extension1 ELSE '' END ELSE '' END   
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ISNULL(ContactInsuranceAdjuster.Email, '')) ELSE '' END   
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactInsuranceAdjuster.FaxNumber) ELSE '' END   
		AS InsuranceAdjusterInfo   
   
		, CASE WHEN ISNULL(EstimationData.InsuranceCompanyName, '') <> '' THEN dbo.HtmlEncode(ISNULL(EstimationData.InsuranceCompanyName, '')) + '<br/>' ELSE '' END      
		+ CASE WHEN ISNULL(EstimationData.PolicyNumber, '') <> '' THEN 'Policy #: ' + dbo.HtmlEncode(EstimationData.PolicyNumber) + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(EstimationData.ClaimNumber, '') <> '' THEN 'Claim #: ' + dbo.HtmlEncode(EstimationData.ClaimNumber) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(EstimationData.AssignmentDate, '') <> '' THEN 'Assignment Date: ' + CONVERT(varchar,AssignmentDate,101) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(EstimationData.DateOfLoss, '') <> '' THEN 'Date Of Loss: ' + CONVERT(varchar,DateOfLoss,101) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(EstimationData.Deductible, 0) <> 0 THEN 'Deductible: $' + CAST(ISNULL(EstimationData.Deductible, 0) AS VARCHAR)  + '<br/>' ELSE '' END   
		+ CASE WHEN EstimationData.CoverageType IS NULL OR EstimationData.CoverageType = 255 THEN ''   
		ELSE   
			+ 'Coverage Type: ' +   
			CASE WHEN EstimationData.CoverageType = 0 THEN 'Comprehensive'   
				 WHEN EstimationData.CoverageType = 1 THEN 'Collision'   
				 WHEN EstimationData.CoverageType = 2 THEN 'Third Party'   
			END   + '<br/>' 
		END   
		+ CASE WHEN @PrintInspectionDate = 1 THEN CASE WHEN ISNULL(EstimationData.EstimationDate, '') <> '' THEN 'Inspection Date: ' + CONVERT(varchar,EstimationDate,101) + '<br/>' ELSE '' END ELSE '' END
		+ CASE WHEN ISNULL(@RepairDays, '') <> '' THEN 'Repair Days: ' + @RepairDays + '<br/>' ELSE '' END
		AS InsuranceCompanyInfo    
   
		, dbo.HtmlEncode(ISNULL(Vendor.CompanyName, ''))   
		+ CASE WHEN ISNULL(Vendor.FirstName, '') + ISNULL(Vendor.LastName, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ISNULL(Vendor.FirstName, '') + CASE WHEN ISNULL(Vendor.LastName, '') <> '' THEN ' ' + ISNULL(Vendor.LastName, '') ELSE '' END) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.MobileNumber, '') <> '' THEN '<br/>Mobile: ' + dbo.FormatPhoneNumber(Vendor.MobileNumber) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.WorkNumber, '') <> '' THEN '<br/>Business: ' + dbo.FormatPhoneNumber(Vendor.WorkNumber) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(Vendor.FaxNumber) ELSE '' END  
		+ CASE WHEN ISNULL(Vendor.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(Vendor.Email) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.Address1, '') <> '' THEN '<br/>' + dbo.HtmlEncode(Vendor.Address1) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.Address2, '') <> '' THEN '<br/>' + dbo.HtmlEncode(Vendor.Address2) ELSE '' END   
		+ CASE WHEN ISNULL(Vendor.City, '') <> '' THEN '<br/>' + ISNULL(Vendor.City, '') + CASE WHEN ISNULL(Vendor.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(Vendor.State, '') + CASE WHEN ISNULL(Vendor.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(Vendor.zip, '') ELSE '' END    
		
		+ CASE WHEN @HideVendorFBLRInfo = 0 THEN (CASE WHEN ISNULL(Vendor.FederalTaxID, '') <> '' THEN '<br/>Federal Tax ID: ' + dbo.HtmlEncode(Vendor.FederalTaxID) ELSE '' END) END
		+ CASE WHEN ISNULL(Vendor.LicenseNumber, '') <> '' THEN '<br/>License Number: ' + dbo.HtmlEncode(Vendor.LicenseNumber) ELSE '' END
		+ CASE WHEN ISNULL(Vendor.BarNumber, '') <> '' THEN '<br/>Bar Number: ' + dbo.HtmlEncode(Vendor.BarNumber) ELSE '' END
		+ CASE WHEN ISNULL(Vendor.RegistrationNumber, '') <> '' THEN '<br/>Registration Number: ' + dbo.HtmlEncode(Vendor.RegistrationNumber) ELSE '' END 
		+ CASE WHEN ISNULL(EstimationData.FacilityRepairOrder, '') <> '' THEN '<br/>R/O #: ' + dbo.HtmlEncode(EstimationData.FacilityRepairOrder) ELSE '' END
		+ CASE WHEN ISNULL(EstimationData.FacilityRepairInvoice, '') <> '' THEN '<br/>Invoice #: ' + dbo.HtmlEncode(EstimationData.FacilityRepairInvoice) ELSE '' END
		AS RepairFacilityInfo
   
		, CASE WHEN ISNULL(VehicleInfoManual.ManualSelection, 0) = 1 THEN    
			dbo.HtmlEncode(   
				ISNULL(VehicleInfoManual.ModelYear,'') + ' ' +     
				ISNULL(VehicleInfoManual.Make,'') + ' ' +    
				ISNULL(VehicleInfoManual.Model,'') + ' ' +     
				ISNULL(VehicleInfoManual.SubModel,'')    
			)   
		ELSE    
			dbo.HtmlEncode(FocusWrite.dbo.GetVehicleName(VehicleInfo.VehicleID))   
		END      
		+ CASE WHEN ISNULL(VehicleInfo.Vin, '') <> '' THEN '<br/>' + VehicleInfo.Vin ELSE '' END   
		+ CASE WHEN ISNULL(VehicleInfo.IntColor, '') <> '' THEN '<br/>Int. Color: ' + dbo.HtmlEncode(VehicleInfo.IntColor) ELSE '' END   
		+ CASE WHEN ISNULL(VehicleInfo.ExtColor, '') <> '' THEN '<br/>' + CASE WHEN ISNULL(VehicleInfo.ExtColor2, '') <> '' THEN 'Primary ' ELSE '' END + 'Ext. Color: ' + dbo.HtmlEncode(VehicleInfo.ExtColor) ELSE '' END
		+ CASE WHEN ISNULL(VehicleInfo.ExtColor2, '') <> '' THEN '<br/>' + CASE WHEN ISNULL(VehicleInfo.ExtColor, '') <> '' THEN 'Second ' ELSE '' END + 'Ext. Color: ' + dbo.HtmlEncode(VehicleInfo.ExtColor2) ELSE '' END
		+ CASE WHEN ISNULL(VehicleInfo.License, '') <> '' THEN '<br/>License: ' + dbo.HtmlEncode(UPPER(VehicleInfo.License))  + ' ' + UPPER(ISNULL(VehicleInfo.State, '')) ELSE '' END   
		+ CASE WHEN ISNULL(VehicleInfo.MilesIn, 0) > 0 OR ISNULL(VehicleInfo.MilesOut, 0) > 0 THEN   
			'<br/>Mileage In/Out: ' + CAST(ISNULL(VehicleInfo.MilesIn, 0) AS VARCHAR) + '/' + CAST(ISNULL(VehicleInfo.MilesOut, 0) AS VARCHAR)   
		ELSE '' END    
		+ CASE WHEN ISNULL(Bodys.Body, '') <> '' THEN '<br/>Body Type: ' + Bodys.Body ELSE '' END   
		+ CASE WHEN ISNULL(Engines.Engine, '') <> '' THEN '<br/>Engine: ' + Engines.Engine ELSE '' END   
		+ CASE WHEN ISNULL(Transmissions.Transmission, '') <> '' THEN '<br/>Transmission: ' + Transmissions.Transmission ELSE '' END   
		+ CASE WHEN ISNULL(Drives.Drive, '') <> '' THEN '<br/>Drive Type: ' + Drives.Drive ELSE '' END   
		+ CASE WHEN ISNULL(VehicleInfo.ProductionDate, '') <> '' THEN '<br/>Production Date: ' + CONVERT(varchar, VehicleInfo.ProductionDate, 101) ELSE '' END   
		+ CASE WHEN ISNULL(VehicleInfo.StockNumber, '') <> '' THEN '<br/>Stock #: ' + dbo.HtmlEncode(VehicleInfo.StockNumber) ELSE '' END 
		+ CASE WHEN TRIM(ISNULL(POILabel1.Name, '') + ' ' + ISNULL(POIOptions1.Name, '') + ' ' + ISNULL(VehicleInfo.POICustom1, '')) <> '' 
				-- THEN '<br/>First POI: ' + TRIM(ISNULL(POILabel1.Name, '') + ' ' + ISNULL(POIOptions1.Name, '') + ' ' + ISNULL(VehicleInfo.POICustom1, '')) 
				THEN '<br/>First POI: ' + dbo.FormatPOI(POILabel1.Name, POIOptions1.Name, dbo.HtmlEncode(ISNULL(VehicleInfo.POICustom1,'')))  
				ELSE '' 
			  END 
			  + 
			  CASE WHEN TRIM(ISNULL(POILabel2.Name, '') + ' ' + ISNULL(POIOptions2.Name, '') + ' ' + ISNULL(VehicleInfo.POICustom2, '')) <> '' 
				THEN '<br/>Second POI: ' + dbo.FormatPOI(POILabel2.Name, POIOptions2.Name, dbo.HtmlEncode(ISNULL(VehicleInfo.POICustom2,''))) 
				ELSE '' 
			  END 
		+ CASE WHEN @IncludeVehicleOptions = 1 AND @VehicleOptions <> '' THEN '<br/><br/><b>Accessories</b>: ' + @VehicleOptions ELSE '' END 
		AS VehicleInfo
   
		, CASE WHEN ISNULL(AdminInfo.WorkOrderNumber, 0) > 0 THEN 'Repair Order #: ' + CAST(AdminInfo.WorkOrderNumber AS VARCHAR) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(Logins.PrintFederalTaxID, 0) = 1 AND ISNULL(Logins.FederalTaxID, '') <> '' THEN 'Tax ID: ' + CAST(Logins.FederalTaxID AS VARCHAR) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(Logins.PrintRegistrationNumber, 0) = 1 AND ISNULL(Logins.RegistrationNumber, '') <> '' THEN 'Registration #: ' + CAST(Logins.RegistrationNumber AS VARCHAR) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(Logins.PrintLicenseNumber, 0) = 1 AND ISNULL(Logins.LicenseNumber, '') <> '' THEN 'License #: ' + CAST(Logins.LicenseNumber AS VARCHAR) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(Logins.PrintBarNumber, 0) = 1 AND ISNULL(Logins.BarNumber, '') <> '' THEN 'Bar #: ' + CAST(Logins.BarNumber AS VARCHAR) + '<br/>' ELSE '' END   
		+ CASE WHEN ISNULL(EstimationData.PurchaseOrderNumber, '') <> '' THEN 'Purchase Order #: ' + EstimationData.PurchaseOrderNumber + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(EstimatorsData.Phone, '') <> '' THEN 'Estimators Phone: ' + EstimatorsData.Phone + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(EstimatorsData.Email, '') <> '' THEN 'Estimators Email: ' + EstimatorsData.Email ELSE '' END  
		AS OtherInfo, 
		
		CASE WHEN ContactInsured.ContactID IS NOT NULL	
			THEN
				dbo.HtmlEncode(ISNULL(ContactInsured.FirstName + ' ' + ContactInsured.LastName, ' '))     
				+ CASE WHEN ISNULL(ContactInsured.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsured.Phone1) + CASE WHEN ISNULL(ContactInsured.Extension1, '') <> '' THEN ' x' + ContactInsured.Extension1 ELSE '' END ELSE '' END   
				+ CASE WHEN ISNULL(ContactInsured.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ISNULL(ContactInsured.Email, '')) ELSE '' END   
				+ CASE WHEN ISNULL(ContactInsured.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactInsured.FaxNumber) ELSE '' END   
			ELSE
				dbo.HtmlEncode(ISNULL(ContactInsuredSame.FirstName + ' ' + ContactInsuredSame.LastName, ' '))     
				+ CASE WHEN ISNULL(ContactInsuredSame.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsuredSame.Phone1) + CASE WHEN ISNULL(ContactInsuredSame.Extension1, '') <> '' THEN ' x' + ContactInsured.Extension1 ELSE '' END ELSE '' END   
				+ CASE WHEN ISNULL(ContactInsuredSame.Email, '') <> '' THEN '<br/>' + dbo.HtmlEncode(ISNULL(ContactInsuredSame.Email, '')) ELSE '' END   
				+ CASE WHEN ISNULL(ContactInsuredSame.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactInsuredSame.FaxNumber) ELSE '' END   
			END
		AS InsuranceInsuredInfo    
	INTO #Temp   
	FROM AdminInfo    with(nolock)  
	INNER JOIN EstimationData with(nolock)  ON EstimationData.AdminInfoID = AdminInfo.id     
	INNER JOIN Logins with(nolock)  ON AdminInfo.CreatorID = Logins.ID   
    
	LEFT JOIN Vendor AS AltIdentity  with(nolock) ON EstimationData.AlternateIdentitiesID   = AltIdentity.ID 
    
	LEFT JOIN tbl_ContactPerson AS CompanyContact  with(nolock) ON Logins.ContactID    = CompanyContact.ContactID  
	LEFT JOIN tbl_Address AS CompanyAddress  with(nolock) ON Logins.ContactID    =  CompanyAddress.ContactsID 
    
	LEFT JOIN Customer ON AdminInfo.CustomerID = Customer.ID 
	LEFT JOIN tbl_ContactPerson AS ContactOwner  with(nolock) ON ContactOwner.ContactID = Customer.ContactID 
	LEFT JOIN tbl_Address AS OwnerAddress  with(nolock) ON OwnerAddress.AddressID = Customer.AddressID  
	    
	LEFT JOIN tbl_ContactPerson AS ContactInsuranceAgent  with(nolock) ON  AdminInfo.id  =  ContactInsuranceAgent.AdminInfoID AND ContactInsuranceAgent.ContactTypeID = 1 AND ContactInsuranceAgent.ContactSubTypeID = 7    
    
	LEFT JOIN tbl_ContactPerson AS ContactInsuranceAdjuster  with(nolock) ON  AdminInfo.id =  ContactInsuranceAdjuster.AdminInfoID  AND ContactInsuranceAdjuster.ContactTypeID = 1 AND ContactInsuranceAdjuster.ContactSubTypeID = 11    
    
	LEFT JOIN tbl_ContactPerson AS ContactClaimRep  with(nolock) ON AdminInfo.id = ContactClaimRep.AdminInfoID  AND ContactClaimRep.ContactTypeID = 1 AND ContactClaimRep.ContactSubTypeID = 20    

	LEFT JOIN tbl_ContactPerson AS ContactInsured  with(nolock) ON 
		EstimationData.PrintInsured = 1 
		AND EstimationData.InsuredSameAsOwner = 0
		AND AdminInfo.id = ContactInsured.AdminInfoID  
		AND ContactInsured.ContactTypeID = 1 
		AND ContactInsured.ContactSubTypeID = 2 

	LEFT JOIN tbl_ContactPerson AS ContactInsuredSame  with(nolock) ON 
		EstimationData.PrintInsured = 1 
		AND EstimationData.InsuredSameAsOwner = 1
		AND ContactInsuredSame.ContactID = 
				(
					SELECT Customer.ContactID
					FROM AdminInfo
					LEFT OUTER JOIN Customer ON AdminInfo.CustomerID = Customer.ID
					WHERE AdminInfo.ID = @AdminInfoID
				)
    
	LEFT JOIN Vendor  with(nolock) ON EstimationData.RepairFacilityVendorID    =  Vendor.ID 
   
	LEFT JOIN VehicleInfo  with(nolock) ON EstimationData.id  =  VehicleInfo.EstimationDataId  
	LEFT JOIN VehicleInfoManual with(nolock)  ON (VehicleInfo.ID =VehicleInfoManual.VehicleInfoID  )    
	LEFT JOIN Vinn.dbo.Bodys  with(nolock ) ON VehicleInfo.BodyType   =  Bodys.BodyID 
	LEFT JOIN Vinn.dbo.Engines  with(nolock) ON VehicleInfo.EngineType  =   Engines.EngineID 
	LEFT JOIN Vinn.dbo.Transmissions  with(nolock) ON VehicleInfo.TransmissionType  =  Transmissions.TransmissionsID  
	LEFT JOIN Vinn.dbo.Drives  with(nolock) ON VehicleInfo.DriveType  =  Drives.DriveID  
	 
    LEFT JOIN POILabels POILabel1 ON VehicleInfo.POILabel1 = POILabel1.ID 
	LEFT JOIN POIOptions POIOptions1 ON VehicleInfo.POIOption1 = POIOptions1.ID 
	LEFT JOIN POILabels POILabel2 ON VehicleInfo.POILabel2 = POILabel2.ID 
	LEFT JOIN POIOptions POIOptions2 ON VehicleInfo.POIOption2 = POIOptions2.ID 

	LEFT JOIN EstimatorsData  WITH(NOLOCK) ON  EstimationData.EstimatorID  = EstimatorsData.EstimatorID
    
	WHERE AdminInfo.ID = @AdminInfoID     
   
	SELECT *   
	INTO #DetailsSections   
	FROM (   
		-- a, b, c at the beginning of the titles are for sorting... The dispaly report removes the first character and starts with the opening <b> tag   
		SELECT ROW_NUMBER() OVER(ORDER BY Information) AS RowNum, Information   
		FROM   
		(   
			SELECT 'aVehicle Info</b><br/>' + VehicleInfo AS Information FROM #Temp   
   
			UNION   
			SELECT 'bOwner</b><br/>' + OwnerInfo FROM #Temp WHERE OwnerInfo <> '' AND (@HideCustomerData = 0 OR @ForceShowInsurance = 1) 
   
			UNION   
			SELECT 'cInsurance Company</b><br/>' + InsuranceCompanyInfo FROM #Temp WHERE InsuranceCompanyInfo <> '' AND (@HideCustomerData = 0 OR @ForceShowInsurance = 1)
   
			UNION   
			SELECT 'dInsurance Adjuster</b><br/>' + InsuranceAdjusterInfo FROM #Temp WHERE InsuranceAdjusterInfo <> '' AND @HideCustomerData = 0    
   
			UNION    
			SELECT 'eClaim Rep</b><br/>' + ClaimRepInfo FROM #Temp WHERE ClaimRepInfo <> '' AND @HideCustomerData = 0   
   
			UNION    
			SELECT 'fInsurance Agent</b><br/>' + InsuranceAgentInfo FROM #TEMP WHERE InsuranceAgentInfo <> '' AND @HideCustomerData = 0   
 
			UNION    
			SELECT 'gRepair Facility</b><br/>' + RepairFacilityInfo FROM #Temp WHERE RepairFacilityInfo <> '' AND (@HideCustomerData = 0 OR @HideVendorFBLRInfo = 0)   
    
			UNION    
			SELECT 'hShop Info</b><br/>' + OtherInfo FROM #TEMP WHERE OtherInfo <> ''

			UNION    
			SELECT 'iInsured</b><br/>' + InsuranceInsuredInfo FROM #TEMP WHERE InsuranceInsuredInfo <> '' 			
		) Base   
	) PvtBase   
	PIVOT    
	(   
		MAX(Information)   
		FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8], [9])   
	) pvt   
   
	SELECT DISTINCT TOP 1   
		AdminInfo.EstimateNumber    
		, AdminInfo.id AS EstimateID    
		, dbo.HtmlEncode(AdminInfo.Description) AS EstimateDescription   
		, ISNULL(AdminInfo.WorkOrderNumber, 0) AS WorkOrderNumber    
		, (SELECT COUNT(*) FROM PaymentInfo WHERE AdminInfoID = @AdminInfoID) AS PaymentRecords    
		, (SELECT SUM(Amount) AS AmountPaid FROM PaymentInfo WHERE AdminInfoID = @AdminInfoID) As AmountPaid    
		, CAST(AdminInfo.GrandTotal AS MONEY) AS GrandTotal    
    
		, ISNULL(   
		  dbo.HtmlEncode(   
		  CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN AltIdentity.CompanyName    
			ELSE Logins.CompanyName    
		  END    
		  ), '') As CompanyName		     
    
		, CASE WHEN ISNULL(Logins.PrintFederalTaxID, 0) = 1 THEN dbo.HtmlEncode(Logins.FederalTaxID) ELSE '' END AS FederalTaxID    
		, ISNULL(Logins.PrintFederalTaxID, 0) AS PrintFederalTaxID    
		, CASE WHEN ISNULL(Logins.PrintRegistrationNumber, 0) = 1 THEN dbo.HtmlEncode(Logins.RegistrationNumber) ELSE '' END AS RegistrationNumber    
		, ISNULL(Logins.PrintRegistrationNumber, 0) AS PrintRegistrationNumber    
		, CASE WHEN ISNULL(Logins.PrintLicenseNumber, 0) = 1 THEN dbo.HtmlEncode(Logins.LicenseNumber) ELSE '' END AS LicenseNumber    
		, ISNULL(Logins.PrintLicenseNumber, 0) AS PrintLicenseNumber    
		, CASE WHEN ISNULL(Logins.PrintBarNumber, 0) = 1 THEN dbo.HtmlEncode(Logins.BarNumber) ELSE '' END AS BarNumber    
		, ISNULL(Logins.PrintBarNumber, 0) AS PrintBarNumber    
		    
		, dbo.HtmlEncode(ISNULL(ContactClaimRep.FirstName, '') + ' ' + ISNULL(ContactClaimRep.LastName, '')) AS ClaimRepName    
		, dbo.FormatPhoneNumber(ContactClaimRep.Phone1) + CASE WHEN ISNULL(ContactClaimRep.Extension1, '') <> '' THEN ' x' + ContactClaimRep.Extension1 ELSE '' END AS ClaimRepPhone    
		, dbo.HtmlEncode(ContactClaimRep.Email) AS ClaimRepEmail    
		, dbo.FormatPhoneNumber(ContactClaimRep.FaxNumber) AS ClaimRepFaxNumber    
    
		, ISNULL(dbo.HtmlEncode(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN AltIdentity.FirstName + ' ' + AltIdentity.LastName     
			ELSE Logins.HeaderContact 
			--ELSE CompanyContact.FirstName + ' ' + CompanyContact.LastName     
		  END), '') As CompanyContactName    
		, dbo.HtmlEncode(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN IsNull(AltIdentity.WorkNumber, '')    
			ELSE dbo.FormatPhoneNumber(IsNull(CompanyContact.Phone1, '')) + ' ' + ISNULL(CompanyContact.Extension1, '')     
		  END) AS CompanyContactCellPhone    
		, dbo.HtmlEncode(dbo.FormatPhoneNumber(IsNull(CompanyContact.Phone2, '')) + ' ' + ISNULL(CompanyContact.Extension2, '')) AS CompanyContactSecondaryPhone   
		, dbo.HtmlEncode(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
		    THEN dbo.FormatPhoneNumber(IsNull(AltIdentity.FaxNumber, ''))   
			ELSE dbo.FormatPhoneNumber(CompanyContact.FaxNumber)     
		  END) AS CompanyContactFax    
		, dbo.HtmlEncode(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN AltIdentity.Email    
			ELSE CompanyContact.Email     
		  END) AS CompanyContactEmail    
		, ISNULL(SiteSetting.[Value], 'True') AS PrintEmailAddressInHeader 
		, dbo.HtmlEncode(Phone1Type.Report) AS CompanyContactPhoneType1   
		, dbo.HtmlEncode(Phone2Type.Report) AS CompanyContactPhoneType2   
		, CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN dbo.HtmlEncode(AltIdentity.Address1) + CASE WHEN ISNULL(AltIdentity.Address1, '') <> '' AND ISNULL(AltIdentity.Address2, '') <> '' THEN '<br/>' ELSE '' END + dbo.HtmlEncode(AltIdentity.Address2)   
			ELSE dbo.HtmlEncode(CompanyAddress.Address1) + CASE WHEN ISNULL(CompanyAddress.Address1, '') <> '' AND ISNULL(CompanyAddress.Address2, '') <> '' THEN '<br/>' ELSE '' END + dbo.HtmlEncode(CompanyAddress.Address2)   
		  END AS CompanyAddress    
		, dbo.HtmlEncode(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1     
			THEN ISNULL(AltIdentity.City, '') + CASE WHEN ISNULL(AltIdentity.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(AltIdentity.State, '') + CASE WHEN ISNULL(AltIdentity.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(AltIdentity.Zip, '')   
			ELSE ISNULL(CompanyAddress.City, '') + CASE WHEN ISNULL(CompanyAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(CompanyAddress.State, '') + CASE WHEN ISNULL(CompanyAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(CompanyAddress.zip, '')   
		  END) AS CompanyCityStateZip    
		, CASE WHEN ISNULL(Logins.LogoImageType, '') = ''    
			THEN @ContentDirectory + '/CompanyLogos/default.png'    
			ELSE @ContentDirectory + '/CompanyLogos/' + CAST(Logins.ID AS VARCHAR) + '.' + Logins.LogoImageType    
		  END AS CompanyLogoPath    
    
		, dbo.HtmlEncode(ISNULL(ContactOwner.FirstName, '') + ' ' + ISNULL(ContactOwner.LastName, '')) AS [Owner]   
		, dbo.HtmlEncode(ContactOwner.BusinessName) AS OwnerCompanyName    
		, dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone1, '')) + ' ' + ISNULL(ContactOwner.Extension1, '') AS OwnerCell    
		, dbo.HtmlEncode(ContactOwner.Email) AS OwnerEmail    
		, dbo.HtmlEncode(OwnerAddress.Address1) AS OwnerAddress1    
		, dbo.HtmlEncode(OwnerAddress.Address2) AS OwnerAddress2    
		, dbo.HtmlEncode(ISNULL(OwnerAddress.City, '') + CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(OwnerAddress.State, '') + CASE WHEN ISNULL(OwnerAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(OwnerAddress.zip, '')) AS OwnerCityStateZip    
   
		, dbo.HtmlEncode(ISNULL(ContactInsuranceAgent.FirstName, '') + ' ' + ISNULL(ContactInsuranceAgent.LastName, '')) AS InsuranceAgentName    
		, dbo.FormatPhoneNumber(ContactInsuranceAgent.Phone1) + CASE WHEN ISNULL(ContactInsuranceAgent.Extension1, '') <> '' THEN ' x' + ContactInsuranceAgent.Extension1 ELSE '' END AS InsuranceAgentCell    
		, dbo.HtmlEncode(ContactInsuranceAgent.Email) AS InsuranceAgentEmail    
		, dbo.FormatPhoneNumber(ContactInsuranceAgent.FaxNumber) AS InsuranceAgentFax    
    
		, dbo.HtmlEncode(ISNULL(ContactInsuranceAdjuster.FirstName + ' ' + ContactInsuranceAdjuster.LastName, ' ')) AS InsuranceAdjusterName    
		, dbo.FormatPhoneNumber(ContactInsuranceAdjuster.Phone1) + CASE WHEN ISNULL(ContactInsuranceAdjuster.Extension1, '') <> '' THEN ' x' + ContactInsuranceAdjuster.Extension1 ELSE '' END AS InsuranceAdjusterCell    
		, dbo.HtmlEncode(ContactInsuranceAdjuster.Email) AS InsuranceAdjusterEmail    
		, dbo.FormatPhoneNumber(ContactInsuranceAdjuster.FaxNumber) AS InsuranceAdjusterFax    
    
		, CASE WHEN ISNULL(CustomerProfilePrint.PrintEstimator, 0) = 1 THEN dbo.HtmlEncode(ISNULL(EstimatorsData.AuthorFirstName, '') + ' ' + ISNULL(EstimatorsData.AuthorLastName, '')) ELSE '' END As EstimatorsName    
    
		, CASE WHEN ISNULL(VehicleInfoManual.ManualSelection, 0) = 1 THEN    
				ISNULL(VehicleInfoManual.ModelYear,'') + ' ' +     
				ISNULL(VehicleInfoManual.Make,'') + ' ' +    
				ISNULL(VehicleInfoManual.Model,'') + ' ' +     
				ISNULL(VehicleInfoManual.SubModel,'')    
			ELSE    
				FocusWrite.dbo.GetVehicleName(VehicleInfo.VehicleID)    
		END  AS VDesc    
		, VehicleInfo.Vin    
		, dbo.HtmlEncode(VehicleInfo.ExtColor) + CASE WHEN ISNULL(VehicleInfo.ExtColor, '') <> '' THEN '<br/>' ELSE '' END + dbo.HtmlEncode(VehicleInfo.ExtColor2) AS ExtColor   
		, dbo.HtmlEncode(VehicleInfo.IntColor) AS IntColor   
		, dbo.HtmlEncode(VehicleInfo.License) AS License 
		, VehicleInfo.MilesIn    
		, VehicleInfo.MilesOut    
    
		, dbo.HtmlEncode(Vendor.CompanyName) AS RepairFacilityCompanyName    
		, dbo.HtmlEncode(ISNULL(Vendor.FirstName, '') + ' ' + ISNULL(Vendor.LastName, '')) AS RepairFacilityName    
		, dbo.HtmlEncode(Vendor.Address1) AS RepairFacilityAddress    
		, dbo.HtmlEncode(Vendor.Address2) AS RepairFacilityAddress2    
		, ISNULL(Vendor.City, '') + CASE WHEN ISNULL(Vendor.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(Vendor.State, '') + CASE WHEN ISNULL(Vendor.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(Vendor.zip, '') AS RepairFacilityCityStateZip    
   
		, dbo.FormatPhoneNumber(Vendor.MobileNumber) AS RepairFacilityMobileNumber    
		, dbo.FormatPhoneNumber(Vendor.WorkNumber) AS RepairFacilityWorkNumber    
		, ISNULL(Vendor.Email, '') AS RepairFacilityEmail    
    
		, dbo.HtmlEncode(EstimationData.InsuranceCompanyName) AS InsuranceCompanyName    
		, dbo.HtmlEncode(CASE WHEN ISNULL(EstimationData.PrintNote, 0) = 1 THEN ISNULL(EstimationData.Note, '') ELSE '' END) AS EstimateNote    
		, EstimationData.PrintNote    
		, dbo.HtmlEncode(EstimationData.PolicyNumber) AS PolicyNumber    
		, dbo.HtmlEncode(EstimationData.ClaimNumber) AS ClaimNumber    
		, dbo.HtmlEncode(EstimationData.EstimationDate) AS EstimationDate    
		, dbo.HtmlEncode(EstimationData.AssignmentDate) AS AssignmentDate    
		, dbo.HtmlEncode(EstimationData.Deductible) AS Deductible    
		, dbo.HtmlEncode(EstimationData.DateOfLoss) AS DateOfLoss    
		, CAST(CASE WHEN EstimationData.EstimationDate IS NULL AND EstimationData.AssignmentDate IS NULL THEN 0 ELSE 1 END AS BIT) As ShowDates    
    
		, ISNULL(EstimationData.LockLevel, 0) As SupplementLevel    
		, dbo.HtmlEncode(EstimationData.PurchaseOrderNumber) AS PurcahseOrderNumber 
    
	    , REPLACE(REPLACE(ISNULL(CustomerProfilePrint.FooterText, ''), '<o:', '<'), '</o:', '</') as CompanyNotes
		, CustomerProfilePrint.NoHeaderLogo    
		, dbo.HtmlEncode(@VehicleOptions) AS VehicleOptions 
		, dbo.HtmlEncode(ISNULL(VehicleInfo.StockNumber, '')) AS VehicleStockNumber
		, #DetailsSections.*   
	FROM AdminInfo with(nolock)  
	INNER JOIN EstimationData with(nolock) ON EstimationData.AdminInfoID = AdminInfo.id     
	INNER JOIN Logins with(nolock) ON AdminInfo.CreatorID = Logins.ID   
    
	LEFT JOIN Vendor AS AltIdentity  with(nolock) ON EstimationData.AlternateIdentitiesID  = AltIdentity.ID 
    
	LEFT JOIN tbl_ContactPerson AS CompanyContact  with(nolock) ON Logins.ContactID = CompanyContact.ContactID 
	LEFT JOIN tbl_Address AS CompanyAddress  with(nolock) ON Logins.ContactID = CompanyAddress.ContactsID  
	LEFT JOIN PhoneTypes AS Phone1Type  with(nolock) ON CompanyContact.PhoneNumberType1 = Phone1Type.Code 
	LEFT JOIN PhoneTypes AS Phone2Type  with(nolock) ON CompanyContact.PhoneNumberType2 = Phone2Type.Code 
    
	LEFT JOIN Customer ON AdminInfo.CustomerID = Customer.ID 
	LEFT JOIN tbl_ContactPerson AS ContactOwner  with(nolock) ON Customer.ContactID = ContactOwner.ContactID 
	LEFT JOIN tbl_Address AS OwnerAddress  with(nolock) ON Customer.AddressID = OwnerAddress.ContactsID  
	    
	LEFT JOIN tbl_ContactPerson AS ContactInsuranceAgent  with(nolock) ON AdminInfo.id =  ContactInsuranceAgent.AdminInfoID AND ContactInsuranceAgent.ContactTypeID = 1 AND ContactInsuranceAgent.ContactSubTypeID = 7    
    
	LEFT JOIN tbl_ContactPerson AS ContactInsuranceAdjuster  with(nolock) ON  AdminInfo.id =  ContactInsuranceAdjuster.AdminInfoID  AND ContactInsuranceAdjuster.ContactTypeID = 1 AND ContactInsuranceAdjuster.ContactSubTypeID = 11    
    
	LEFT JOIN tbl_ContactPerson AS ContactClaimRep  with(nolock) ON   AdminInfo.id = ContactClaimRep.AdminInfoID  AND ContactClaimRep.ContactTypeID = 1 AND ContactClaimRep.ContactSubTypeID = 20    
    
	LEFT JOIN VehicleInfo  with(nolock) ON EstimationData.id  =   VehicleInfo.EstimationDataId  
	LEFT JOIN VehicleInfoManual  with(nolock) ON (VehicleInfo.ID = VehicleInfoManual.VehicleInfoID ) 	   
    
	LEFT JOIN EstimatorsData  with(nolock) ON  EstimationData.EstimatorID  = EstimatorsData.EstimatorID  
    
	LEFT JOIN Vendor  with(nolock) ON EstimationData.RepairFacilityVendorID   =   Vendor.ID 
    
	LEFT JOIN CustomerProfilePrint  with(nolock) ON AdminInfo.CustomerProfilesID = CustomerProfilePrint.CustomerProfilesID  
	LEFT JOIN SiteSetting  SiteSetting WITH(NOLOCK) ON  SiteSetting.LoginID  = Logins.ID AND TagGroup = 'ReportOptions' AND Tag = 'PrintEmailAddressInHeader'      
    
	LEFT JOIN #DetailsSections ON 1 = 1   
   
	WHERE AdminInfo.ID = @AdminInfoID     
 
 
	  
END    
GO
