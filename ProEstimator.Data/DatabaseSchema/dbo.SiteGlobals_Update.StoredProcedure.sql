USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/23/2020 
-- Description:	Update the site globals data 
-- ============================================= 
CREATE PROCEDURE [dbo].[SiteGlobals_Update] 
	  @HomePageMessage		varchar(500) 
	, @AluminumEstimateID	int
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	UPDATE SiteGlobals 
	SET 
		  HomePageMessage = @HomePageMessage 
		, AluminumEstimateID = @AluminumEstimateID
	WHERE ID = 1 
END 
GO
