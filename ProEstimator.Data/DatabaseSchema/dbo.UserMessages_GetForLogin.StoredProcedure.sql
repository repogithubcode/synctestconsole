USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/16/2020 
-- Description:	Get user messages to show a user 
-- ============================================= 
CREATE PROCEDURE [dbo].[UserMessages_GetForLogin] 
	@LoginID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	SELECT UserMessages.*, CASE WHEN UserMessagesConfirm.ID IS NOT NULL THEN 1 ELSE 0 END AS SeenByUser 
	FROM UserMessages 
	LEFT OUTER JOIN UserMessagesConfirm ON UserMessages.ID = UserMessagesConfirm.UserMessageID AND UserMessagesConfirm.UserLoginID = @LoginID 
	WHERE  
		UserMessages.IsDeleted = 0 
		AND (GETDATE() BETWEEN UserMessages.StartDate AND UserMessages.EndDate OR UserMessages.IsPermanent = 1) 
	ORDER BY UserMessages.CreatedDate DESC 
    
END 
GO
