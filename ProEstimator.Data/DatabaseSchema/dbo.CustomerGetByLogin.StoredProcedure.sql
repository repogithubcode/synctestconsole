USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CustomerGetByLogin]   
	@LoginID					int 
AS 
BEGIN 
 
	SELECT Customer.*, tbl_ContactPerson.*, tbl_Address.* 
 
	-- This section was to show each first/last combination only once, but it turns out that's not a good thing to do because we hide customer.  Leaving this commented out if we want it later 
	--FROM  
	--( 
	--	SELECT  
	--		MAX(Customer.ID) AS CustomerID 
	--		, tbl_ContactPerson.FirstName 
	--		, tbl_ContactPerson.LastName 
	--	FROM Customer 
	--	LEFT JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID 
	--	WHERE Customer.LoginID = @LoginID 
	--	AND 
	--	( 
	--		ISNULL(FirstName, '') <> ''  
	--		OR ISNULL(LastName, '') <> '' 
	--	) 
	--	GROUP BY FirstName, LastName 
	--) Base 
	FROM Customer with(nolock)
	inner JOIN tbl_ContactPerson with(nolock) ON Customer.ContactID = tbl_ContactPerson.ContactID 
	LEFT JOIN tbl_Address with(nolock) ON Customer.AddressID = tbl_Address.AddressID 
	WHERE Customer.LoginID = @LoginID AND (ISNULL(tbl_ContactPerson.FirstName, '') <> '' OR ISNULL(tbl_ContactPerson.LastName, '') <> '') 
 
	ORDER BY LastName, FirstName 
 
end 
GO
