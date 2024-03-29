USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[aaaDeleteLoginsData]
	@LoginsID		int
AS
BEGIN


--DELETE Contract_DW FROM Contract_DW JOIN Contract ON Contract_DW.ContractID = Contract.ContractID WHERE Contract_DW.LoginID = @LoginsID
--DELETE Contract_Unused FROM Contract_Unused JOIN Contract ON Contract_Unused.ContractID = Contract.ContractID WHERE Contract_Unused.LoginID = @LoginsID
DELETE InvoicePaidTracking FROM Invoice JOIN InvoicePaidTracking ON InvoicePaidTracking.InvoiceID = Invoice.InvoiceID WHERE Invoice.LoginID = @LoginsID
--DELETE AlternateIdentities FROM AlternateIdentities JOIN OrganizationInfo ON OrganizationInfo.id = AlternateIdentities.OrganizationInfoID JOIN Logins ON OrganizationInfo.id = Logins.OrganizationID WHERE Logins.id = @LoginsID
DELETE Invoice FROM Invoice JOIN Contract ON Invoice.ContractID = Contract.ContractID WHERE Invoice.LoginID = @LoginsID
DELETE Contracts FROM Contracts WHERE LoginID = @LoginsID
DELETE Trial FROM Trial WHERE LoginID = @LoginsID
DELETE Invoices FROM Invoices WHERE LoginID = @LoginsID
DELETE Vendor FROM Vendor WHERE LoginsID = @LoginsID

DELETE CustomerProfilePresetsLabor FROM CustomerProfilePresetsLabor JOIN CustomerProfilePresets ON CustomerProfilePresetsLabor.CustomerProfilePresetsID = CustomerProfilePresets.ID JOIN CustomerProfiles ON CustomerProfilePresets.CustomerProfilesID = CustomerProfiles.ID WHERE CustomerProfiles.OwnerID = @LoginsID
DELETE CustomerProfilePresetsNotes FROM CustomerProfilePresetsNotes JOIN CustomerProfilePresets ON CustomerProfilePresetsNotes.CustomerProfilePresetsID = CustomerProfilePresets.ID JOIN CustomerProfiles ON CustomerProfilePresets.CustomerProfilesID = CustomerProfiles.ID WHERE CustomerProfiles.OwnerID = @LoginsID
DELETE CustomerProfileRates FROM CustomerProfileRates JOIN CustomerProfiles ON CustomerProfileRates.CustomerProfilesID = CustomerProfiles.id WHERE CustomerProfiles.OwnerID = @LoginsID
DELETE CustomerProfilesMisc FROM CustomerProfilesMisc JOIN CustomerProfiles ON CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id WHERE CustomerProfiles.OwnerID = @LoginsID
DELETE CustomerProfilesPaint FROM CustomerProfilesPaint JOIN CustomerProfiles ON CustomerProfilesPaint.CustomerProfilesID = CustomerProfiles.id WHERE CustomerProfiles.OwnerID = @LoginsID

DELETE CustomerProfilePresets FROM CustomerProfilePresets JOIN CustomerProfiles ON CustomerProfilePresets.CustomerProfilesID = CustomerProfiles.id WHERE CustomerProfiles.OwnerID = @LoginsID
DELETE FROM CustomerProfiles WHERE OwnerID = @LoginsID

DELETE Contract FROM ContractLogins JOIN Contract ON ContractLogins.ContractID = Contract.ContractID WHERE ContractLogins.LoginID = @LoginsID
--DELETE OrganizationInfo FROM Logins JOIN OrganizationInfo ON OrganizationInfo.id = Logins.OrganizationID WHERE Logins.id = @LoginsID
DELETE FROM Logins WHERE id = @LoginsID

DELETE FROM EstimatorsData WHERE LoginID = @LoginsID

END
GO
