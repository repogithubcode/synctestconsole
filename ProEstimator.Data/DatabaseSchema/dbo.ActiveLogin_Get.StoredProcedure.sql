USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ActiveLogin_Get]   
	@ID				INT
AS   
BEGIN   
 -- SET NOCOUNT ON added to prevent extra result sets from   
 -- interfering with SELECT statements.   
 SET NOCOUNT ON;   
   
 SELECT ActiveLogin.*
 FROM ActiveLogin
 WHERE ID = @ID

END   
GO
