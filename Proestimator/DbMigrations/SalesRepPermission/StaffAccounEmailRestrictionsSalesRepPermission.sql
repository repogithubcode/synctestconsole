BEGIN TRY

BEGIN TRAN
	DECLARE @bnaborsID AS INT
	SELECT @bnaborsID = SalesRepID FROM [dbo].[SalesRep] WHERE Email = 'bnabors@web-est.com';

	DECLARE @cortizID AS INT
	SELECT @cortizID = SalesRepID FROM [dbo].[SalesRep] WHERE Email = 'cortiz@web-est.com';

	DECLARE @mhoughID AS INT
	SELECT @mhoughID = SalesRepID FROM [dbo].[SalesRep] WHERE Email = 'mhough@web-est.com';

	DECLARE @permissionID AS INT

	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentAccountRenewOn','EmailSentAccountRenewOn','Has permssion to receive email for account renew on');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentAutoRenewOff','IsEmailSentAutoRenewOff','Has permssion to receive email for auto renew off');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentAutoRenewWarning','IsEmailSentAutoRenewWarning','Has permssion to receive email for auto renew warning');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentAutoRenewComplete','IsEmailSentAutoRenewComplete','Has permssion to receive email for auto renew complete');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentNewAddOn','IsEmailSentNewAddOn','Has permssion to receive email for new addon');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentAutoRenewUponAutPay','IsEmailSentAutoRenewUponAutPay','Has permssion to receive email for auto renew upon auto pay');
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentContractEarlyRenewDone','IsEmailSentContractEarlyRenewDone','Has permssion to receive email for contract early renew done');

	-- IsEmailSentImportWELogin
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentImportWELogin','IsEmailSentImportWELogin','Has permssion to receive email for import WE login');
	SELECT @permissionID = MAX(ID) FROM SalesRepPermission
	INSERT INTO SalesRepPermissionLink(SalesRepID, PermissionID, HasPermission) VALUES(@mhoughID, @permissionID, 1)

	-- IsEmailSentInsertSalesBoard
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentInsertSalesBoard','IsEmailSentInsertSalesBoard','Has permssion to receive email for insert sales board');
	SELECT @permissionID = MAX(ID) FROM SalesRepPermission
	INSERT INTO SalesRepPermissionLink(SalesRepID, PermissionID, HasPermission) VALUES(@bnaborsID, @permissionID, 1)
	INSERT INTO SalesRepPermissionLink(SalesRepID, PermissionID, HasPermission) VALUES(@cortizID, @permissionID, 1)

	-- IsEmailSentSignContract
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description]) VALUES(255,'IsEmailSentSignContract','IsEmailSentSignContract','Has permssion to receive email for sign contract');
	SELECT @permissionID = MAX(ID) FROM SalesRepPermission
	INSERT INTO SalesRepPermissionLink(SalesRepID, PermissionID, HasPermission) VALUES(@cortizID, @permissionID, 1)

	COMMIT TRAN

	PRINT('SalesRepPermission and SalesRepPermissionLink data population completed successfully.');

END TRY
BEGIN CATCH

	PRINT('Error in SalesRepPermission and SalesRepPermissionLink data population.');
	ROLLBACK TRAN

END CATCH




