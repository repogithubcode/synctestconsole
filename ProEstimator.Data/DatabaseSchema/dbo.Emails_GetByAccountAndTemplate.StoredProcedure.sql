USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/23/2023
-- =============================================
CREATE PROCEDURE [dbo].[Emails_GetByAccountAndTemplate]
	  @LoginID			int
	, @TemplateID		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *, '' As CompanyName
	FROM Emails
	WHERE LoginID = @LoginID AND TemplateID = @TemplateID

END
GO
