USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================    
-- Author:  <Author,,Name>    
-- Create date: <Create Date,,>    
-- Description: <Description,,>    
-- =============================================    
CREATE PROCEDURE [dbo].[sp_InsertIP]    
 -- Add the parameters for the stored procedure here    
 @LoginID int,    
 @IP nvarchar(50)    
AS    
BEGIN    
 -- SET NOCOUNT ON added to prevent extra result sets from    
 -- interfering with SELECT statements.    
 SET NOCOUNT ON;    
    
     --Insert statements for procedure here    
 insert into IPTracking(LoginID,IP)    
 Values(@LoginID,@IP)    
END 

GO
