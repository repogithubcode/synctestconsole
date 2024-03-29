USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/7/2017
-- Description:	Get a Logins record by ID
-- =============================================
CREATE PROCEDURE [dbo].[GetLogins]
	@loginsID	int
AS
BEGIN
	SET NOCOUNT ON;

	SELECT * FROM Logins WHERE id = @loginsID
END

GO
