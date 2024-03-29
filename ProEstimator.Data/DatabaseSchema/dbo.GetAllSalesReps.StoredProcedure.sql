USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 2/28/2019 
-- Description:	Get all sales reps 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetAllSalesReps] 
 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	OPEN SYMMETRIC KEY WebEstEncryptionKey
	DECRYPTION BY CERTIFICATE WebEstEncryptionCertificate


	SELECT		
		  SalesRepID
		, SalesNumber
		, FirstName
		, LastName
		, Email
		, UserName
		, Convert(varchar, DecryptByKey(Password)) AS Password
		, Deleted
		, [pInvoiceTab]
		, [pOrgMaintTab]
		, [pSalesRepMaint]
		, [pSalesBoard]
		, [pServerLogsTab]
		, [pLoginFailureTab]
		, [pErrorsTab]
		, [pCurrentSessionsTab]
		, [pLinkingTab]
		, [pPaymentReport]
		, [pForcastedRevReport]
		, [pRenewalReport]
		, [pRoyaltyReport]
		, [pExpectedRenReport]
		, [pShopActivityReport]
		, [pWebsiteAccessReport]
		, [pUnusedContracts]
		, [pEstimatesByShop]
		, [pUserMaintLoginInfo]
		, [pUserMaintOrgInfo]
		, [pUserMaintContactInfo]
		, [pUserMaintSalesRep]
		, [pEditPermissions]
		, [pEditBonusGoals]
		, [pPromoMaintenance]
		, [pExtensionReport]
		, [pUserMaintImpersonate]
		, [pLoginAttempts]
		, [pUserMaintCreate]
		, [pImport]
		, [pImportEst]
		, [PhoneNumber]
		, [PhoneExtension]
		, [isSalesRep]
		, [pDeleteRenewal]
		, [Active]
		, ISNULL([pCarfax],0) AS pCarfax
		, ISNULL([pSuccessBox],0) AS pSuccessBox
		, ISNULL([pPartsNowManager],0) AS pPartsNowManager 
		, pAddOns
		, pDocumentsManagerUpload
		, pDocumentsManagerDownload
		, pTrialSetup
	FROM SalesRep
 
END 
GO
