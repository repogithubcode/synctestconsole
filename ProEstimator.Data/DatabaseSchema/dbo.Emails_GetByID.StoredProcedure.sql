USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- =============================================
CREATE PROCEDURE [dbo].[Emails_GetByID]
	@id				int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *, '' AS CompanyName
	FROM Emails
	WHERE ID = @id

END
GO
