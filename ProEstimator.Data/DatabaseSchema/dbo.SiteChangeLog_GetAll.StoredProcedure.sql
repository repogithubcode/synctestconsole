USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/13/2020 
-- Description:	Get all site change logs 
-- ============================================= 
CREATE PROCEDURE [dbo].[SiteChangeLog_GetAll] 
 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	SELECT * 
	FROM SiteChangeLog 
	ORDER BY [Date] DESC 
 
END 
GO
