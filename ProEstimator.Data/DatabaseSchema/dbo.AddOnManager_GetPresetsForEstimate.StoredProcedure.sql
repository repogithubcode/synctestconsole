USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 6/16/21
-- =============================================
CREATE PROCEDURE [dbo].[AddOnManager_GetPresetsForEstimate]
	@EstimateID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT DISTINCT PresetShellID
	FROM EstimationData
	LEFT OUTER JOIN EstimationLineItems ON EstimationData.ID = EstimationLineItems.EstimationDataID
	WHERE AdminInfoID = @EstimateID
	AND PresetShellID IS NOT NULL
END
GO
