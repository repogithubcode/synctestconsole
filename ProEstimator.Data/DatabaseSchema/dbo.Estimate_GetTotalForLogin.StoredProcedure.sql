USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 7/20/20 
-- Description:	Get the number of estimates for a login 
-- ============================================= 
CREATE PROCEDURE [dbo].[Estimate_GetTotalForLogin] 
	@LoginID			int 
AS 
BEGIN 
	SELECT COUNT(*) AS Total 
	FROM AdminInfo 
	WHERE CreatorID = @LoginID AND Deleted = 0 
END 
GO
