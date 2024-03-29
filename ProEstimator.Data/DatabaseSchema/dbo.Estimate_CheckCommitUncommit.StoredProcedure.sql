USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[Estimate_CheckCommitUncommit] 10097864

CREATE PROCEDURE [dbo].[Estimate_CheckCommitUncommit]
	@AdminInfoID		int 
AS 
BEGIN 
	 
	DECLARE @lineCount INT 
 
	SET @lineCount =  
	(	 
		SELECT Count(*)  
		FROM EstimationData  with(nolock) 
		INNER JOIN EstimationLineItems  with(nolock) ON EstimationLineItems.EstimationDataID = EstimationData.ID  
 
		WHERE AdminInfoID = @AdminInfoID AND  
			ISNULL(EstimationLineItems.SupplementVersion, 0) = ISNULL(EstimationData.LockLevel, 0) 
	) 
 
	SET @lineCount = @lineCount + 
	( 
		SELECT COUNT(*)
		FROM EstimationData
		JOIN PDR_EstimateDataPanel ON PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID 
		LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange change ON 
			change.EstimateDataPanelID = PDR_EstimateDataPanel.ID 
			AND change.SupplementVersion = ISNULL(EstimationData.LockLevel, 0) 
		LEFT OUTER JOIN PDR_EstimateDataPanelOversize oversize ON 
			PDR_EstimateDataPanel.ID = oversize.EstimateDataPanelID 
			AND (oversize.SupplementAdded = ISNULL(EstimationData.LockLevel, 0) OR oversize.SupplementDeleted = ISNULL(EstimationData.LockLevel, 0))
		WHERE  
			EstimationData.AdminInfoID = @AdminInfoID 
			AND (ISNULL(change.ID, 0) > 0 OR ISNULL(oversize.ID, 0) > 0) 
			AND   
			( 
				PDR_EstimateDataPanel.QuantityID > 0 
				OR PDR_EstimateDataPanel.OversizedDents > 0 
				OR ISNULL(PDR_EstimateDataPanel.CustomCharge, 0) <> 0
				OR ISNULL(change.QuantityID, 0) <> 0 
				OR ISNULL(change.OversizedDents, 0) <> 0 
				OR ISNULL(change.CustomCharge, 0) <> 0
			) 
	) 

	PRINT @lineCount

	IF @lineCount > 0  
	BEGIN  
		SELECT 1 -- 'Commit' 'ErrorReturn'
	END  
	ELSE  
	BEGIN  
			IF  
			( 
				SELECT ISNULL(LockLevel, 0)  
				FROM EstimationData    
				WHERE AdminInfoID = @AdminInfoID  
			) = 0 
		BEGIN
			SELECT 1 -- 'Commit' 'ErrorReturn'
		END
		ELSE
		BEGIN
			SELECT 2 -- 'Uncommit' 'ErrorReturn'
		END

	END   
 
END 
GO
