USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 6/23/22
-- Description:	For the PDR Work Order report
--				NOTE!  Most of this is copied from FillProcessedLines.  If that file changes this must be updated too!
-- =============================================
CREATE PROCEDURE [dbo].[PDR_WorkOrderReport]
	@AdminInfoID		INT,
	@SupplementVersion	INT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.       
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component       
	 IF 1=0 BEGIN       
	  SET FMTONLY OFF       
	 END  

	-----------------------------------------------------------------------------------------------------------------------------------------
	-- The following is copied from FillProcessedLines.  If anything there changes, change it here to.  
	-----------------------------------------------------------------------------------------------------------------------------------------
	-- Begin copy from GetProcessedLines
	-----------------------------------------------------------------------------------------------------------------------------------------

	-------------------------------------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------------------------
	-- PDR Summary
	-------------------------------------------------------------------------------------------------------------------------------------
	-------------------------------------------------------------------------------------------------------------------------------------

	CREATE TABLE #OversizedSummary  
	(  
		EstimateDataPanelID		int,   
		Supplement				int,   
		SupplementDeleted		int, 
		Count					int,   
		Size					varchar(50),   
		Depth					varchar(50),   
		Total					money 				  
	)  
  
	INSERT INTO #OversizedSummary   
   
	-- Add oversized dents added   
	SELECT    
			PDR_EstimateDataPanel.ID AS EstimateDataPanelID	   
		, PDR_EstimateDataPanelOversize.SupplementAdded AS Supplement   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, COUNT(*) AS Count   
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
		, SUM(Amount) AS Total   
	FROM PDR_EstimateDataPanel     
	JOIN PDR_EstimateDataPanelOversize ON PDR_EstimateDataPanel.ID = PDR_EstimateDataPanelOversize.EstimateDataPanelID  
	JOIN PDR_SizeLookup ON PDR_EstimateDataPanelOversize.Size = PDR_SizeLookup.ID 
	JOIN PDR_DepthLookup ON PDR_EstimateDataPanelOversize.Depth = PDR_DepthLookup.ID  
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
		--AND PDR_EstimateDataPanelOversize.SupplementDeleted = 0 
		--AND (PDR_EstimateDataPanelOversize.SupplementDeleted <> PDR_EstimateDataPanelOversize.SupplementAdded OR PDR_EstimateDataPanelOversize.SupplementDeleted = 0)   
	GROUP BY    
			PDR_EstimateDataPanel.ID   
		, PDR_EstimateDataPanelOversize.SupplementAdded   
		, PDR_EstimateDataPanelOversize.SupplementDeleted 
		, PDR_SizeLookup.Size   
		, PDR_DepthLookup.Depth   
   
	UNION   
   
	SELECT    
		PDR_EstimateDataPanel.ID AS EstimateDataPanelID   
		, 0 AS Supplement   
		, 0 
		, PDR_EstimateDataPanel.OversizedDents AS Count   
		, 'Oversized' As Size   
		, '' AS Depth   
		, PDR_Rate.Amount * PDR_EstimateDataPanel.OversizedDents AS Total   
	FROM PDR_EstimateDataPanel   
	LEFT OUTER JOIN PDR_RateProfile ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID AND PDR_RateProfile.Deleted = 0
	LEFT OUTER JOIN PDR_Rate ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_Rate.PanelID = 1 AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
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
	FROM PDR_EstimateDataPanel   
	JOIN PDR_EstimateDataPanelSupplementChange Sup ON PDR_EstimateDataPanel.ID = Sup.EstimateDataPanelID  
	LEFT OUTER JOIN PDR_RateProfile ON PDR_EstimateDataPanel.AdminInfoID = PDR_RateProfile.AdminInfoID  AND PDR_RateProfile.Deleted = 0   
	LEFT OUTER JOIN PDR_Rate ON PDR_RateProfile.ID = PDR_Rate.RateProfileID AND PDR_EstimateDataPanel.PanelID = PDR_Rate.PanelID AND PDR_Rate.Size = 9   
	WHERE    
		PDR_EstimateDataPanel.AdminInfoID = @AdminInfoID   
		AND Sup.OversizedDents <> 0   
   
	-- SELECT * FROM #OversizedSummary   
	-- End of getting #OversizedSummary   
	---------------------------------------------------------------------------------------------------------------------------------   
	   
	   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create another temporary table to add final result lines to with data from the #OversizedSummary made above   
	-- There will be 1 line for each data panel   
	---------------------------------------------------------------------------------------------------------------------------------   
	CREATE TABLE #OversizedResults  
	(  
		EstimateDataPanelID		int,   
		SupplementVersion		int,   
		Description				varchar(300),   
		Total					money 				  
	)  

	-- Loop through each data panel ID in the summary table and add a line to the results table   
	DECLARE @PDRcursor CURSOR	   
	SET @PDRcursor = CURSOR FOR   
		SELECT DISTINCT EstimateDataPanelID, dbo.Biggest(Supplement, SupplementDeleted) As Supplement 
		FROM #OversizedSummary   
   
	DECLARE @dataPanelID INT   
	DECLARE @suppVersion INT

	OPEN @PDRcursor   
	FETCH NEXT FROM @PDRcursor   
	INTO @dataPanelID, @suppVersion   
   
	WHILE @@FETCH_STATUS = 0   
		BEGIN   
   
			DECLARE @Summary VARCHAR(200) = ''   
			DECLARE @Total MONEY = 0   
   
			SELECT    
				@Summary = @Summary + CAST(Count AS VARCHAR) + ' x ' + Size + ' ' + Depth + ', '   
			, @Total = @Total + Total   
			FROM #OversizedSummary   
			WHERE    
				EstimateDataPanelID = @dataPanelID 
				AND Supplement <= @suppVersion   
				AND (SupplementDeleted > @suppVersion OR SupplementDeleted = 0) 
				AND (Supplement = (SELECT MAX(Supplement) FROM #OversizedSummary WHERE EstimateDataPanelID = @dataPanelID AND Size = 'Oversized' AND Supplement <= @suppVersion) OR Size <> 'Oversized')	 
   
			INSERT INTO #OversizedResults    
			(   
					EstimateDataPanelID   
				, SupplementVersion   
				, Description   
				, Total   
			)   
			VALUES   
			(   
					@dataPanelID   
				, @suppVersion   
				, SUBSTRING(@Summary, 0, LEN(@Summary))   
				, @Total   
			)   
   
			FETCH NEXT FROM @PDRcursor   
			INTO @dataPanelID, @suppVersion   
		END     
   
    -- If a panel has a supplement, it will be in the list twice.  Delete all but the most recent supplement, leaving one row per panel.
    DELETE #OversizedResults
	FROM #OversizedResults
	LEFT OUTER JOIN
	(
		-- Get the highest supplement for the panel
		SELECT EstimateDataPanelID, MAX(SupplementVersion) AS MaxSupplementVersion
		FROM #OversizedResults
		GROUP BY EstimateDataPanelID
	) AS MaxSuppVersion ON #OversizedResults.EstimateDataPanelID = MaxSuppVersion.EstimateDataPanelID
	WHERE #OversizedResults.SupplementVersion < MaxSuppVersion.MaxSupplementVersion

	-- SELECT * FROM #OversizedResults   
   
	-- End of creating #OversizedResults   
	---------------------------------------------------------------------------------------------------------------------------------   
   
   
	---------------------------------------------------------------------------------------------------------------------------------   
	-- Create a table to replace the PDR_EstimateDataPanel table that joins in data from the SupplementChange table so that we have    
	-- a record for the base supplement as well as the supplements.   
   
	CREATE TABLE #PanelData  
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
   
	INSERT INTO #PanelData   
  
	SELECT DISTINCT * FROM (   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, 0 As SupplementVersion, base.LineNumber, base.Description    
		FROM PDR_EstimateDataPanel base   
		LEFT OUTER JOIN #OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND OversizedResults.SupplementVersion = 0
		WHERE AdminInfoID = @AdminInfoID   
			AND (base.QuantityID > 0 OR base.SizeID > 0 OR base.OversizedDents > 0 OR OversizedResults.EstimateDataPanelID IS NOT NULL OR ISNULL(CustomCharge, 0) <> 0)   
   
		UNION    
   
		SELECT base.ID, base.PanelID, Sup.QuantityID, Sup.SizeID, Sup.OversizedDents, Sup.Multiplier, Sup.CustomCharge, Sup.SupplementVersion, Sup.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelSupplementChange Sup ON base.ID = Sup.EstimateDataPanelID   
		LEFT OUTER JOIN #OversizedResults AS OversizedResults ON base.ID = OversizedResults.EstimateDataPanelID AND Sup.SupplementVersion = OversizedResults.SupplementVersion  
		WHERE base.AdminInfoID = @AdminInfoID 		  
   
	) AS panelData   
  
	-- Add any EstimateDataPanels who haven't been changed this supplement but have an oversize change   
	INSERT INTO #PanelData  
  
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, ISNULL(sup.OversizedDents, base.OversizedDents) AS OversizedDents, ISNULL(sup.Multiplier, base.Multiplier) AS Multiplier, ISNULL(sup.CustomCharge, base.CustomCharge), oversize.SupplementAdded As SupplementVersion, dbo.Biggest(ISNULL(sup.LineNumber, 0), base.LineNumber) AS LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID    
		LEFT OUTER JOIN PDR_EstimateDataPanelSupplementChange sup ON base.ID = sup.EstimateDataPanelID AND sup.SupplementVersion = (SELECT MAX(SupplementVersion) FROM PDR_EstimateDataPanelSupplementChange WHERE EstimateDataPanelID = base.ID) 
		LEFT OUTER JOIN #PanelData AS PanelData ON base.PanelID = PanelData.PanelID
		WHERE base.AdminInfoID = @AdminInfoID AND PanelData.PanelID IS NULL AND oversize.SupplementAdded > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData)  
   
		UNION   
   
		SELECT base.ID, base.PanelID, base.QuantityID, base.SizeID, base.OversizedDents, base.Multiplier, base.CustomCharge, oversize.SupplementDeleted As SupplementVersion, base.LineNumber, base.Description   
		FROM PDR_EstimateDataPanel base   
		JOIN PDR_EstimateDataPanelOversize oversize ON base.id = oversize.EstimateDataPanelID  
		LEFT OUTER JOIN #PanelData AS PanelData ON base.PanelID = PanelData.PanelID
		WHERE base.AdminInfoID = @AdminInfoID AND PanelData.PanelID IS NULL AND oversize.SupplementDeleted > 0 --AND base.ID NOT IN (SELECT EstimateDataPanelID FROM @PanelData) 
   
		--DELETE FROM #PanelData WHERE SupplementVersion > @SupplementVersion


	-- SELECT * FROM #PanelData

	-----------------------------------------------------------------------------------------------------------------------------------------
	-- End Copy from GetProcessedLines
	-----------------------------------------------------------------------------------------------------------------------------------------

	DECLARE @RateProfileID INT = (SELECT RateProfileID FROM PDR_EstimateData WHERE AdminInfoID = @AdminInfoID)

	SELECT --*
		  PDR_Panel.PanelName
		--, ISNULL(OversizedResults.Description, '') AS DentCount
		, 
			CASE WHEN PanelData.QuantityID > 0 
				THEN CAST(ISNULL(PDR_QuantityLookup.Min, 0) as VARCHAR) + ' - ' + CAST(ISNULL(PDR_QuantityLookup.Max, 0) as VARCHAR) + ' ' + ISNULL(PDR_SizeLookup.Size, '') 
				ELSE ''
				END
				AS DentCount
		, ISNULL(PanelData.OversizedDents, 0) AS OversizedDents
		, ISNULL(PanelData.CustomCharge, 0) AS CustomCharge
		, ISNULL(PDR_Multiplier.Name, '') AS Multiplier
		, 
			CASE WHEN PanelData.CustomCharge = 0 THEN ISNULL(PDR_Rate.Amount, 0) ELSE 0 END -- If there's no custom charge, use the Rate's amount
			+ ISNULL(OversizedResults.Total, 0) 
			+ ISNULL(PanelData.CustomCharge, 0) 
			+ CASE WHEN ISNULL(PDR_Multiplier.ID, 0) = 0 THEN 0 ELSE CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END * (PDR_Multiplier.Value / 100) END -- The Modifier Price
			AS TotalCharge
	FROM PDR_Panel
	LEFT OUTER JOIN #PanelData PanelData ON PDR_Panel.PanelID = PanelData.PanelID
	LEFT OUTER JOIN
	(
		-- Get the highest supplement for the panel
		SELECT EstimateDataPanelID, MAX(SupplementVersion) AS MaxSupplementVersion
		FROM #PanelData
		GROUP BY EstimateDataPanelID
	) AS MaxSuppVersion ON PanelData.EstimateDataPanelID = MaxSuppVersion.EstimateDataPanelID
	LEFT OUTER JOIN PDR_SizeLookup ON PanelData.SizeID = PDR_SizeLookup.ID
	LEFT OUTER JOIN PDR_QuantityLookup ON PanelData.QuantityID = PDR_QuantityLookup.ID
	LEFT OUTER JOIN #OversizedResults OversizedResults ON PanelData.EstimateDataPanelID = OversizedResults.EstimateDataPanelID
	LEFT OUTER JOIN PDR_Multiplier ON PanelData.Multiplier = PDR_Multiplier.ID-- Join the rate profile data   
	JOIN PDR_RateProfile ON PDR_RateProfile.ID = @RateProfileID AND PDR_RateProfile.Deleted = 0
	LEFT OUTER JOIN PDR_Rate ON    
		PDR_Rate.RateProfileID = PDR_RateProfile.ID    
		AND PanelData.SizeID = PDR_Rate.Size   
		AND PanelData.PanelID = PDR_Rate.PanelID  
		AND PanelData.QuantityID = PDR_Rate.Quantity   
	WHERE
	(
		CASE WHEN PanelData.CustomCharge = 0 THEN ISNULL(PDR_Rate.Amount, 0) ELSE 0 END 
		+ ISNULL(OversizedResults.Total, 0) 
		+ ISNULL(PanelData.CustomCharge, 0) 
		+ CASE WHEN ISNULL(PDR_Multiplier.ID, 0) = 0 THEN 0 ELSE CASE WHEN PanelData.CustomCharge <> 0 THEN PanelData.CustomCharge ELSE PDR_Rate.Amount END * (PDR_Multiplier.Value / 100) END -- The Modifier Price
	) > 0
	--AND ISNULL(OversizedResults.SupplementVersion, PanelData.SupplementVersion) = MaxSuppVersion.MaxSupplementVersion
	AND PanelData.SupplementVersion = MaxSuppVersion.MaxSupplementVersion
	ORDER BY PDR_Panel.SortOrder

	--DROP TABLE #OversizedResults
	--DROP TABLE #OversizedSummary
	--DROP TABLE #PanelData
END
GO
