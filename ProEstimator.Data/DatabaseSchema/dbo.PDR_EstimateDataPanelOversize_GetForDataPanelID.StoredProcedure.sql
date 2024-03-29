USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/7/2018 
-- Description:	Get all PDR_EstimateDataPanelOversize records for a panel 
-- ============================================= 
CREATE PROCEDURE [dbo].[PDR_EstimateDataPanelOversize_GetForDataPanelID] 
	@EstimateDataPanelID		int 
AS 
BEGIN 
	SELECT * 
	FROM PDR_EstimateDataPanelOversize 
	WHERE EstimateDataPanelID = @EstimateDataPanelID 
END 
GO
