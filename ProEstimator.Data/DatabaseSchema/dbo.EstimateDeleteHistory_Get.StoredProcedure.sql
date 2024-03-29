USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 9/13/2018 
-- Description:	Get delete history for an estimate.  
-- ============================================= 
CREATE PROCEDURE [dbo].[EstimateDeleteHistory_Get] 
	@AdminInfoID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    SELECT * 
	FROM EstimateDeleteHistory 
	WHERE AdminInfoID = @AdminInfoID 
	ORDER BY DeleteStamp DESC 
END 
GO
