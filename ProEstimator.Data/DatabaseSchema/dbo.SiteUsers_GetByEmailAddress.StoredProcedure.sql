USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 7/15/2021
-- =============================================
CREATE PROCEDURE [dbo].[SiteUsers_GetByEmailAddress]
	    @EmailAddress					varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM SiteUsers
	WHERE EmailAddress = @EmailAddress

END
GO
