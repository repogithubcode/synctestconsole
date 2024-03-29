USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/13/2017
-- Description:	Get a Logins record by name and organization.  This is replacing the old sp_CheckLogins which had a lot of extra stuff that isn't used any more
-- =============================================
CREATE PROCEDURE [dbo].[GetLoginsByName]
	@loginName	varchar(25)
	,@organization varchar(50)
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * 
	FROM Logins 
	WHERE LoginName = @loginName AND Organization = @organization
END
GO
