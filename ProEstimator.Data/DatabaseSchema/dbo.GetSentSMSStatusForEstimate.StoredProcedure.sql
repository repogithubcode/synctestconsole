USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/3/2017 
-- Description:	Get all SentSMSStatus records for an estimate 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetSentSMSStatusForEstimate] 
	@EstimateID		int 
AS 
BEGIN 
	SELECT SentSMS_Status.* 
	FROM SentEstimate 
	JOIN SentSMS_Status ON SentSMS_Status.ReportID = SentEstimate.ReportID 
	WHERE AdminInfoID = @EstimateID 
END 
GO
