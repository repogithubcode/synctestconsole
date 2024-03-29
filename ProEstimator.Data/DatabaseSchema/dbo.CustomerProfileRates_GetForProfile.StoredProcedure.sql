USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/29/2018 
-- Description:	Get all rates for a profile 
-- ============================================= 
CREATE PROCEDURE [dbo].[CustomerProfileRates_GetForProfile] 
	@ProfileID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    SELECT * 
	FROM CustomerProfileRates 
	WHERE CustomerProfilesID = @ProfileID 
END 
GO
