USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/19/2017
-- Description:	Gets the most recent "Job Status History" record for the passed estimate
-- =============================================
CREATE PROCEDURE [dbo].[GetJobStatusCurrent]
	@estimateID		int
AS
BEGIN
	SELECT TOP 1 *
	FROM JobStatusHistory
	WHERE AdminInfoID = @estimateID
	ORDER BY ID DESC
END
GO
