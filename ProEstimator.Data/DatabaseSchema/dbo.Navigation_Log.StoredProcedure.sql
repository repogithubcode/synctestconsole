USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Navigation_Log]
	@UserID INT,
	@ControlButton VARCHAR(50)
AS 
BEGIN 

	INSERT INTO [dbo].[NavigationLog]([UserID],[ControlButton])
	VALUES(@UserID,@ControlButton)
 
END 
 
GO
