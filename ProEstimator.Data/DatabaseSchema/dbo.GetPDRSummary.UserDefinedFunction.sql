USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetPDRSummary]  
(  
	@AdminInfoID		int,  
	@ForEOR				bit,
	@Supplement			int
)  
RETURNS @PDRSummary TABLE   
(  
	 AdminInfoID			int  
	, EstimateDataPanelID	int  
	, SupplementVersion		int  
	, LineNumber			int  
	, ForEOR				bit  
	, Panel					varchar(50)  
	, Description			varchar(200)  
	, Price					money  
	, OversizedDescription	varchar(200)  
	, OversizedPrice		money  
	, ModifierDescription	varchar(200)  
	, ModifierPrice			money  
	, Notes					varchar(300)  
	, Sorter				int  
	, GroupNumber			int  
)  
AS  
BEGIN  
 
	DECLARE @estimateID INT = @AdminInfoID 
	DECLARE @eor BIT = @ForEOR 
 
	DECLARE @Service_BarCode VarCHar(10)    
	SELECT @Service_BarCode = Mitchell3.dbo.GetServiceBarcode(@estimateID)   
	  
	DECLARE @OversizedSummary TABLE  
	(  
		EstimateDataPanelID		int,   
		Supplement				int,   
		SupplementDeleted		int, 
		Count					int,   
		Size					varchar(50),   
		Depth					varchar(50),   
		Total					money 				  
	)  
  
	INSERT INTO @OversizedSummary   
   
	-- Add oversized dents added   
	SELECT    
		  PDR_EstimateDataPanel.ID AS EstimateDataPanelID	   
		, PDR_EstimateDataPanelOversize.SupplementAdded AS Supplement   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, COUNT(*) AS Count   
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
		, SUM(Amount) AS Total   
	FROM PDR_EstimateDataPanel      with(nolock)
	JOIN PDR_EstimateDataPanelOversize  with(nolock) ON PDR_EstimateDataPanel.ID = PDR_EstimateDataPanelOversize.EstimateDataPanelID  
	JOIN PDR_SizeLookup  with(nolock) ON PDR_EstimateDataPanelOversize.Size = PDR_SizeLookup.ID 
	JOIN PDR_DepthLookup  with(nolock) ON PDR_EstimateDataPanelOversize.Depth = PDR_DepthLookup.ID  
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @estimateID   
		--AND PDR_EstimateDataPanelOversize.SupplementDeleted = 0 
		--AND (PDR_EstimateDataPanelOversize.SupplementDeleted <> PDR_EstimateDataPanelOversize.SupplementAdded OR PDR_EstimateDataPanelOversize.SupplementDeleted = 0)   
	GROUP BY    
		  PDR_EstimateDataPanel.ID   
		, PDR_EstimateDataPanelOversize.SupplementAdded   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
   
	UNION   
   
	---- Add lines for oversized dents that were removed   
	--SELECT    
	--	  PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
	--	, PDR_EstimateDataPanelOversize.SupplementDeleted AS Supplement   
	--	, COUNT(*) AS Count   
	--	, PDR_SizeLookup.Size   
	--	, PDR_DepthLookup.Depth   
	--	, SUM(Amount) * -1 AS Total   
	--FROM PDR_EstimateDataPanel    
	--JOIN PDR_EstimateDataPanelOversize ON PDR_EstimateDataPanelOversize.EstimateDataPanelID = PDR_EstimateDataPanel.ID   
	--JOIN PDR_SizeLookup ON PDR_SizeLookup.ID = PDR_EstimateDataPanelOversize.Size   
	--JOIN PDR_DepthLookup ON PDR_DepthLookup.ID = PDR_EstimateDataPanelOversize.Depth   
	--WHERE    
	--	PDR_EstimateDataPanel.AdminInfoID = @estimateID   
	--	AND PDR_EstimateDataPanelOversize.SupplementDeleted > 0   
	--	AND PDR_EstimateDataPanelOversize.SupplementDeleted <> PDR_EstimateDataPanelOversize.SupplementAdded    
	--GROUP BY    
	--	PDR_EstimateDataPanel.ID   
	--	, PDR_EstimateDataPanelOversize.SupplementDeleted   
	--	, PDR_SizeLookup.Size   
 --		, PDR_DepthLookup.Depth   
   
	-- Add generic oversized dents and their supplements   
	--UNION    
   
	SELECT    
		PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
		, 0 AS Supplement   
		, 0 
		, PDR_EstimateDataPanel.OversizedDents AS Count   
		, 'Oversized' As Size   
		, '' AS Depth   
		, PDR_Rate.Amount * PDR_EstimateDataPanel.OversizedDents AS Total   
	FROM PDR_EstimateDataPanel    with(nolock)
	LEFT OUTER JOIN PDR_RateProfile  with(nolock) ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID AND PDR_RateProfile.Deleted = 0
	LEFT OUTER JOIN PDR_Rate  with(nolock) ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_EstimateDataPanel.PanelID = PDR_Rate.PanelID AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @estimateID   
		AND PDR_EstimateDataPanel.OversizedDents <> 0   
   
	UNION   
   
	SELECT    
		PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
		, Sup.SupplementVersion AS Supplement   
		, 0 
		, Sup.OversizedDents AS Count   
		, 'Oversized' As Size   
		, '' AS Depth   
		, PDR_Rate.Amount * Sup.OversizedDents AS Total   
	FROM PDR_EstimateDataPanel    with(nolock) 
	JOIN PDR_EstimateDataPanelSupplementChange Sup  with(nolock) ON PDR_EstimateDataPanel.ID = Sup.EstimateDataPanelID  
	LEFT OUTER JOIN PDR_RateProfile  with(nolock) ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID  AND PDR_RateProfile.Deleted = 0   
	LEFT OUTER JOIN PDR_Rate  with(nolock) ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_EstimateDataPanel.PanelID = PDR_Rate.PanelID AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @estimateID   
		AND Sup.OversizedDents <> 0   
   
	--SELECT * FROM #OversizedSummary   
	-- End of getting #OversizedSummary   
	---------------------------------------------------------------------------------------------------------------------------------   
	   
	   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create another temporary table to add final result lines to with data from the #OversizedSummary made above   
	-- There will be 1 line for each data panel   
	---------------------------------------------------------------------------------------------------------------------------------   
	DECLARE @OversizedResults TABLE  
	(  
		EstimateDataPanelID		int,   
		SupplementVersion		int,   
		Description				varchar(300),   
		Total					money 				  
	)  
	-- Loop through each data panel ID in the summary table and add a line to the results table   
	DECLARE @cursor CURSOR	   
	SET @cursor = CURSOR FOR   
		SELECT DISTINCT EstimateDataPanelID, dbo.Biggest(Supplement, SupplementDeleted) As Supplement 
		FROM @OversizedSummary   
   
	DECLARE @dataPanelID INT   
	DECLARE @supplementVersion INT   
   
	OPEN @cursor   
	FETCH NEXT FROM @cursor   
	INTO @dataPanelID, @supplementVersion   
   
	WHILE @@FETCH_STATUS = 0   
		BEGIN   
   
			DECLARE @Summary VARCHAR(200) = ''   
			DECLARE @Total MONEY = 0   
   
			SELECT    
				  @Summary = @Summary + CAST(Count AS VARCHAR) + ' x ' + Size + ' ' + Depth + ', '   
				, @Total = @Total + Total   
				FROM @OversizedSummary   
				WHERE    
					EstimateDataPanelID = @dataPanelID 
					AND Supplement <= @SupplementVersion   
					AND (SupplementDeleted > @SupplementVersion OR SupplementDeleted = 0) 
					AND (Supplement = (SELECT MAX(Supplement) FROM @OversizedSummary WHERE EstimateDataPanelID = @dataPanelID AND Size = 'Oversized' AND Supplement <= @supplementVersion) OR Size <> 'Oversized')	 
   
			INSERT INTO @OversizedResults    
			(   
				  EstimateDataPanelID   
				, SupplementVersion   
				, Description   
				, Total   
			)   
			VALUES   
			(   
				  @dataPanelID   
				, @supplementVersion   
				, SUBSTRING(@Summary, 0, LEN(@Summary))   
				, @Total   
			)   
   
			FETCH NEXT FROM @cursor   
			INTO @dataPanelID, @supplementVersion   
		END   
   
	--SELECT * FROM @OversizedResults   
   
	-- End of creating #OversizedResults   
	---------------------------------------------------------------------------------------------------------------------------------   
   
   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create a table to replace the PDR_EstimateDataPanel table that joins in data from the SupplementChange table so that we have    
	-- a record for the base supplement as well as the supplements.   
   
	DECLARE @PanelData TABLE  
	(  
		EstimateDataPanelID		int,   
		PanelID					int,   
		QuantityID				int,   
		SizeID					int,   
		OversizedDents			int,   
		Multiplier				int,   
		CustomCharge			money,
		SupplementVersion		int,   
		LineNumber				int,   
		Notes					varchar(300) 				  
	)  
   
	INSERT INTO @PanelData   
  
	SELECT DISTINCT * FROM (   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, 0 As SupplementVersion, base.LineNumber, base.Description    
		FROM PDR_EstimateDataPanel base   
		LEFT OUTER JOIN @OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND OversizedResults.SupplementVersion = 0
		WHERE AdminInfoID = @estimateID   
			AND (base.QuantityID > 0 OR base.SizeID > 0 OR base.OversizedDents > 0 OR OversizedResults.EstimateDataPanelID IS NOT NULL OR ISNULL(CustomCharge, 0) <> 0)   
   
		UNION    
   
		SELECT base.ID, base.PanelID, Sup.QuantityID, Sup.SizeID, Sup.OversizedDents, Sup.Multiplier, Sup.CustomCharge, Sup.SupplementVersion, Sup.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelSupplementChange Sup ON base.ID = Sup.EstimateDataPanelID   
		LEFT OUTER JOIN @OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND Sup.SupplementVersion = OversizedResults.SupplementVersion  
		WHERE base.AdminInfoID = @estimateID 		  
   
	) AS panelData   
  
	-- Add any EstimateDataPanels who haven't been changed this supplement but have an oversize change   
	INSERT INTO @PanelData  
  
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, ISNULL(sup.OversizedDents, base.OversizedDents) AS OversizedDents, ISNULL(sup.Multiplier, base.Multiplier) AS Multiplier, ISNULL(sup.CustomCharge, base.CustomCharge), oversize.SupplementAdded As SupplementVersion, dbo.Biggest(ISNULL(sup.LineNumber, 0), base.LineNumber) AS LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID    
		LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange sup ON base.ID = sup.EstimateDataPanelID AND sup.SupplementVersion = (SELECT MAX(SupplementVersion) FROM PDR_EstimateDataPanelSupplementChange WHERE EstimateDataPanelID = base.ID) 
		WHERE base.AdminInfoID = @estimateID AND oversize.SupplementAdded > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
		UNION   
   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, oversize.SupplementDeleted As SupplementVersion, base.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID  
		WHERE base.AdminInfoID = @estimateID AND oversize.SupplementDeleted > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
    	DELETE FROM @PanelData WHERE SupplementVersion > @Supplement 

	--DROP TABLE #SupplementVersions   

	--SELECT * FROM @PanelData
	---------------------------------------------------------------------------------------------------------------------------------   
	  
	INSERT INTO @PDRSummary   
   
	SELECT -- DISTINCT  *
		DISTINCT    
			  @estimateID  
			, PanelData.EstimateDataPanelID As ID   
			, PanelData.SupplementVersion   
			--, (SELECT MAX(LineNumber) FROM @PanelData WHERE EstimateDataPanelID = PanelData.EstimateDataPanelID) AS LineNumber 
			, LineNumber
			, CASE    
				WHEN PanelData.SupplementVersion = 
				dbo.Smallest(
				(   
					SELECT MAX(SupplementVersion)    
					FROM @PanelData panelDataSub    
					WHERE panelDataSub.EstimateDataPanelID = PanelData.EstimateDataPanelID   
				), @Supplement)
				AND (PDR_Rate.Amount IS NOT NULL OR ISNULL(OversizedResults.Total, 0) <> 0 OR ISNULL(CustomCharge, 0) <> 0) THEN 1 ELSE 0 END As ForEOR    
			, ISNULL(PDR_CategoryMap.CategoryName, UPPER(PDR_Panel.PanelName)) AS panel   
			, PDR_Panel.PanelName + ' Dents '  
			  + CASE 
					WHEN PanelData.CustomCharge <> 0 THEN '- Custom Charge'
				ELSE  
					CASE   
						WHEN PDR_Rate.ID IS NULL   
							THEN CASE WHEN ISNULL(OversizedResults.Total, 0) <> 0 THEN '' ELSE 'DELETED' END  
						ELSE   
							CASE WHEN ISNULL(PDR_RateProfile.HideDentCounts, 0) = 0   
								THEN CAST(PDR_QuantityLookup.Min AS VARCHAR) + ' - ' + CAST(PDR_QuantityLookup.Max AS VARCHAR)    
							ELSE ' -'   
							END   
						+ ' ' + PDR_SizeLookup.Size   
					END 
			  END AS Description   
			, CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END AS Price   
			, ISNULL(OversizedResults.Description, '') As OversizedDescription   
			, ISNULL(OversizedResults.Total, 0) As OversizedPrice   
			, CASE   
				 WHEN ISNULL(PDR_Multiplier.ID, 0) > 0   
					THEN CAST(ISNULL(PDR_Multiplier.Value, 0) AS VARCHAR(50)) + '% ' + PDR_Multiplier.Name + ' Modifier'    
					ELSE ''    
				END AS ModifierDescription   
			, CASE WHEN ISNULL(PDR_Multiplier.ID, 0) = 0 THEN 0 ELSE CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END * (PDR_Multiplier.Value / 100) END AS ModifierPrice   
			, PanelData.Notes   
			, (ISNULL(Category.nheader, 1000) * 256) + PDR_Panel.SortOrder AS Sorter		   
			, (ISNULL(Category.nheader, 1000) * 256) + PDR_Panel.SortOrder AS GroupNumber   
		FROM PDR_EstimateData   
   
		-- Join in PDR panels with selections   
		JOIN @PanelData AS PanelData ON 1 = 1   
   
		-- Join the rate profile data   
		JOIN PDR_RateProfile ON PDR_RateProfile.ID = PDR_EstimateData.RateProfileID  AND PDR_RateProfile.Deleted = 0
		LEFT OUTER JOIN PDR_Rate ON    
			PDR_Rate.RateProfileID = PDR_RateProfile.ID    
			AND PanelData.SizeID = PDR_Rate.Size   
			AND PanelData.PanelID = PDR_Rate.PanelID  
			AND PanelData.QuantityID = PDR_Rate.Quantity   
   
		LEFT OUTER JOIN PDR_EstimateDataPanelOversize ON PanelData.EstimateDataPanelID = PDR_EstimateDataPanelOversize.EstimateDataPanelID    
   
		JOIN PDR_Panel ON PanelData.PanelID = PDR_Panel.PanelID   
   
		LEFT OUTER JOIN PDR_CategoryMap ON PDR_Panel.PanelID = PDR_CategoryMap.PanelID   
		LEFT OUTER JOIN Mitchell3.dbo.Category ON Category.Service_Barcode = @Service_BarCode AND PDR_CategoryMap.CategoryName = Category.Category   
   
		LEFT OUTER JOIN PDR_SizeLookup ON PanelData.SizeID = PDR_SizeLookup.ID   
		LEFT OUTER JOIN PDR_QuantityLookup ON PanelData.QuantityID = PDR_QuantityLookup.ID   
		LEFT OUTER JOIN PDR_Multiplier ON PanelData.Multiplier = PDR_Multiplier.ID   
   
		LEFT OUTER JOIN @OversizedResults AS OversizedResults ON OversizedResults.EstimateDataPanelID = PanelData.EstimateDataPanelID AND OversizedResults.SupplementVersion = PanelData.SupplementVersion   
   
		WHERE    
			PDR_EstimateData.AdminInfoID = @estimateID   
			AND PanelData.SupplementVersion <= @Supplement
			--AND    
			--(   
			--	PDR_Rate.Amount * CASE WHEN ISNULL(PDR_Multiplier.Value, 0) = 0 THEN 1 ELSE PDR_Multiplier.Value / 100 END > 0   
			--)   
  
  
	RETURN   
END  
GO
