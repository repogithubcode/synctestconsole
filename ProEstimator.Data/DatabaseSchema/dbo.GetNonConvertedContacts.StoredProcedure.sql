USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE   PROCEDURE [dbo].[GetNonConvertedContacts]
	@DaysBack int,
	@LastID int,
	@FirstID int,
	@Next bit	
	
AS

if @Next = 1
begin
	SELECT TOP 50 fly.ID,
		Company, 
		Contact, 
		Address, 
		Address2, 
		City, State, 
		Zip, 
		Status, 
		Phone, 
		Rep, 
		InvitationNumber, 
		fly.ExpireDate,
		CreationDate
	FROM FocusWrite.dbo.Flyer fly WITH(NOLOCK)  
		left outer join logins l WITH(NOLOCK)
		on l.organization = fly.invitationnumber		
	WHERE
		Dateadd(d,@DaysBack*-1,getdate()) <= fly.ExpireDate		
		AND fly.ID > @LastID
		AND l.organization is null
	ORDER BY fly.ID ASC
end
else
begin
	SELECT TOP 50 fly.ID,
		Company, 
		Contact, 
		Address, 
		Address2, 
		City, State, 
		Zip, 
		Status, 
		Phone, 
		Rep, 
		InvitationNumber, 
		fly.ExpireDate,
		CreationDate
	FROM FocusWrite.dbo.Flyer fly WITH(NOLOCK) 
		left outer join logins l WITH(NOLOCK)
		on l.organization = fly.invitationnumber	
	WHERE
		Dateadd(d,@DaysBack*-1,getdate()) <= fly.ExpireDate		
		AND fly.ID < @FirstID
		AND l.organization is null
	ORDER BY fly.ID ASC
end
	



GO
