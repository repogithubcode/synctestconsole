USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/4/2019 
-- Description:	Get a profile print record by profile ID 
-- ============================================= 
CREATE PROCEDURE [dbo].[CustomerProfilesPrint_GetByProfile] 
   @CustomerProfilesID	int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    SELECT *  
	FROM CustomerProfilePrint 
	WHERE CustomerProfilesID = @CustomerProfilesID 
 
END 
GO
