USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



Create procedure [dbo].[Get_UseTIN_Status]
(
@LoginID  int
)
as
begin
select usetin from Logins where id=@LoginID 
 end


GO
