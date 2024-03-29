USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[SP_GetSourcePartsFileMapping]  
	@SourceID VARCHAR(250) = NULL,
	@SourceName VARCHAR(250) = NULL,
	@PartsFileNameWithExt VARCHAR(250) = NULL
AS                      
BEGIN        
        
SELECT SourceID,SourceName,PartsFileNameWithExt         
FROM  [dbo].[SourcePartsFileMapping]
WHERE SourceID = ISNULL(@SourceID, SourceID) AND SourceName = ISNULL(@SourceName, SourceName)
AND PartsFileNameWithExt = ISNULL(@PartsFileNameWithExt, PartsFileNameWithExt)
        
END 
GO
