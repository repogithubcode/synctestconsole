USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/15/2018
-- Description:	Get a PDR Estimate Data record by ID
-- =============================================
CREATE PROCEDURE [dbo].[PDR_EstimateDataPanel_GetForEstimate]
	@AdminInfoID		int
	
AS
BEGIN
	SELECT PDR_EstimateDataPanel.*
	FROM PDR_EstimateDataPanel
	JOIN PDR_Panel ON PDR_EstimateDataPanel.PanelID = PDR_Panel.PanelID
	WHERE AdminInfoID = @AdminInfoID 
	ORDER BY PDR_Panel.SortOrder
END
GO
