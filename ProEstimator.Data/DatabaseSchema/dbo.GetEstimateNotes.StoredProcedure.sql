USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetEstimateNotes] 
	@EstimateID INT = NULL,
	@IsDeleted	BIT	= 0
AS 
BEGIN 

	IF(@IsDeleted = 0)
	BEGIN
		SELECT EstimateNotes.id, EstimateNotes.LoginID, EstimateID,NotesText, DATEADD(HOUR,CAST(SiteSetting.[Value] AS INT), DateCreated) AS [TimeStamp], 
		@IsDeleted AS IsDeleted
		FROM dbo.EstimateNotes 
		LEFT JOIN dbo.SiteSetting ON EstimateNotes.LoginID = SiteSetting.LoginID AND Tag = 'TimeZone'
		WHERE EstimateID = ISNULL(@EstimateID, EstimateID)
		AND DeleteStamp IS NULL
	END
	ELSE IF (@IsDeleted = 1)
	BEGIN
		SELECT EstimateNotes.id, EstimateNotes.LoginID, EstimateID,NotesText, DATEADD(HOUR,CAST(SiteSetting.[Value] AS INT),DeleteStamp) AS [TimeStamp], 
		@IsDeleted AS IsDeleted
		FROM dbo.EstimateNotes 
		LEFT JOIN dbo.SiteSetting ON EstimateNotes.LoginID = SiteSetting.LoginID AND Tag = 'TimeZone' 
		WHERE EstimateID = ISNULL(@EstimateID, EstimateID)
		AND DeleteStamp IS Not NULL
	END
END
GO
