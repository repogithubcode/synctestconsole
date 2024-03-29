USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/28/2018 
-- Description:	Get all rate profiles for a login 
-- ============================================= 
CREATE PROCEDURE [dbo].[CustomerProfile_GetForLogin] 
	@LoginID			int, 
	@Deleted			bit = 0,
	@OmitImported		bit = 0
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
 IF @OmitImported = 0
    SELECT * 
	FROM CustomerProfiles 
	WHERE  
		OwnerID = @LoginID  
		AND ISNULL(AdminInfoID, 0) = 0 
		AND ISNULL(Deleted, 0) = @Deleted 
 ELSE
	SELECT * 
	FROM CustomerProfiles 
	WHERE  
		OwnerID = @LoginID  
		AND ISNULL(AdminInfoID, 0) = 0 
		AND ISNULL(Deleted, 0) = @Deleted
		AND Imported = 0
END 
GO
