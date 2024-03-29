USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:    T. Gibson  
-- Create date: 9/7/2017  
-- Description:  Get sales rep by id  
-- =============================================  
CREATE PROCEDURE [dbo].[Getsalesrepbyid] @salesrepid INT = NULL  
AS  
  BEGIN  
      SELECT *  
      FROM   salesrep WITH(nolock)  
      WHERE  salesrepid = @salesrepid  
  END  
 
GO
