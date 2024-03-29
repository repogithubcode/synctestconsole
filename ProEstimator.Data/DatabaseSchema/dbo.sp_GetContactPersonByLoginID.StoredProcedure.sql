USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure


 
CREATE PROCEDURE [dbo].[sp_GetContactPersonByLoginID]
	@LoginsID Int 
AS
BEGIN

	-- Ezra - 9/27/2019
	-- Change this query to get the contact from the OrgInfo table instead of the Logins table.  Don't think the Logins one should be used.  There's no place for it to be edited currently, it only comes from WebEst imports
   -- SELECT cp.*
	  --FROM [FocusWrite].[dbo].[tbl_ContactPerson] as cp with(nolock)
			--inner join Logins as l with(nolock)
			--	on l.ContactsID = cp.ContactID
	  --where l.id = @LoginsID
	  --and cp.ContactTypeID = 1
	  --and cp.ContactSubTypeID = 22

	SELECT tbl_ContactPerson.*
	FROM Logins
	LEFT OUTER JOIN OrganizationInfo ON Logins.OrganizationID = OrganizationInfo.id
	LEFT OUTER JOIN tbl_ContactPerson ON OrganizationInfo.OrgInfoContactsID = tbl_ContactPerson.ContactID
	WHERE Logins.ID = @LoginsID

END

GO
