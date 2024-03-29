USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/9/2020 
-- Description:	Get a user message by ID 
-- ============================================= 
CREATE PROCEDURE [dbo].[UserMessages_Get] 
	@ID				int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	SELECT * 
	FROM UserMessages 
	WHERE ID = @ID 
    
END 
GO
