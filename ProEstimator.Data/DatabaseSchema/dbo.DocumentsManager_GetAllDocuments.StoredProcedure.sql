USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DocumentsManager_GetAllDocuments]      
AS 
BEGIN 

	SELECT DocumentID, [Name], [FileName], [Category], [UploadDate], [IsDeleted]
	FROM Document 
	WHERE IsDeleted = 0
END 
GO
