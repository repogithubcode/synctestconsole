USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
   
   
CREATE PROCEDURE [dbo].[EstimateReport_GetHeader_BACKUP]   
	@AdminInfoID	int,   
	@ContentDirectory VARCHAR(100),   
	@IsExcludeImage bit=0   
AS   
BEGIN   
  
	 -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.   
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component   
	 IF 1=0 BEGIN   
       SET FMTONLY OFF   
     END   
        
  
	SELECT   
		CASE WHEN ISNULL(ContactOwner.FirstName, '') <> '' OR ISNULL(ContactOwner.LastName, '') <> '' THEN   
			REPLACE(ISNULL(ContactOwner.FirstName, '') + CASE WHEN ISNULL(ContactOwner.LastName, '') <> '' THEN ' ' + ISNULL(ContactOwner.LastName, '') ELSE '' END , '&', '&amp;') 	  
		ELSE '' END  
		+ CASE WHEN ISNULL(ContactOwner.BusinessName, '') <> '' THEN '<br/>' + REPLACE(ContactOwner.BusinessName, '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(ContactOwner.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone1, '')) + ' ' + ISNULL(ContactOwner.Extension1, '') ELSE '' END   
		+ CASE WHEN ISNULL(ContactOwner.Email, '') <> '' THEN '<br/>' + REPLACE(ISNULL(ContactOwner.Email, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(OwnerAddress.Address1, '') <> '' THEN '<br/>' + REPLACE(OwnerAddress.Address1, '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(OwnerAddress.Address2, '') <> '' THEN '<br/>' + REPLACE(OwnerAddress.Address2, '&', '&amp;') ELSE '' END   
		+ CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN '<br/>' + REPLACE(ISNULL(OwnerAddress.City, '') + CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(OwnerAddress.State, '') + CASE WHEN ISNULL(OwnerAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(OwnerAddress.zip, ''), '&', '&amp;') ELSE '' END   
		AS OwnerInfo  
  
		, REPLACE(ISNULL(ContactClaimRep.FirstName, '') + ' ' + ISNULL(ContactClaimRep.LastName, ''), '&', '&amp;')   
		+ CASE WHEN ISNULL(ContactClaimRep.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactClaimRep.Phone1) + CASE WHEN ISNULL(ContactClaimRep.Extension1, '') <> '' THEN ' x' + ContactClaimRep.Extension1 ELSE '' END ELSE '' END  
		+ CASE WHEN ISNULL(ContactClaimRep.Email, '') <> '' THEN '<br/>' + REPLACE(ISNULL(ContactClaimRep.Email, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(ContactClaimRep.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactClaimRep.FaxNumber) ELSE '' END  
		AS ClaimRepInfo  
  
		, REPLACE(ISNULL(ContactInsuranceAgent.FirstName, '') + ' ' + ISNULL(ContactInsuranceAgent.LastName, ''), '&', '&amp;')  
		+ CASE WHEN ISNULL(ContactInsuranceAgent.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsuranceAgent.Phone1) + CASE WHEN ISNULL(ContactInsuranceAgent.Extension1, '') <> '' THEN ' x' + ContactInsuranceAgent.Extension1 ELSE '' END ELSE '' END  
		+ CASE WHEN ISNULL(ContactInsuranceAgent.Email, '') <> '' THEN '<br/>' + REPLACE(ISNULL(ContactInsuranceAgent.Email, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(ContactInsuranceAgent.FaxNumber, '') <> '' THEN '<br />' + dbo.FormatPhoneNumber(ContactInsuranceAgent.FaxNumber) ELSE '' END  
		AS InsuranceAgentInfo  
  
		, REPLACE(ISNULL(ContactInsuranceAdjuster.FirstName + ' ' + ContactInsuranceAdjuster.LastName, ' '), '&', '&amp;')    
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.Phone1, '') <> '' THEN '<br/>Phone: ' + dbo.FormatPhoneNumber(ContactInsuranceAdjuster.Phone1) + CASE WHEN ISNULL(ContactInsuranceAdjuster.Extension1, '') <> '' THEN ' x' + ContactInsuranceAdjuster.Extension1 ELSE '' END ELSE '' END  
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.Email, '') <> '' THEN '<br/>' + REPLACE(ISNULL(ContactInsuranceAdjuster.Email, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(ContactInsuranceAdjuster.FaxNumber, '') <> '' THEN '<br/>Fax: ' + dbo.FormatPhoneNumber(ContactInsuranceAdjuster.FaxNumber) ELSE '' END  
		AS InsuranceAdjusterInfo  
  
		, CASE WHEN ISNULL(EstimationData.InsuranceCompanyName, '') <> '' THEN REPLACE(ISNULL(EstimationData.InsuranceCompanyName, ''), '&', '&amp;') ELSE '' END     
		+ CASE WHEN ISNULL(EstimationData.PolicyNumber, '') <> '' THEN '<br/>Policy #: ' + ISNULL(EstimationData.PolicyNumber, '') ELSE '' END  
		+ CASE WHEN ISNULL(EstimationData.ClaimNumber, '') <> '' THEN '<br/>Claim #: ' + ISNULL(EstimationData.ClaimNumber, '') ELSE '' END  
		+ CASE WHEN ISNULL(EstimationData.AssignmentDate, '') <> '' THEN '<br/>Assignment Date: ' + CONVERT(varchar,AssignmentDate,101) ELSE '' END  
		+ CASE WHEN ISNULL(EstimationData.DateOfLoss, '') <> '' THEN '<br/>Date Of Loss: ' + CONVERT(varchar,DateOfLoss,101) ELSE '' END  
		+ CASE WHEN ISNULL(EstimationData.Deductible, '') <> '' THEN '<br/>Deductible: $' + CAST(ISNULL(EstimationData.Deductible, '') AS VARCHAR) ELSE '' END  
		+ CASE WHEN EstimationData.CoverageType IS NULL OR EstimationData.CoverageType = 255 THEN ''  
		ELSE  
			+ '<br/>Coverage Type: ' +  
			CASE WHEN EstimationData.CoverageType = 0 THEN 'Comprehensive'  
				 WHEN EstimationData.CoverageType = 1 THEN 'Collision'  
				 WHEN EstimationData.CoverageType = 2 THEN 'Third Party'  
			END  
		END  
		+ CASE WHEN ISNULL(EstimationData.EstimationDate, '') <> '' THEN '<br/>Inspection Date: ' + CONVERT(varchar,EstimationDate,101) ELSE '' END  
		+ CASE WHEN ISNULL(POI.Description, '') <> '' THEN '<br/>Point Of Impact: ' + ISNULL(POI.Description, '') ELSE '' END  
		AS InsuranceCompanyInfo  
  
		, REPLACE(ISNULL(Vendor.CompanyName, ''), '&', '&amp;')  
		+ CASE WHEN ISNULL(Vendor.FirstName, '') + ISNULL(Vendor.LastName, '') <> '' THEN '<br/>' + REPLACE(ISNULL(Vendor.FirstName, '') + CASE WHEN ISNULL(Vendor.LastName, '') <> '' THEN ' ' + ISNULL(Vendor.LastName, '') ELSE '' END, '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(Vendor.Address1, '') <> '' THEN '<br/>' + REPLACE(ISNULL(Vendor.Address1, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(Vendor.Address2, '') <> '' THEN '<br/>' + REPLACE(ISNULL(Vendor.Address2, ''), '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(Vendor.City, '') <> '' THEN '<br/>' + ISNULL(Vendor.City, '') + CASE WHEN ISNULL(Vendor.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(Vendor.State, '') + CASE WHEN ISNULL(Vendor.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(Vendor.zip, '') ELSE '' END   
		AS RepairFacilityInfo			  
  
		, CASE WHEN ISNULL(VehicleInfoManual.ManualSelection, 0) = 1 THEN   
			REPLACE(  
				ISNULL(VehicleInfoManual.ModelYear,'') + ' ' +    
				ISNULL(VehicleInfoManual.Make,'') + ' ' +   
				ISNULL(VehicleInfoManual.Model,'') + ' ' +    
				ISNULL(VehicleInfoManual.SubModel,'')   
			, '&', '&amp;')  
		ELSE   
			REPLACE(FocusWrite.dbo.GetVehicleName(VehicleInfo.VehicleID), '&', '&amp;')  
		END     
		+ CASE WHEN ISNULL(VehicleInfo.Vin, '') <> '' THEN '<br/>' + VehicleInfo.Vin ELSE '' END  
		+ CASE WHEN ISNULL(VehicleInfo.IntColor, '') <> '' THEN '<br/>Int. Color: ' + REPLACE(VehicleInfo.IntColor , '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(VehicleInfo.ExtColor, '') <> '' THEN '<br/>Ext. Color: ' + REPLACE(VehicleInfo.ExtColor , '&', '&amp;') ELSE '' END  
		+ CASE WHEN ISNULL(VehicleInfo.License, '') <> '' THEN '<br/>License: ' + UPPER(VehicleInfo.License) + ' ' + UPPER(ISNULL(VehicleInfo.State, '')) ELSE '' END  
		+ CASE WHEN ISNULL(VehicleInfo.MilesIn, 0) > 0 OR ISNULL(VehicleInfo.MilesOut, 0) > 0 THEN  
			'<br/>Mileage In/Out: ' + CAST(ISNULL(VehicleInfo.MilesIn, 0) AS VARCHAR) + '/' + CAST(ISNULL(VehicleInfo.MilesOut, 0) AS VARCHAR)  
		ELSE '' END   
		+ CASE WHEN ISNULL(Bodys.Body, '') <> '' THEN '<br/>Body Type: ' + Bodys.Body ELSE '' END  
		+ CASE WHEN ISNULL(Engines.Engine, '') <> '' THEN '<br/>Engine: ' + Engines.Engine ELSE '' END  
		+ CASE WHEN ISNULL(Transmissions.Transmission, '') <> '' THEN '<br/>Transmission: ' + Transmissions.Transmission ELSE '' END  
		+ CASE WHEN ISNULL(Drives.Drive, '') <> '' THEN '<br/>Drive Type: ' + Drives.Drive ELSE '' END  
		+ CASE WHEN ISNULL(VehicleInfo.ProductionDate, '') <> '' THEN '<br/>Production Date: ' + CONVERT(varchar, VehicleInfo.ProductionDate, 101) ELSE '' END  
		AS VehicleInfo  
  
		, CASE WHEN ISNULL(AdminInfo.WorkOrderNumber, 0) > 0 THEN 'Repair Order #: ' + CAST(AdminInfo.WorkOrderNumber AS VARCHAR) + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(OrganizationInfo.PrintFederalTaxID, 0) = 1 AND ISNULL(OrganizationInfo.FederalTaxID, '') <> '' THEN 'Tax ID: ' + CAST(OrganizationInfo.FederalTaxID AS VARCHAR) + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(OrganizationInfo.PrintRegistrationNumber, 0) = 1 AND ISNULL(OrganizationInfo.RegistrationNumber, '') <> '' THEN 'Registration #: ' + CAST(OrganizationInfo.RegistrationNumber AS VARCHAR) + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(OrganizationInfo.PrintLicenseNumber, 0) = 1 AND ISNULL(OrganizationInfo.LicenseNumber, '') <> '' THEN 'License #: ' + CAST(OrganizationInfo.LicenseNumber AS VARCHAR) + '<br/>' ELSE '' END  
		+ CASE WHEN ISNULL(OrganizationInfo.PrintBarNumber, 0) = 1 AND ISNULL(OrganizationInfo.BarNumber, '') <> '' THEN 'Bar #: ' + CAST(OrganizationInfo.BarNumber AS VARCHAR) ELSE '' END  
		AS OtherInfo  
	INTO #Temp  
	FROM AdminInfo    with(nolock) 
	LEFT OUTER JOIN EstimationData with(nolock)  ON EstimationData.AdminInfoID = AdminInfo.id    
	LEFT OUTER JOIN Logins with(nolock)  ON AdminInfo.CreatorID = Logins.ID  
   
	LEFT OUTER JOIN OrganizationInfo  with(nolock) ON Logins.OrganizationID = OrganizationInfo.ID   
   
	LEFT OUTER JOIN Vendor AS AltIdentity  with(nolock) ON AltIdentity.ID = EstimationData.AlternateIdentitiesID   
   
	LEFT OUTER JOIN tbl_ContactPerson AS CompanyContact  with(nolock) ON CompanyContact.ContactID = OrganizationInfo.OrgInfoContactsID   
	LEFT OUTER JOIN tbl_Address AS CompanyAddress  with(nolock) ON CompanyAddress.ContactsID = OrganizationInfo.OrgInfoContactsID   
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactOwner  with(nolock) ON ContactOwner.AdmininfoID = AdminInfo.id AND ContactOwner.ContactSubTypeID = 4   
	LEFT OUTER JOIN tbl_Address AS OwnerAddress  with(nolock) ON ContactOwner.ContactID = OwnerAddress.ContactsID   
	   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactInsuranceAgent  with(nolock) ON ContactInsuranceAgent.AdminInfoID = AdminInfo.id AND ContactInsuranceAgent.ContactTypeID = 1 AND ContactInsuranceAgent.ContactSubTypeID = 7   
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactInsuranceAdjuster  with(nolock) ON ContactInsuranceAdjuster.AdminInfoID = AdminInfo.id AND ContactInsuranceAdjuster.ContactTypeID = 1 AND ContactInsuranceAdjuster.ContactSubTypeID = 11   
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactClaimRep  with(nolock) ON ContactClaimRep.AdminInfoID = AdminInfo.id AND ContactClaimRep.ContactTypeID = 1 AND ContactClaimRep.ContactSubTypeID = 20   
   
	LEFT OUTER JOIN Vendor  with(nolock) ON Vendor.ID = EstimationData.RepairFacilityVendorID   
  
	LEFT OUTER JOIN VehicleInfo  with(nolock) ON VehicleInfo.EstimationDataId = EstimationData.id   
	LEFT OUTER JOIN VehicleInfoManual with(nolock)  ON (VehicleInfoManual.VehicleInfoID = VehicleInfo.ID)   
  
	LEFT OUTER JOIN Vinn.dbo.Engines  with(nolock) ON Engines.EngineID = VehicleInfo.EngineType  
	LEFT OUTER JOIN Vinn.dbo.Transmissions  with(nolock) ON Transmissions.TransmissionsID = VehicleInfo.TransmissionType  
	LEFT OUTER JOIN Vinn.dbo.Drives  with(nolock) ON Drives.DriveID = VehicleInfo.DriveType  
	LEFT OUTER JOIN Vinn.dbo.Bodys  with(nolock) ON Bodys.BodyID = VehicleInfo.BodyType  
  
	LEFT OUTER JOIN POI  with(nolock) ON VehicleInfo.POI_ID = POI.POI_ID  
   
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
			SELECT 'bOwner</b><br/>' + OwnerInfo FROM #Temp WHERE OwnerInfo <> ''  
  
			UNION  
			SELECT 'cInsurance Company</b><br/>' + InsuranceCompanyInfo FROM #Temp WHERE InsuranceCompanyInfo <> ''  
  
			UNION  
			SELECT 'dInsurance Adjuster</b><br/>' + InsuranceAdjusterInfo FROM #Temp WHERE InsuranceAdjusterInfo <> ''   
  
			UNION   
			SELECT 'eClaim Rep</b><br/>' + ClaimRepInfo FROM #Temp WHERE ClaimRepInfo <> ''  
  
			UNION   
			SELECT 'fRepair Facility</b><br/>' + RepairFacilityInfo FROM #Temp WHERE RepairFacilityInfo <> ''  
  
			UNION   
			SELECT 'gInsurance Agent</b><br/>' + InsuranceAgentInfo FROM #TEMP WHERE InsuranceAgentInfo <> ''  
  
			UNION   
			SELECT 'hShop Info</b><br/>' + OtherInfo FROM #TEMP WHERE OtherInfo <> ''  
		) Base  
	) PvtBase  
	PIVOT   
	(  
		MAX(Information)  
		FOR RowNum IN ([1], [2], [3], [4], [5], [6], [7], [8])  
	) pvt  
  
	SELECT DISTINCT TOP 1  
		AdminInfo.EstimateNumber   
		, AdminInfo.id AS EstimateID   
		, REPLACE(AdminInfo.Description, '&', '&amp;') AS EstimateDescription  
		, ISNULL(AdminInfo.WorkOrderNumber, 0) AS WorkOrderNumber   
		, (SELECT COUNT(*) FROM PaymentInfo WHERE AdminInfoID = @AdminInfoID) AS PaymentRecords   
		, (SELECT SUM(Amount) AS AmountPaid FROM PaymentInfo WHERE AdminInfoID = @AdminInfoID) As AmountPaid   
		, CAST(AdminInfo.GrandTotal AS MONEY) AS GrandTotal   
   
		, case when @IsExcludeImage=1  then 0 else   
		  (SELECT COUNT(*) FROM EstimationImages WHERE AdminInfoID = @AdminInfoID) end   
		  AS ImageCount   
		,   
		 REPLACE(  
		  CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN AltIdentity.CompanyName   
			ELSE OrganizationInfo.CompanyName   
		  END   
		  , '&', '&amp;') As CompanyName		    
   
		, CASE WHEN ISNULL(OrganizationInfo.PrintFederalTaxID, 0) = 1 THEN ISNULL(OrganizationInfo.FederalTaxID, '') ELSE '' END AS FederalTaxID   
		, ISNULL(OrganizationInfo.PrintFederalTaxID, 0) AS PrintFederalTaxID   
		, CASE WHEN ISNULL(OrganizationInfo.PrintRegistrationNumber, 0) = 1 THEN ISNULL(OrganizationInfo.RegistrationNumber, '') ELSE '' END AS RegistrationNumber   
		, ISNULL(OrganizationInfo.PrintRegistrationNumber, 0) AS PrintRegistrationNumber   
		, CASE WHEN ISNULL(OrganizationInfo.PrintLicenseNumber, 0) = 1 THEN ISNULL(OrganizationInfo.LicenseNumber, '') ELSE '' END AS LicenseNumber   
		, ISNULL(OrganizationInfo.PrintLicenseNumber, 0) AS PrintLicenseNumber   
		, CASE WHEN ISNULL(OrganizationInfo.PrintBarNumber, 0) = 1 THEN ISNULL(OrganizationInfo.BarNumber, '') ELSE '' END AS BarNumber   
		, ISNULL(OrganizationInfo.PrintBarNumber, 0) AS PrintBarNumber   
		   
		, REPLACE(ISNULL(ContactClaimRep.FirstName, '') + ' ' + ISNULL(ContactClaimRep.LastName, ''), '&', '&amp;') AS ClaimRepName   
		, dbo.FormatPhoneNumber(ContactClaimRep.Phone1) + CASE WHEN ISNULL(ContactClaimRep.Extension1, '') <> '' THEN ' x' + ContactClaimRep.Extension1 ELSE '' END AS ClaimRepPhone   
		, ISNULL(ContactClaimRep.Email, '') AS ClaimRepEmail   
		, dbo.FormatPhoneNumber(ContactClaimRep.FaxNumber) AS ClaimRepFaxNumber   
   
		, REPLACE(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN AltIdentity.FirstName + ' ' + AltIdentity.LastName    
			ELSE CompanyContact.FirstName + ' ' + CompanyContact.LastName    
		  END, '&', '&amp;') As CompanyContactName   
		, CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN IsNull(AltIdentity.WorkNumber, '')   
			ELSE dbo.FormatPhoneNumber(IsNull(CompanyContact.Phone1, '')) + ' ' + ISNULL(CompanyContact.Extension1, '')    
		  END AS CompanyContactCellPhone   
		, dbo.FormatPhoneNumber(IsNull(CompanyContact.Phone2, '')) + ' ' + ISNULL(CompanyContact.Extension2, '') AS CompanyContactSecondaryPhone  
		, CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
		    THEN dbo.FormatPhoneNumber(IsNull(AltIdentity.FaxNumber, ''))  
			ELSE dbo.FormatPhoneNumber(CompanyContact.FaxNumber)    
		  END AS CompanyContactFax   
		, CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN AltIdentity.Email   
			ELSE CompanyContact.Email    
		  END AS CompanyContactEmail   
		, Phone1Type.Report AS CompanyContactPhoneType1  
		, Phone2Type.Report AS CompanyContactPhoneType2  
		, REPLACE(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN ISNULL(AltIdentity.Address1, '') + ' ' + ISNULL(AltIdentity.Address2, '')  
			ELSE ISNULL(CompanyAddress.Address1, '') + ' ' + ISNULL(CompanyAddress.Address2, '')  
		  END, '&', '&amp;') AS CompanyAddress   
		, REPLACE(CASE WHEN ISNULL(AltIdentity.ID, 0) > 1    
			THEN ISNULL(AltIdentity.City, '') + CASE WHEN ISNULL(AltIdentity.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(AltIdentity.State, '') + CASE WHEN ISNULL(AltIdentity.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(AltIdentity.Zip, '')  
			ELSE ISNULL(CompanyAddress.City, '') + CASE WHEN ISNULL(CompanyAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(CompanyAddress.State, '') + CASE WHEN ISNULL(CompanyAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(CompanyAddress.zip, '')  
		  END, '&', '&amp;') AS CompanyCityStateZip   
		, CASE WHEN ISNULL(OrganizationInfo.LogoImageType, '') = ''   
			THEN @ContentDirectory + '/CompanyLogos/default.png'   
			ELSE @ContentDirectory + '/CompanyLogos/' + CAST(OrganizationInfo.ID AS VARCHAR) + '.' + OrganizationInfo.LogoImageType   
		  END AS CompanyLogoPath   
   
		, REPLACE(ISNULL(ContactOwner.FirstName, '') + ' ' + ISNULL(ContactOwner.LastName, ''), '&', '&amp;') AS [Owner]   
		, REPLACE(ContactOwner.BusinessName, '&', '&amp;') AS OwnerCompanyName   
		, dbo.FormatPhoneNumber(ISNULL(ContactOwner.Phone1, '')) + ' ' + ISNULL(ContactOwner.Extension1, '') AS OwnerCell   
		, ContactOwner.Email AS OwnerEmail   
		, REPLACE(OwnerAddress.Address1, '&', '&amp;') AS OwnerAddress1   
		, REPLACE(OwnerAddress.Address2, '&', '&amp;') AS OwnerAddress2   
		, REPLACE(ISNULL(OwnerAddress.City, '') + CASE WHEN ISNULL(OwnerAddress.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(OwnerAddress.State, '') + CASE WHEN ISNULL(OwnerAddress.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(OwnerAddress.zip, ''), '&', '&amp;') AS OwnerCityStateZip   
  
		, REPLACE(ISNULL(ContactInsuranceAgent.FirstName, '') + ' ' + ISNULL(ContactInsuranceAgent.LastName, ''), '&', '&amp;') AS InsuranceAgentName   
		, dbo.FormatPhoneNumber(ContactInsuranceAgent.Phone1) + CASE WHEN ISNULL(ContactInsuranceAgent.Extension1, '') <> '' THEN ' x' + ContactInsuranceAgent.Extension1 ELSE '' END AS InsuranceAgentCell   
		, ISNULL(ContactInsuranceAgent.Email, '') AS InsuranceAgentEmail   
		, dbo.FormatPhoneNumber(ContactInsuranceAgent.FaxNumber) AS InsuranceAgentFax   
   
		, REPLACE(ISNULL(ContactInsuranceAdjuster.FirstName + ' ' + ContactInsuranceAdjuster.LastName, ' '), '&', '&amp;') AS InsuranceAdjusterName   
		, dbo.FormatPhoneNumber(ContactInsuranceAdjuster.Phone1) + CASE WHEN ISNULL(ContactInsuranceAdjuster.Extension1, '') <> '' THEN ' x' + ContactInsuranceAdjuster.Extension1 ELSE '' END AS InsuranceAdjusterCell   
		, ISNULL(ContactInsuranceAdjuster.Email, '') AS InsuranceAdjusterEmail   
		, dbo.FormatPhoneNumber(ContactInsuranceAdjuster.FaxNumber) AS InsuranceAdjusterFax   
   
		, CASE WHEN ISNULL(CustomerProfilePrint.PrintEstimator, 0) = 1 THEN REPLACE(ISNULL(EstimatorsData.AuthorFirstName, '') + ' ' + ISNULL(EstimatorsData.AuthorLastName, ''), '&', '&amp;') ELSE '' END As EstimatorsName   
   
		, CASE WHEN ISNULL(VehicleInfoManual.ManualSelection, 0) = 1 THEN   
				ISNULL(VehicleInfoManual.ModelYear,'') + ' ' +    
				ISNULL(VehicleInfoManual.Make,'') + ' ' +   
				ISNULL(VehicleInfoManual.Model,'') + ' ' +    
				ISNULL(VehicleInfoManual.SubModel,'')   
			ELSE   
				FocusWrite.dbo.GetVehicleName(VehicleInfo.VehicleID)   
		END  AS VDesc   
		, VehicleInfo.Vin   
		, REPLACE(VehicleInfo.ExtColor, '&', '&amp;') AS ExtColor  
		, REPLACE(VehicleInfo.IntColor , '&', '&amp;') AS IntColor  
		, VehicleInfo.License   
		, VehicleInfo.MilesIn   
		, VehicleInfo.MilesOut   
   
		, REPLACE(ISNULL(Vendor.CompanyName, ''), '&', '&amp;') AS RepairFacilityCompanyName   
		, REPLACE(ISNULL(Vendor.FirstName, '') + ' ' + ISNULL(Vendor.LastName, ''), '&', '&amp;') AS RepairFacilityName   
		, REPLACE(ISNULL(Vendor.Address1, ''), '&', '&amp;') AS RepairFacilityAddress   
		, REPLACE(ISNULL(Vendor.Address2, ''), '&', '&amp;') AS RepairFacilityAddress2   
		, ISNULL(Vendor.City, '') + CASE WHEN ISNULL(Vendor.City, '') <> '' THEN ', ' ELSE '' END + ISNULL(Vendor.State, '') + CASE WHEN ISNULL(Vendor.State, '') <> '' THEN ' ' ELSE '' END + ISNULL(Vendor.zip, '') AS RepairFacilityCityStateZip   
  
		, dbo.FormatPhoneNumber(Vendor.MobileNumber) AS RepairFacilityMobileNumber   
		, dbo.FormatPhoneNumber(Vendor.WorkNumber) AS RepairFacilityWorkNumber   
		, ISNULL(Vendor.Email, '') AS RepairFacilityEmail   
   
		, REPLACE(ISNULL(EstimationData.InsuranceCompanyName, ''), '&', '&amp;') AS InsuranceCompanyName   
		, REPLACE(CASE WHEN ISNULL(EstimationData.PrintNote, 0) = 1 THEN ISNULL(EstimationData.Note, '') ELSE '' END, '&', '&amp;') AS EstimateNote   
		, EstimationData.PrintNote   
		, ISNULL(EstimationData.PolicyNumber, '') AS PolicyNumber   
		, ISNULL(EstimationData.ClaimNumber, '') AS ClaimNumber   
		, ISNULL(EstimationData.EstimationDate, '') AS EstimationDate   
		, ISNULL(EstimationData.AssignmentDate, '') AS AssignmentDate   
		, ISNULL(EstimationData.Deductible, '') AS Deductible   
		, ISNULL(EstimationData.DateOfLoss, '') AS DateOfLoss   
		, CAST(CASE WHEN EstimationData.EstimationDate IS NULL AND EstimationData.AssignmentDate IS NULL THEN 0 ELSE 1 END AS BIT) As ShowDates   
   
		, ISNULL(EstimationData.LockLevel, 0) As SupplementLevel   
   
		--, REPLACE(REPLACE(REPLACE(CustomerProfilePrint.FooterText, '&', '&amp;'),'>','&gt;'),'<','&lt;') as CompanyNotes  
		,  ISNULL(CustomerProfilePrint.FooterText, '') AS CompanyNotes   
		, CustomerProfilePrint.NoHeaderLogo   
		, #DetailsSections.*  
	FROM AdminInfo    with(nolock) 
	LEFT OUTER JOIN EstimationData  with(nolock) ON EstimationData.AdminInfoID = AdminInfo.id    
	LEFT OUTER JOIN Logins  with(nolock) ON AdminInfo.CreatorID = Logins.ID  
   
	LEFT OUTER JOIN OrganizationInfo  with(nolock) ON Logins.OrganizationID = OrganizationInfo.ID   
   
	LEFT OUTER JOIN Vendor AS AltIdentity  with(nolock) ON AltIdentity.ID = EstimationData.AlternateIdentitiesID   
   
	LEFT OUTER JOIN tbl_ContactPerson AS CompanyContact  with(nolock) ON CompanyContact.ContactID = OrganizationInfo.OrgInfoContactsID   
	LEFT OUTER JOIN tbl_Address AS CompanyAddress  with(nolock) ON CompanyAddress.ContactsID = OrganizationInfo.OrgInfoContactsID   
	LEFT OUTER JOIN PhoneTypes AS Phone1Type  with(nolock) ON Phone1Type.Code = CompanyContact.PhoneNumberType1  
	LEFT OUTER JOIN PhoneTypes AS Phone2Type  with(nolock) ON Phone2Type.Code = CompanyContact.PhoneNumberType2  
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactOwner  with(nolock) ON ContactOwner.AdmininfoID = AdminInfo.id AND ContactOwner.ContactSubTypeID = 4   
	LEFT OUTER JOIN tbl_Address AS OwnerAddress  with(nolock) ON ContactOwner.ContactID = OwnerAddress.ContactsID   
	   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactInsuranceAgent  with(nolock) ON ContactInsuranceAgent.AdminInfoID = AdminInfo.id AND ContactInsuranceAgent.ContactTypeID = 1 AND ContactInsuranceAgent.ContactSubTypeID = 7   
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactInsuranceAdjuster  with(nolock) ON ContactInsuranceAdjuster.AdminInfoID = AdminInfo.id AND ContactInsuranceAdjuster.ContactTypeID = 1 AND ContactInsuranceAdjuster.ContactSubTypeID = 11   
   
	LEFT OUTER JOIN tbl_ContactPerson AS ContactClaimRep  with(nolock) ON ContactClaimRep.AdminInfoID = AdminInfo.id AND ContactClaimRep.ContactTypeID = 1 AND ContactClaimRep.ContactSubTypeID = 20   
   
	LEFT OUTER JOIN VehicleInfo  with(nolock) ON VehicleInfo.EstimationDataId = EstimationData.id   
	LEFT OUTER JOIN VehicleInfoManual  with(nolock) ON (VehicleInfoManual.VehicleInfoID = VehicleInfo.ID) 	  
   
	LEFT OUTER JOIN EstimatorsData  with(nolock) ON EstimatorsData.EstimatorID = EstimationData.EstimatorID   
   
	LEFT OUTER JOIN Vendor  with(nolock) ON Vendor.ID = EstimationData.RepairFacilityVendorID   
   
	LEFT OUTER JOIN CustomerProfilePrint  with(nolock) ON AdminInfo.CustomerProfilesID = CustomerProfilePrint.CustomerProfilesID   
   
	LEFT OUTER JOIN #DetailsSections ON 1 = 1  
  
	WHERE AdminInfo.ID = @AdminInfoID    
END   
   

GO
