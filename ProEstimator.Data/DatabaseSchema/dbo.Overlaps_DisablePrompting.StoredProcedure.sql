USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Ezra Schwartz
-- Create date: 4/3/2018
-- Description:	Copied from App_Functions.  Turns on the "suppress add related prompt" value for the estimate.
-- =============================================
CREATE PROCEDURE [dbo].[Overlaps_DisablePrompting]
	@AdminInfoID	int
AS
BEGIN
	
	UPDATE CustomerProfilesMisc  
	SET SuppressAddRelatedPrompt = 1 
	FROM CustomerProfilesMisc  
	INNER JOIN AdminInfo ON AdminInfo.CustomerProfilesID = CustomerProfilesMisc.CustomerProfilesID
	WHERE AdminInfo.ID = @AdminInfoID 

END

GO
