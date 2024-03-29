USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Estimate_GetLineItemsPreview]     
	@AdminInfoID		int 
AS    
BEGIN    
 
	DECLARE @IDLocked INT
	DECLARE @SuppVersion INT
	
	SELECT 
		  @IDLocked = ISNULL(EstimationData.EstimationLineItemIDLocked, -1)
		, @SuppVersion = ISNULL(LockLevel, 0)
	FROM EstimationData 
	WHERE AdminInfoID = @AdminInfoID 
    
	DECLARE @Service_BarCode VarCHar(11)     
	SELECT @Service_BarCode = Mitchell3.dbo.GetServiceBarcode(@AdminInfoID) 
	

	SELECT DISTINCT    
		CASE     
			WHEN ISNULL(ProcessedLines.LaborIncluded, 0) = 1 THEN 'Included, '    
			ELSE     
				CASE WHEN ISNULL(ProcessedLines.LaborTotalExtra, 0) <> 0     
					THEN 
						CASE WHEN NOT ProcessedLines.ExtraLaborIncludeIn IS NULL 
							-- If the extra labor is "included" in another labor type, the summary shows the other type.  In the line item preview we want to show the original  
							-- type so do a string replace 
							THEN REPLACE(ISNULL(ProcessedLines.LaborSummaryExtraPrintDescSubText, ''), ProcessedLines.ExtraLaborIncludeIn, ProcessedLines.ExtraLaborType)  +    
								CASE WHEN ISNULL(ProcessedLines.LaborQuantity, 0) > 1 
								THEN (CASE WHEN ProcessedLines.IsLaborQuantity = 1 THEN ' x ' + CAST(ProcessedLines.LaborQuantity AS VARCHAR(10)) + ', ' ELSE '' END) 
								ELSE '' END

							ELSE CASE WHEN ProcessedLines.LaborSummaryExtraPrintDescSubText IS NOT NULL THEN     
								ProcessedLines.LaborSummaryExtraPrintDescSubText + CASE WHEN ISNULL(ProcessedLines.LaborQuantity, 0) > 1
								THEN (CASE WHEN ProcessedLines.IsLaborQuantity = 1 THEN ' x ' + CAST(ProcessedLines.LaborQuantity AS VARCHAR(10)) + ', ' ELSE '' END) 
								ELSE '' END 
							ELSE '' END
						END 
						 +     
						CASE WHEN ISNULL(ProcessedLines.LaborSummaryExtraPrintDescSubText, '') <> '' THEN ' ' ELSE '' END     
					ELSE ''    
				END
		END    
		+     
			CASE   
				WHEN ISNULL(ProcessedLines.LaborTotalPaintPanel, 0) <> 0     
					THEN CAST(ISNULL(ProcessedLines.LaborSummaryPaintPanelPrintDescSubText, '') AS VARCHAR(200)) + CASE WHEN ISNULL(ProcessedLines.PaintQuantity, 0) > 1   
					THEN (CASE WHEN ProcessedLines.IsPaintQuantity = 1 THEN ' x ' + CAST(ProcessedLines.PaintQuantity AS VARCHAR(10)) ELSE '' END) 
					ELSE '' END 
				+ CASE WHEN ISNULL(ProcessedLines.LaborSummaryPaintPanelPrintDescSubText, '') <> '' THEN ', ' ELSE '' END    
				ELSE ''     
			END
		+     
			CASE   
				WHEN ISNULL(ProcessedLines.LaborTotalPaint, 0) <> 0 OR ISNULL(ProcessedLines.LaborTotalClearcoat, 0) <> 0   
					THEN CAST(ISNULL(ProcessedLines.LaborSummaryPaintH, '') AS VARCHAR(200)) +  CASE WHEN ISNULL(ProcessedLines.PaintQuantity, 0) > 1 
					THEN (CASE WHEN ProcessedLines.IsPaintQuantity = 1 THEN ' x ' + CAST(ProcessedLines.PaintQuantity AS VARCHAR(10)) + ', ' ELSE '' END) 
					ELSE '' END + CASE WHEN ISNULL(ProcessedLines.LaborSummaryPaintH, '') <> '' THEN ', ' ELSE '' END    
				ELSE ''    
			END    
		+ CASE WHEN ISNULL(ProcessedLines.OtherChargesPreview, 0) <> 0
			THEN '$' + CAST(ProcessedLines.OtherChargesPreview AS VARCHAR)  +  ' ' + ISNULL(LaborTypes.LaborType, '') +
			(CASE WHEN ProcessedLines.IsOtherChargesQuantity = 1 AND ProcessedLines.OtherChargesQuantity > 1 THEN ' x ' + CAST(ProcessedLines.OtherChargesQuantity AS VARCHAR(10)) ELSE '' END) 
			ELSE '' END    

		+ CASE WHEN ISNULL(ProcessedLines.AdjacentMessage, '') <> '' THEN ' ' + ProcessedLines.AdjacentMessage ELSE '' END   
		AS LaborItems,    
    
		REPLACE(ProcessedLines.Panel, 'ZZZ', '') AS Step, 
		ProcessedLines.OperationDescription AS ActionDescription,    
		ProcessedLines.Action AS ActionCode,    
		ProcessedLines.PartNumber,    
		'$' + CAST(    
			ROUND(    
				  ISNULL(ProcessedLines.PricePreview, 0)     
				+ ISNULL(ProcessedLines.OversizedPrice, 0)     
				+ ISNULL(ProcessedLines.ModifierPrice, 0)    
			, 2)     
		AS Varchar) AS Price,    
		ProcessedLines.PartSource,    
		ProcessedLines.Description + 
		(
			CASE WHEN ISNULL(ProcessedLines.Quantity, 0) > 1 
				THEN (CASE WHEN ProcessedLines.IsPartsQuantity = 1 THEN ' x ' + CAST(ProcessedLines.Quantity AS VARCHAR(10)) ELSE '' END) 
			ELSE '' END
		) 
		AS PartDescription,       
		CASE WHEN EstimationLineItems.PartOfOverHaul <> 0 THEN 'Yes' ELSE 'No' END 'PartOfOverHaul',    
		ProcessedLines.LineItemID AS LineID,    
		ProcessedLines.LineNumber,    
		CASE     
			WHEN     
				EstimationLineItems.ID <= @IDLocked     
				OR (ProcessedLines.Action = 'PDR Matrix' AND ProcessedLines.SupplementVersion < @SuppVersion)    
				THEN 1    
			ELSE 0    
		END 'Locked',    
		CASE 	    
			WHEN ISNULL(EstimationLineItemsModified.LineNumber, -1) <> EstimationLineItems.LineNumber THEN ISNULL(EstimationLineItemsModified.LineNumber, -1)    
			WHEN ISNULL(ProcessedLinesModified.LineNumber, 0) > 0 THEN ProcessedLinesModified.LineNumber    
			ELSE -1    
		END 'Modified',
		@SuppVersion AS EstimationDataSuppVer, ProcessedLines.SupplementVersion AS ProcessedLineSuppVer
	FROM ProcessedLines  
    
	LEFT OUTER JOIN EstimationLineItems with(nolock) ON ProcessedLines.LineItemID = EstimationLineItems.ID 
	LEFT OUTER JOIN EstimationLineItems AS EstimationLineItemsModified with(nolock) ON EstimationLineItemsModified.ID = Focuswrite.Dbo.GetLatestLineItemMod(EstimationLineItems.ID)    
    
	LEFT OUTER JOIN ProcessedLines AS ProcessedLinesModified  with(nolock)    
		ON ProcessedLines.LineItemID = ProcessedLinesModified.LineItemID
		AND ProcessedLinesModified.SupplementVersion = ProcessedLines.SupplementVersion + 1 
		AND ProcessedLinesModified.Action = 'PDR Matrix' 
		AND ProcessedLines.Action = 'PDR Matrix'  
    
	LEFT JOIN LaborTypes ON ProcessedLines.OtherChargesLaborType = LaborTypes.id 
    
	WHERE    
		ProcessedLines.EstimateID = @AdminInfoID
		AND ProcessedLines.Supplement = @SuppVersion
		AND
		(
			ProcessedLines.Action <> 'PDR Matrix'  
			OR 
			(
				ProcessedLines.Action = 'PDR Matrix' 
				AND (ProcessedLines.ForSummary = 1 OR ProcessedLines.Removed = 1 OR ProcessedLines.Added = 1)
			)    
		)
    
	ORDER BY LineNumber DESC    
    
END
GO
