USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

Create PROCEDURE [dbo].[EMS_Export_GetAdministrative1_201]	
	@AdminInfoID Int
AS

SET NOCOUNT ON
SET FMTONLY OFF

DECLARE @PolicyNumber VarChar(25)
DECLARE @ClaimNumber VarChar(25)
DECLARE @Deductible Money

SELECT @Deductible = (SELECT ISNULL(Deductible, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID)
SELECT @PolicyNumber = (SELECT ISNULL(PolicyNumber, '') FROM EstimationData WHERE AdminInfoID = @AdminInfoID)
SELECT @ClaimNumber = (SELECT ISNULL(ClaimNumber, '') FROM EstimationData WHERE AdminInfoID = @AdminInfoID)

CREATE TABLE #InsuranceInfo (
	ContactsID Int,
	CompanyName VarChar (255)  NULL ,
	CompanyIDCode VarChar (255)  NULL ,
	ContactFirstName VarChar (255)  NULL ,
	ContactLastName VarChar (255)  NULL ,
	AddressLine1 VarChar (255)  NULL ,
	AddressLine2 VarChar (255)  NULL ,
	City VarChar (255)  NULL ,
	State VarChar (255)  NULL ,
	Zip VarChar (255)  NULL ,
	Phone1 VarChar (255)  NULL ,
	Phone2 VarChar (255)  NULL ,
	Fax VarChar (255)  NULL ,
	EmailAddress VarChar(255) NULL ,
	Country VarChar (25)  NULL)

INSERT INTO #InsuranceInfo
SELECT 0, InsuranceCompanyName, '', '', '', '', '', '', '', '', '', '', '', '', ''
FROM EstimationData
WHERE AdminInfoID = @AdminInfoID
	

CREATE TABLE #ClaimOfficeInfo (
	ContactsID Int,
	CompanyName VarChar (255)  NULL ,
	CompanyIDCode VarChar (255)  NULL ,
	ContactFirstName VarChar (255)  NULL ,
	ContactLastName VarChar (255)  NULL ,
	AddressLine1 VarChar (255)  NULL ,
	AddressLine2 VarChar (255)  NULL ,
	City VarChar (255)  NULL ,
	State VarChar (255)  NULL ,
	Zip VarChar (255)  NULL ,
	Phone1 VarChar (255)  NULL ,
	Phone2 VarChar (255)  NULL ,
	Fax VarChar (255)  NULL  ,
	EmailAddress VarChar(255) NULL,
	Country VarChar (25)  NULL)


INSERT INTO #ClaimOfficeInfo
	--EXECUTE GetContactDetails @AdminInfoID, 6 --Claims Office
	SELECT ContactID, BusinessName, '', FirstName, LastName, Address1, Address2, City, State, Zip, Phone1, Phone2, FaxNumber, Email, Country
	FROM tbl_ContactPerson
	LEFT OUTER JOIN tbl_Address ON tbl_Address.ContactsID = tbl_ContactPerson.ContactID
	WHERE tbl_ContactPerson.AdminInfoID = @AdminInfoID
	AND ContactSubTypeID = 20


CREATE TABLE #InsuranceAgent (
	ContactsID Int,
	CompanyName VarChar (255)  NULL ,
	CompanyIDCode VarChar (255)  NULL ,
	ContactFirstName VarChar (255)  NULL ,
	ContactLastName VarChar (255)  NULL ,
	AddressLine1 VarChar (255)  NULL ,
	AddressLine2 VarChar (255)  NULL ,
	City VarChar (255)  NULL ,
	State VarChar (255)  NULL ,
	Zip VarChar (255)  NULL ,
	Phone1 VarChar (255)  NULL ,
	Phone2 VarChar (255)  NULL ,
	Fax VarChar (255)  NULL  ,
	EmailAddress VarChar(255) NULL,
	Country VarChar (25)  NULL)

INSERT INTO #InsuranceAgent
	--EXECUTE GetContactDetails @AdminInfoID, 7 --Insurance Agent
	SELECT ContactID, BusinessName, '', FirstName, LastName, Address1, Address2, City, State, Zip, Phone1, Phone2, FaxNumber, Email, Country
	FROM tbl_ContactPerson
	LEFT OUTER JOIN tbl_Address ON tbl_Address.ContactsID = tbl_ContactPerson.ContactID
	WHERE tbl_ContactPerson.AdminInfoID = @AdminInfoID
	AND ContactSubTypeID = 7


CREATE TABLE #Insured (
	ContactsID Int,
	CompanyName VarChar (255)  NULL ,
	CompanyIDCode VarChar (255)  NULL ,
	ContactFirstName VarChar (255)  NULL ,
	ContactLastName VarChar (255)  NULL ,
	AddressLine1 VarChar (255)  NULL ,
	AddressLine2 VarChar (255)  NULL ,
	City VarChar (255)  NULL ,
	State VarChar (255)  NULL ,
	Zip VarChar (255)  NULL ,
	Phone1 VarChar (255)  NULL ,
	Phone2 VarChar (255)  NULL ,
	Fax VarChar (255)  NULL ,
	EmailAddress VarChar(255) NULL ,
	Country VarChar (25)  NULL)

IF (SELECT ISNULL(InsuredSameAsOwner, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID) = 1
	BEGIN
		INSERT INTO #Insured
		SELECT tbl_ContactPerson.ContactID, tbl_ContactPerson.BusinessName, '', tbl_ContactPerson.FirstName, tbl_ContactPerson.LastName, tbl_Address.Address1, tbl_Address.Address2, tbl_Address.City, tbl_Address.State, tbl_Address.Zip, dbo.getNumericValue(tbl_ContactPerson.Phone1), dbo.getNumericValue(tbl_ContactPerson.Phone2), dbo.getNumericValue(tbl_ContactPerson.FaxNumber), tbl_ContactPerson.Email, tbl_Address.Country
		FROM AdminInfo
		LEFT JOIN Customer ON AdminInfo.CustomerID = Customer.ID
		LEFT JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID
		LEFT JOIN tbl_Address ON Customer.AddressID = tbl_Address.AddressID
		WHERE AdminInfo.ID = @AdminInfoID  
	END
ELSE
	BEGIN
		INSERT INTO #Insured
		SELECT ContactID, BusinessName, '', FirstName, LastName, Address1, Address2, City, State, Zip, Phone1, Phone2, FaxNumber, Email, Country 
		FROM tbl_ContactPerson 
		LEFT OUTER JOIN tbl_Address ON tbl_Address.ContactsID = tbl_ContactPerson.ContactID 
		LEFT OUTER JOIN EstimationData ON EstimationData.AdminInfoID = @AdminInfoID 
		WHERE tbl_ContactPerson.AdminInfoID = @AdminInfoID 
		AND ContactSubTypeID = CASE WHEN ISNULL(EstimationData.InsuredSameAsOwner, 0) = 1 THEN 4 ELSE 2 END 
	END

CREATE TABLE #Owner (
	ContactsID Int,
	CompanyName VarChar (255)  NULL ,
	CompanyIDCode VarChar (255)  NULL ,
	ContactFirstName VarChar (255)  NULL ,
	ContactLastName VarChar (255)  NULL ,
	AddressLine1 VarChar (255)  NULL ,
	AddressLine2 VarChar (255)  NULL ,
	City VarChar (255)  NULL ,
	State VarChar (255)  NULL ,
	Zip VarChar (255)  NULL ,
	Phone1 VarChar (255)  NULL ,
	Phone2 VarChar (255)  NULL ,
	Fax VarChar (255)  NULL ,
	EmailAddress VarChar(255) NULL ,
	Country VarChar (25)  NULL)

INSERT INTO #Owner
	SELECT tbl_ContactPerson.ContactID, tbl_ContactPerson.BusinessName, '', tbl_ContactPerson.FirstName, tbl_ContactPerson.LastName, tbl_Address.Address1, tbl_Address.Address2, tbl_Address.City, tbl_Address.State, tbl_Address.Zip, dbo.getNumericValue(tbl_ContactPerson.Phone1), dbo.getNumericValue(tbl_ContactPerson.Phone2), dbo.getNumericValue(tbl_ContactPerson.FaxNumber), tbl_ContactPerson.Email, tbl_Address.Country
	FROM AdminInfo
	LEFT JOIN Customer ON AdminInfo.CustomerID = Customer.ID
	LEFT JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID
	LEFT JOIN tbl_Address ON Customer.AddressID = tbl_Address.AddressID
	WHERE AdminInfo.ID = @AdminInfoID  

SELECT TOP 1
	nullif(CAST(LEFT(II.CompanyIDCode,5) as nvarchar(5)),'')	INS_CO_ID,
	nullif(CAST(LEFT(II.CompanyName,35) as nvarchar(35)) ,'')		INS_CO_NM,
	nullif(CAST(LEFT(II.AddressLine1,40) as nvarchar(40)),'')	INS_ADDR1,
	nullif(CAST(LEFT(II.AddressLine2,40) as nvarchar(40)),'')	INS_ADDR2,
	nullif(CAST(LEFT(II.City,30) as nvarchar(30)),'')		INS_CITY,
	nullif(CAST(LEFT(II.State,2) as nvarchar(2)),'')		INS_ST,
	nullif(cast(LEFT(II.Zip,11) as nvarchar(11)),'')			INS_ZIP,
	nullif(cast('USA' as nvarchar(3)),'')				INS_CTRY,
	nullif(cast(LEFT(dbo.getNumericValue(II.Phone1),10) as nvarchar(10)),'')		INS_PH1,
	NULL AS INS_PH1X,
	nullif(cast(LEFT(dbo.getNumericValue(II.Phone2),10)	as nvarchar(10)),'')	INS_PH2,
	NULL AS INS_PH2X,
	nullif(cast(LEFT(dbo.getNumericValue(II.Fax),10) as nvarchar(10)),'')			INS_FAX,
	NULL AS INS_FAXX,
	nullif(cast(LEFT(II.ContactLastName,35) as nvarchar(35)),'')	INS_CT_LN,
	nullif(cast(LEFT(II.ContactFirstName,35) as nvarchar(35)),'')	INS_CT_FN,
	NULL AS INS_TITLE,
	NULL AS INS_CT_PH,
	NULL AS INS_CT_PHX,
	nullif(cast(LEFT(II.EmailAddress,80) as nvarchar(80)),'')	INS_EA,
	NULL AS INS_MEMO,
	nullif(cast(LEFT(@PolicyNumber,30) as nvarchar(30)),'')		POLICY_NO,
	@Deductible			DED_AMT,
	cast(case when ISNULL(@Deductible,0) = 0 then 'U' else 'Y' end as nvarchar(1)) as DED_STATUS,
	NULL AS ASGN_NO,
	NULL AS ASGN_DATE,
	NULL AS ASGN_TYPE,
	nullif(cast(LEFT(@ClaimNumber,30) as nvarchar(30))	,'')	CLM_NO,
	nullif(cast(LEFT(CLO.CompanyIDCode,5) as nvarchar(5)),'')	CLM_OFC_ID,
	nullif(cast(LEFT(CLO.CompanyName,35) as nvarchar(35)),'')	CLM_OFC_NM,
	nullif(cast(LEFT(CLO.AddressLine1,40) as nvarchar(40)),'')	CLM_ADDR1,
	nullif(cast(LEFT(CLO.AddressLine2,40) as nvarchar(40)),'')	CLM_ADDR2,
	nullif(cast(LEFT(CLO.City,30) as nvarchar(30)),'')		CLM_CITY,
	nullif(cast(LEFT(CLO.State,2) as nvarchar(2)),'')		CLM_ST,
	nullif(cast(LEFT(CLO.Zip,11) as nvarchar(11)),'')		CLM_ZIP,
	nullif(cast('USA' as nvarchar(3)),'')				CLM_CTRY,
	nullif(cast(LEFT(dbo.getNumericValue(CLO.Phone1),10) as nvarchar(10)),'')		CLM_PH1,
	NULL AS CLM_PH1X,
	nullif(cast(LEFT(dbo.getNumericValue(CLO.Phone2),10) as nvarchar(10))	,'')	CLM_PH2,
	NULL AS CLM_PH2X,
	nullif(cast(LEFT(dbo.getNumericValue(CLO.Fax),10) as nvarchar(10)),'')		CLM_FAX,
	NULL AS CLM_FAXX,
	nullif(cast(LEFT(CLO.ContactLastName,35) as nvarchar(35)),'')	CLM_CT_LN,
	nullif(cast(LEFT(CLO.ContactFirstName,35) as nvarchar(35)),'')	CLM_CT_FN,
	nullif(cast(LEFT(CLO.EmailAddress,80) as nvarchar(35)),'') CLM_TITLE,
	0 CLM_CT_PH,
	NULL AS CLM_CT_PHX,
	NULL AS CLM_EA,
	NULL AS PAYEE_NMS,
	NULL AS PAY_TYPE,
	NULL AS PAY_DATE,
	NULL AS PAY_CHKNM,
	NULL AS PAY_AMT,
	NULL AS PAY_MEMO,	
	nullif(cast(LEFT(IA.CompanyIDCode,5) as nvarchar(5)),'')	AGT_CO_ID,
	nullif(cast(LEFT(IA.CompanyName,35) as nvarchar(35)),'')		AGT_CO_NM,
	nullif(cast(LEFT(IA.AddressLine1,40) as nvarchar(40)),'')	AGT_ADDR1,
	nullif(cast(LEFT(IA.AddressLine2,40) as nvarchar(40)),'')	AGT_ADDR2,
	nullif(cast(LEFT(IA.City,30) as nvarchar(30))	,'')	AGT_CITY,
	nullif(cast(LEFT(IA.State,2) as nvarchar(2)),'')		AGT_ST,
	nullif(cast(LEFT(IA.Zip,11) as nvarchar(11)),'')			AGT_ZIP,
	nullif(cast('USA' as nvarchar(3)),'')				AGT_CTRY,
	nullif(cast(LEFT(dbo.getNumericValue(IA.Phone1),10) as nvarchar(10)),'')		AGT_PH1,
	NULL AS AGT_PH1X,
	nullif(cast(LEFT(dbo.getNumericValue(IA.Phone2),10) as nvarchar(10)),'')		AGT_PH2,
	NULL AS AGT_PH2X,
	nullif(cast(LEFT(dbo.getNumericValue(IA.Fax),10) as nvarchar(10)),'')			AGT_FAX,
	NULL AS AGT_FAXX,
	nullif(cast(LEFT(IA.ContactLastName,35) as nvarchar(35)),'')	AGT_CT_LN,
	nullif(cast(LEFT(IA.ContactFirstName,35) as nvarchar(35)),'')	AGT_CT_FN,
	NULL AS AGT_CT_PH,
	NULL AS AGT_CT_PHX,
	nullif(cast(LEFT(IA.EmailAddress,80) as nvarchar(80)),'')	AGT_EA,
	NULL AS AGT_LIC_NO,
	CASE 	WHEN DATEDIFF(d, DateOfLoss,'1990-01-01 00:00:00.000') > 0 THEN NULL 
		ELSE 	DateOfLoss			
	END			LOSS_DATE,
	cast('C' as nvarchar(1)) as LOSS_CAT,
	0 LOSS_TYPE,
	NULL AS LOSS_DESC,
	0 THEFT_IND,
	NULL AS CAT_NO,
	NULL AS TLOS_IND,
	NULL AS LOSS_MEMO	,
	cast('O' as nvarchar(1)) as CUST_PR,
	nullif(cast(LEFT(Ins.ContactLastName,35) as nvarchar(35)),'')	INSD_LN,
	nullif(cast(LEFT(Ins.ContactFirstName,35) as nvarchar(35)),'')	INSD_FN,
	NULL AS INSD_TITLE,
	NULL AS INSD_CO_NM,
	nullif(cast(LEFT(Ins.AddressLine1,40) as nvarchar(40)),'')	INSD_ADDR1,
	nullif(cast(LEFT(Ins.AddressLine2,40) as nvarchar(40)),'')	INSD_ADDR2,
	nullif(cast(LEFT(Ins.City,30) as nvarchar(30)),'')		INSD_CITY,
	nullif(cast(LEFT(Ins.State,2) as nvarchar(2)),'')		INSD_ST,
	nullif(cast(LEFT(Ins.Zip,11) as nvarchar(11)),'')		INSD_ZIP,
	nullif(cast('USA' as nvarchar(3)),'')				INSD_CTRY,
	nullif(cast(LEFT(dbo.getNumericValue(Ins.Phone1),10) as nvarchar(10)),'')		INSD_PH1,
	NULL AS INSD_PH1X,
	nullif(cast(LEFT(dbo.getNumericValue(Ins.Phone2),10) as nvarchar(10)),'')		INSD_PH2,
	NULL AS INSD_PH2X,
	nullif(cast(LEFT(dbo.getNumericValue(Ins.Fax),10) as nvarchar(10)),'')		INSD_FAX,
	NULL AS INSD_FAXX,
	nullif(cast(LEFT(Ins.EmailAddress,80) as nvarchar(80)),'')	INSD_EA,
	NULL AS OWNR_TITLE,
	NULL AS OWNR_CO_NM,					
	nullif(cast(LEFT(Own.ContactLastName,35) as nvarchar(35)),'')	OWNR_LN,
	nullif(cast(LEFT(Own.ContactFirstName,35) as nvarchar(35)),'')	OWNR_FN,
	nullif(cast(LEFT(Own.AddressLine1,40) as nvarchar(40)),'')	OWNR_ADDR1,
	nullif(cast(LEFT(Own.AddressLine2,40) as nvarchar(40)),'')	OWNR_ADDR2,
	nullif(cast(LEFT(Own.City,30) as nvarchar(30)),'')		OWNR_CITY,
	nullif(cast(LEFT(Own.State,2) as nvarchar(2)),'')		OWNR_ST,
	nullif(cast(LEFT(Own.Zip,11) as nvarchar(11)),'')		OWNR_ZIP,
	nullif(cast('USA' as nvarchar(3))	,'')			OWNR_CTRY,
	nullif(cast(LEFT(dbo.getNumericValue(Own.Phone1),10) as nvarchar(10)),'')		OWNR_PH1,
	NULL AS OWNR_PH1X,
	nullif(cast(LEFT(dbo.getNumericValue(Own.Phone2),10) as nvarchar(10)),'')		OWNR_PH2,
	NULL AS OWNR_PH2X,
	nullif(cast(LEFT(dbo.getNumericValue(Own.Fax),10) as nvarchar(10)),'')		OWNR_FAX,
	NULL AS OWNR_FAXX,
	nullif(cast(LEFT(Own.EmailAddress,80) as nvarchar(80)),'')	OWNR_EA
FROM #InsuranceInfo II 
	full join #ClaimOfficeInfo CLO on (1=1)
	full join #InsuranceAgent IA on (1=1)
	full join #Insured Ins on (1=1)
	full join #Owner Own on (1=1)
	full join EstimationData on (1=1)
WHERE EstimationData.AdminInfoID = @AdminInfoID
	
DROP TABLE #InsuranceInfo
DROP TABLE #ClaimOfficeInfo
DROP TABLE #InsuranceAgent
DROP TABLE #Insured
DROP TABLE #Owner







GO
