USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO

CREATE PROCEDURE [dbo].[GetAllTempReports]

AS
SELECT R.ID,AI.CreatorID,R.AdminInfoID,R.Description,
R.FileName,R.IsTemp,R.DateCreated
FROM Reports R
INNER JOIN AdminInfo AI on AI.id=R.AdminInfoID and Isnull(R.IsTemp,0)=1
 






GO
