USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/17/2020 
-- Description:	Get the total of unused user messages to show a user 
-- ============================================= 
CREATE PROCEDURE [dbo].[UserMessages_GetUnseenCountForLogin] 
	@LoginID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
	 
	SELECT COUNT(*) 
	FROM UserMessages 
	LEFT OUTER JOIN UserMessagesConfirm ON UserMessages.ID = UserMessagesConfirm.UserMessageID AND UserMessagesConfirm.UserLoginID = @LoginID 
	WHERE  
		UserMessages.IsDeleted = 0 
		AND (GETDATE() BETWEEN UserMessages.StartDate AND UserMessages.EndDate OR UserMessages.IsPermanent = 1) 
		AND UserMessagesConfirm.ID IS NULL 
    
END 
GO
