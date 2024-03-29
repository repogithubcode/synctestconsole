USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetCustomerProfileForLogin]  
(	 
	@LoginID			int, 
	@Deleted			bit = 0,
	@OmitImported		bit = 0
) 
RETURNS TABLE  
AS 
RETURN  
( 
			SELECT * 
			FROM CustomerProfiles 
			WHERE  
				OwnerID = @LoginID  
				AND ISNULL(AdminInfoID, 0) = 0 
				AND ISNULL(Deleted, 0) = @Deleted
				AND Imported IN (CASE @OmitImported WHEN 0 THEN Imported ELSE 0 END)
) 
GO
