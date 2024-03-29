USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/16/2021
-- =============================================
CREATE PROCEDURE [dbo].[SiteUsers_GetBySearch]
	    @Search					varchar(50)
	  , @IncludeDeleted			bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM SiteUsers
	WHERE 
		Name LIKE '%' + @Search + '%'
		AND (@IncludeDeleted = 1 OR SiteUsers.IsDeleted = 0)

	ORDER BY EmailAddress

END
GO
