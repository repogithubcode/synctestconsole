USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[sp_GetProfileMisc]
	@ProfileID int
AS
BEGIN
	 
	SET NOCOUNT ON;
SELECT *      
 FROM CustomerProfilesMisc WITH (NOLOCK)      
 WHERE CustomerProfilesID = @ProfileID 
end


GO
