USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[DeleteReport] 
	@ReportID	int
AS
BEGIN
	DELETE FROM Reports
	WHERE id = @ReportID
END


GO
