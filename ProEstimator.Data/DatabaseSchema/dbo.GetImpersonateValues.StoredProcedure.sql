USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



--select * from SalesRep   
  
CREATE procedure [dbo].[GetImpersonateValues]       
@LoginID int 
as      
begin      
--GetImpersonateValue 9
      
select Logins.id,Logins.LoginName,Logins.Organization ,Logins.Password,SalesRep.SalesRepID,      
SalesRep.FirstName,SalesRep.LastName , SalesRep.pUsermaintImpersonate from Logins inner join SalesRep       
on Logins.SalesRepID=SalesRep.SalesRepID       
where Logins.id=@LoginID   
end


GO
