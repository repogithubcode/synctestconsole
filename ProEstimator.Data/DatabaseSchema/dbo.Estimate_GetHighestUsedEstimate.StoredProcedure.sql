USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/17/2018 
-- Description:	Get the highest supplement level with data for an estimate.  This is used for the print page so we don't print empty supplements. 
-- ============================================= 
CREATE PROCEDURE [dbo].[Estimate_GetHighestUsedEstimate] 
	@AdminInfoID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	SELECT MAX(Supplement) 
	FROM 
	( 
		SELECT ISNULL(MAX(ISNULL(EstimationLineItems.SupplementVersion, 0)), 0) As Supplement 
		FROM EstimationData 
		JOIN EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID  
		WHERE EstimationData.AdminInfoID = @AdminInfoID   
 
		UNION  
 
		SELECT MAX(ISNULL(Sup.SupplementVersion, 0)) As Supplement 
		FROM PDR_EstimateDataPanel 
		LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange Sup ON Sup.EstimateDataPanelID = PDR_EstimateDataPanel.ID 
		WHERE PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID 
	) base 
END 
GO
