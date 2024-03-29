USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/16/2018 
-- Description:	Get all PDR_EstimateDataPanelOversize records by id 
-- ============================================= 
CREATE PROCEDURE [dbo].[PDR_EstimateDataPanelOversize_GetByID] 
	@ID		int 
AS 
BEGIN 
	SELECT * 
	FROM PDR_EstimateDataPanelOversize 
	WHERE ID = @ID 
END 
GO
