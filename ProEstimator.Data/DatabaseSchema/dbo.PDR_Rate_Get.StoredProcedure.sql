USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 12/8/2017 
-- Description:	Get a PDR Rate record by ID or all for a rate profile 
-- ============================================= 
CREATE PROCEDURE [dbo].[PDR_Rate_Get] 
	@RateID			int = null, 
	@RateProfileID	int = null 
	 
AS 
BEGIN 
	IF NOT @RateID IS NULL 
		BEGIN 
			SELECT * 
			FROM PDR_Rate 
			WHERE ID = @RateID 
		END 
	ELSE IF NOT @RateProfileID IS NULL 
		BEGIN 
			SELECT * 
			FROM PDR_Rate 
			WHERE RateProfileID = @RateProfileID 
		END 
		 
END 
GO
