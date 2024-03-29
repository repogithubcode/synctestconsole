USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 7/12/2021
-- =============================================
CREATE PROCEDURE [dbo].[EmailConfirmLog_Insert]
	@LoginID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO EmailConfirmLog
	(LoginID, TimeStamp)
	VALUES
	(@LoginID, GETDATE())

	SELECT CAST(SCOPE_IDENTITY() AS INT)

END
GO
