USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
   
   
CREATE PROCEDURE [dbo].[GetPayments_Stripe]   
  @startDate datetime = null,   
  @endDate datetime = null 
AS   
BEGIN   
     
 SET NOCOUNT ON;   
 if @startDate is null   
 begin   
  select @startDate = GETDATE()   
 end   
 if @endDate is null   
 begin   
  select @endDate = GETDATE()   
 end   
  
SELECT DISTINCT  
	  LoginID 
	, Organization 
	, Address1 
	, City 
	, [State] 
	, zip 
	, [TimeStamp]
	, ROUND(Total, 2) AS Total 
 
FROM Payment 
LEFT OUTER JOIN Logins ON Payment.LoginID = Logins.id 
LEFT OUTER JOIN tbl_ContactPerson ON tbl_ContactPerson.ContactID = Logins.ContactID 
LEFT OUTER JOIN tbl_Address ON tbl_ContactPerson.ContactID = tbl_Address.ContactsID 
WHERE CAST([TimeStamp] AS DATE) BETWEEN CAST(@startDate AS DATE) AND CAST(@endDate AS DATE) 
 
END   
   
  
GO
