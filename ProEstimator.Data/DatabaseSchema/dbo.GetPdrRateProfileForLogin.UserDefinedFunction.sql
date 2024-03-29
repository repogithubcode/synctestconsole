USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[GetPdrRateProfileForLogin]  
(	 
	@LoginID		int = 113941, 
	@RateProfileID	int = null, 
	@Deleted		bit = 0 
) 
RETURNS TABLE  
AS 
RETURN  
( 
	 
	SELECT * 
	FROM PDR_RateProfile 
	WHERE 1 = (CASE WHEN @RateProfileID IS NOT NULL THEN (CASE WHEN ID = @RateProfileID THEN 1 ELSE 0 END)
					WHEN @RateProfileID IS NULL THEN (CASE WHEN (LoginID = @LoginID AND ISNULL(Deleted, 0) = @Deleted AND ISNULL(AdminInfoID, 0) = 0) THEN 1 ELSE 0 END) END)
) 
GO
