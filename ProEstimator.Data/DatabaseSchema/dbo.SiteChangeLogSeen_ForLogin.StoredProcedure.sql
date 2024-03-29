USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/13/2020 
-- Description:	Return 1 if the passed Login has seen the change log updates 
-- ============================================= 
CREATE PROCEDURE [dbo].[SiteChangeLogSeen_ForLogin] 
	@LoginID		int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	SELECT COUNT(*) 
	FROM SiteChangeLogSeen 
	WHERE LoginID = @LoginID 
 
END 
GO
