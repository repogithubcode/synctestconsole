USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- EXEC [dbo].[SetDefaultRateProfile] 113941

CREATE PROCEDURE [dbo].[SetDefaultRateProfile] 
	@LoginID		int 
AS 
BEGIN 
	 
	 DECLARE @countCustomerProfileForLogin INT
	 DECLARE @countPdrRateProfileForLogin INT
	 DECLARE @countAddOnPresetProfileForLogin INT

	 -- Rate Profile tab
	 SELECT @countCustomerProfileForLogin = (SELECT COUNT(*) FROM [dbo].[GetCustomerProfileForLogin](@LoginID,0,0))
	 IF (@countCustomerProfileForLogin = 1)
	 BEGIN
		UPDATE dbo.[Logins] SET UseDefaultRateProfile = 1 WHERE ID = @LoginID
	 END
	 ELSE IF (@countCustomerProfileForLogin = 0)
	 BEGIN
		UPDATE dbo.[Logins] SET UseDefaultRateProfile = 0 WHERE ID = @LoginID
	 END

	 -- PDR Rate Profile tab
	 SELECT @countPdrRateProfileForLogin = (SELECT COUNT(*) FROM [dbo].[GetPdrRateProfileForLogin](@LoginID,NULL,0))
	 IF (@countPdrRateProfileForLogin = 1)
	 BEGIN
		UPDATE dbo.[Logins] SET UseDefaultPDRRateProfile = 1 WHERE ID = @LoginID
	 END
	ELSE IF (@countPdrRateProfileForLogin = 1)
	BEGIN
		UPDATE dbo.[Logins] SET UseDefaultPDRRateProfile = 0 WHERE ID = @LoginID
	END

	 -- ProAdvisor Profile tab
	 SELECT @countAddOnPresetProfileForLogin = (SELECT COUNT(*) FROM [dbo].[GetAddOnPresetProfileForLogin](@LoginID,0))
	 IF (@countAddOnPresetProfileForLogin = 0)
	 BEGIN
		UPDATE dbo.SiteSetting SET Value = 'True' WHERE Tag = 'UseDefaultAddOnProfile' AND LoginID = @LoginID
	 END

END 
GO
