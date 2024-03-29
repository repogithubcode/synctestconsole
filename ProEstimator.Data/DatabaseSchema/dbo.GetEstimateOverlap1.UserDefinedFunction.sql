USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetEstimateOverlap1]  
(  
	@AdminInfoID		int  
)  
RETURNS @Overlap1 TABLE (AdminInfoID INT, EstimationLineItemsID1 INT, EstimationLineItemsID2 INT, OverlapAdjacentFlag VARCHAR(50), Minimum MONEY, Amount MONEY, SupplementVersion INT)  
AS  
BEGIN  
	 
	DECLARE @estimateID INT = @AdminInfoID 
	DECLARE @ServiceBarcode VARCHAR(10) = Mitchell3.dbo.GetServiceBarcode(@estimateID)  
  
	INSERT INTO @Overlap1     
	SELECT DISTINCT      
		@estimateID,   
		CASE	     
			WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID2      
			WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID2      
			WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID1      
			ELSE EstimationOverlap.EstimationLineItemsID1      
		END 'EstimationLineItemsID1',      
      
		Min(	     
			CASE	     
				WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID1      
				WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime THEN EstimationOverlap.EstimationLineItemsID1      
				WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime THEN EstimationOverlap.EstimationLineItemsID2      
				ELSE EstimationOverlap.EstimationLineItemsID2      
			END) 'EstimationLineItemsID2',      
		EstimationOverlap.OverlapAdjacentFlag 'OverlapAdjacentFlag',      
		Max(EstimationOverlap.Minimum) 'Minimum',      
		Sum(	     
			CASE 	     
				WHEN EstimationOverlap.Amount = 0 AND EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime THEN -EstimationLineLabor2.LaborTime      
				WHEN EstimationOverlap.Amount = 0 AND EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime THEN -EstimationLineLabor.LaborTime      
				ELSE EstimationOverlap.Amount      
			END) 'Amount', 
		EstimationOverlap.SupplementLevel  
	FROM EstimationData      
	INNER JOIN EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID 
	LEFT JOIN EstimationLineLabor ON  EstimationLineItems.ID  = EstimationLineLabor.EstimationLineItemsID AND EstimationLineLabor.LaborType IN (1,2,3,4,5,6,8,24,25)     
	LEFT JOIN EstimationLineItems EstimationLineItemsMod1 ON  EstimationData.ID = EstimationLineItemsMod1.EstimationDataID AND EstimationLineItems.ID = EstimationLineItemsMod1.ModifiesID     
	LEFT JOIN Mitchell3.dbo.Detail ON	     
	(     
		Detail.Service_BarCode = @ServiceBarcode AND      
		EstimationData.AdminInfoID = @estimateID AND      
		Detail.barcode = EstimationLineItems.Barcode AND      
		(      
			(Detail.Labor_Hours IS NULL AND EstimationLineLabor.LaborTime IS NULL)     
			OR       
			(Detail.Labor_Hours IS NOT NULL AND EstimationLineLabor.LaborTime IS NOT NULL AND ROUND(CONVERT(Real,ISNULL(Detail.Labor_Hours,0))/10,1) = ROUND(CONVERT(Real,ISNULL(EstimationLineLabor.LaborTime,0)),1) ))      
	)         
	INNER JOIN EstimationLineItems EstimationLineItems2 ON EstimationLineItems2.EstimationDataID  = EstimationData.ID   
	LEFT JOIN EstimationLineLabor EstimationLineLabor2 ON EstimationLineItems2.ID = EstimationLineLabor2.EstimationLineItemsID  AND EstimationLineLabor2.LaborType IN (1,2,3,4,5,6,8,24,25)     
	LEFT JOIN EstimationLineItems EstimationLineItemsMod2 ON  EstimationData.ID  = EstimationLineItemsMod2.EstimationDataID AND EstimationLineItemsMod2.ModifiesID = EstimationLineItems2.ID 
	LEFT JOIN Mitchell3.dbo.Detail Detail2 ON	     
	(     
		Detail2.Service_BarCode = @ServiceBarcode AND      
		Detail2.barcode = EstimationLineItems2.Barcode  AND      
		(      
			(Detail2.Labor_Hours IS NULL AND EstimationLineLabor2.LaborTime IS NULL)      
			OR       
		    (Detail2.Labor_Hours IS NOT NULL AND EstimationLineLabor2.LaborTime IS NOT NULL AND ROUND(CONVERT(Real,ISNULL(Detail2.Labor_Hours,0))/10,1) = ROUND(CONVERT(Real,ISNULL(EstimationLineLabor2.LaborTime,0)),1) )     
		)      
	)      
	INNER JOIN EstimationOverlap ON EstimationOverlap.EstimationLineItemsID1 = EstimationLineItems.id AND EstimationOverlap.EstimationLineItemsID2 = EstimationLineItems2.id    
	WHERE EstimationData.AdminInfoID = @estimateID AND      
		Detail.Service_BarCode = @ServiceBarcode AND      
		EstimationOverlap.UserAccepted <> 0 AND      
		EstimationLineItemsMod1.id IS NULL AND EstimationLineItemsMod2.id IS NULL AND      
		EstimationOverlap.OverlapAdjacentFlag = 'S'      
	GROUP BY      
		CASE	     
			WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID2      
			WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID2      
			WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID1      
			ELSE EstimationOverlap.EstimationLineItemsID1      
		END,      
		EstimationOverlap.OverlapAdjacentFlag, EstimationOverlap.SupplementLevel      
		      
	INSERT INTO @Overlap1     
		SELECT DISTINCT      
			@estimateID,    
			CASE	     
				WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID2      
				WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID2      
				WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID1      
				ELSE EstimationOverlap.EstimationLineItemsID1      
			END 'EstimationLineItemsID1',      
	      
			Min(	     
				CASE	     
					WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID1      
					WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime THEN EstimationOverlap.EstimationLineItemsID1      
					WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime THEN EstimationOverlap.EstimationLineItemsID2      
					ELSE EstimationOverlap.EstimationLineItemsID2      
				END) 'EstimationLineItemsID2',      
			EstimationOverlap.OverlapAdjacentFlag 'OverlapAdjacentFlag',      
			Max(EstimationOverlap.Minimum) 'Minimum',      
			Sum(	     
				CASE 	     
					WHEN EstimationOverlap.Amount = 0 AND EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime THEN -EstimationLineLabor2.LaborTime      
					WHEN EstimationOverlap.Amount = 0 AND EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime THEN -EstimationLineLabor.LaborTime      
					ELSE EstimationOverlap.Amount      
				END) 'Amount', 
			EstimationOverlap.SupplementLevel 
		FROM EstimationData     
		INNER JOIN EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID     
		LEFT JOIN EstimationLineLabor ON EstimationLineItems.ID = EstimationLineLabor.EstimationLineItemsID AND EstimationLineLabor.LaborType IN (1,2,3,4,5,6,8,24,25)     
	      
		LEFT JOIN Focuswrite.dbo.EstimationLineItems EstimationLineItemsMod1 ON EstimationData.ID = EstimationLineItemsMod1.EstimationDataID  AND EstimationLineItemsMod1.ModifiesID = EstimationLineItems.ID     
	      
		LEFT JOIN Mitchell3.dbo.Detail ON	      
		(     
			Detail.Service_BarCode = @ServiceBarcode AND      
			EstimationData.AdminInfoID = @estimateID AND      
			Detail.barcode = EstimationLineItems.Barcode AND      
			(      
				(Detail.Labor_Hours IS NULL AND EstimationLineLabor.LaborTime IS NULL)      
				OR       
				(Detail.Labor_Hours IS NOT NULL AND EstimationLineLabor.LaborTime IS NOT NULL AND  ROUND(CONVERT(Real,ISNULL(Detail.Labor_Hours,0))/10,1) = ROUND(CONVERT(Real,ISNULL(EstimationLineLabor.LaborTime,0)),1) )     
			)      
		)      
	      
		INNER JOIN EstimationLineItems EstimationLineItems2 ON EstimationData.ID   =    EstimationLineItems2.EstimationDataID 
		LEFT JOIN EstimationLineLabor EstimationLineLabor2 ON EstimationLineItems2.ID = EstimationLineLabor2.EstimationLineItemsID AND EstimationLineLabor2.LaborType IN (1,2,3,4,5,6,8,24,25)     
	      
		LEFT JOIN EstimationLineItems EstimationLineItemsMod2 ON EstimationData.ID = EstimationLineItemsMod2.EstimationDataID AND EstimationLineItemsMod2.ModifiesID = EstimationLineItems2.ID   
	      
		LEFT JOIN Mitchell3.dbo.Detail Detail2 ON	     
		(     
			Detail2.Service_BarCode = @ServiceBarcode AND      
			Detail2.barcode = EstimationLineItems2.Barcode  AND      
			(      
				(Detail2.Labor_Hours IS NULL AND EstimationLineLabor2.LaborTime IS NULL )      
				OR       
			    (Detail2.Labor_Hours IS NOT NULL AND EstimationLineLabor2.LaborTime IS NOT NULL AND ROUND(CONVERT(Real,ISNULL(Detail2.Labor_Hours,0))/10,1) = ROUND(CONVERT(Real,ISNULL(EstimationLineLabor2.LaborTime,0)),1) )     
			)      
		)      
	      
		INNER JOIN EstimationOverlap ON EstimationOverlap.EstimationLineItemsID1 = EstimationLineItems.id AND EstimationOverlap.EstimationLineItemsID2 = EstimationLineItems2.id      
		LEFT JOIN @Overlap1 AS Overlap1 ON Overlap1.AdminInfoID = @estimateID AND EstimationOverlap.EstimationLineItemsID1  =   Overlap1.EstimationLineItemsID1
		WHERE EstimationData.AdminInfoID = @estimateID AND      
			Detail.Service_BarCode = @ServiceBarcode AND      
			EstimationOverlap.UserAccepted <> 0 AND   
  
			-- 4/10/2018 - Ezra - Commented these out, why would modified line items ignore overlaps?  This was causing overlaps to dissapear in supplements.     
			--EstimationLineItemsMod1.id IS NULL AND EstimationLineItemsMod2.id IS NULL AND      
			--EO1.EstimationLineItemsID1 IS NULL AND      
		EstimationOverlap.OverlapAdjacentFlag <> 'S'      
		GROUP BY   
			CASE	  
				WHEN EstimationOverlap.OverlapAdjacentFlag = 'S' THEN EstimationOverlap.EstimationLineItemsID2      
				WHEN EstimationLineLabor2.LaborTime < EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID2      
				WHEN EstimationLineLabor2.LaborTime >= EstimationLineLabor.LaborTime AND EstimationOverlap.Amount < -80 THEN EstimationOverlap.EstimationLineItemsID1      
				ELSE EstimationOverlap.EstimationLineItemsID1      
			END,      
			EstimationOverlap.OverlapAdjacentFlag, EstimationOverlap.SupplementLevel      
	  
	RETURN   
END  
GO
