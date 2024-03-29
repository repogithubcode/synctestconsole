USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/31/2017
-- Description:	Copied from CustomerProfileFunctions so the default profile can be set without saving everything else
-- =============================================
CREATE PROCEDURE [dbo].[CustomerProfile_SetAsDefaultProfile] 
	@ProfileID		INT
AS
BEGIN
	-- Set all Default flags to false    
	  UPDATE CustomerProfiles WITH (ROWLOCK)      
	  SET DefaultFlag = 0      
	  FROM CustomerProfiles WITH (ROWLOCK)      
	  INNER JOIN CustomerProfiles CustomerProfiles2 WITH(NOLOCK) ON      
	   (CustomerProfiles2.OwnerID = CustomerProfiles.OwnerID AND      
		CustomerProfiles2.OwnerType = CustomerProfiles.OwnerType)      
	  WHERE CustomerProfiles2.ID = @ProfileID      
      
	  -- Set the passed Profile ID's default flag to True
	  IF EXISTS ( SELECT ID FROM CustomerProfiles WITH (NOLOCK) WHERE ID = @ProfileID)      
	  UPDATE CustomerProfiles WITH (ROWLOCK)      
	  SET  DefaultFlag = 1      
	  WHERE ID = @ProfileID      
	
END

GO
