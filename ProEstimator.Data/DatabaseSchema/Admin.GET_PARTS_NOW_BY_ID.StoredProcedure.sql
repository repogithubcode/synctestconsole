USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:    T. Gibson  
-- Create date: 9/4/20189  
-- Description:  Get parts now client list  
-- =============================================  
CREATE PROCEDURE [Admin].[GET_PARTS_NOW_BY_ID] 
	@Id int 
AS 
BEGIN 
	SELECT ISNULL(PartsNow, 0) 
	FROM Logins 
	WHERE ID = @Id 
END 
GO
