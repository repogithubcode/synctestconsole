USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DocumentsManager_GetAllCategories] 
AS 
BEGIN 
 
	SELECT DISTINCT Category FROM [dbo].[Document] WHERE IsDeleted = 0;

END 
GO
