USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/14/2020
-- Description:	Get all QBExportEstimateLog records for an Export Log ID
-- =============================================
CREATE PROCEDURE [dbo].[QBExportEstimateLog_Get]
	@ExportLogID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM QBExportEstimateLog
	WHERE ExportLogID = @ExportLogID

END
GO
