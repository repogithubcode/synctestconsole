USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/27/2017
-- Description:	Makes a copy of a tbl_ContactPerson and their associated tbl_Address.  Used for copying an estimate.
-- =============================================
CREATE PROCEDURE [dbo].[CopyContactAndAddress]
	@adminInfoID INT,
	@newAdminInfoID INT,
	@contactTypeID TINYINT,
	@contactSubTypeID SMALLINT
AS
BEGIN

	DECLARE @oldContactID INT;
	DECLARE @newContactID INT;

	-- Get the old contact ID used later to copy the address
	SET @oldContactID = (SELECT TOP 1 ContactID FROM tbl_ContactPerson WHERE AdminInfoID = @adminInfoID AND ContactTypeID = @contactTypeID AND ContactSubTypeID = @contactSubTypeID)

	-- Copy the Contact record
	INSERT INTO tbl_ContactPerson
	(AdminInfoID, FirstName, MiddleName, LastName, Email, Phone2, Phone1, FaxNumber, BusinessName, Notes, Title, SaveCustomer, Extension1, Extension2, ContactTypeID, ContactSubTypeID, DateAdded)
	SELECT @newAdminInfoID, FirstName, MiddleName, LastName, Email, Phone2, Phone1, FaxNumber, BusinessName, Notes, Title, SaveCustomer, Extension1, Extension2, ContactTypeID, ContactSubTypeID, GETDATE()
	FROM tbl_ContactPerson
	WHERE AdminInfoID = @adminInfoID AND ContactTypeID = @contactTypeID AND ContactSubTypeID = @contactSubTypeID

	-- Get the newly created contact ID
	SET @newContactID = (SELECT CAST(SCOPE_IDENTITY() AS INT))

	-- Copy the Address record
	INSERT INTO tbl_Address
	(AdminInfoID, ContactsID, Address1, Address2, City, State, Country, zip, TimeZone)
	SELECT @newAdminInfoID, @newContactID, Address1, Address2, City, State, Country, zip, TimeZone
	FROM tbl_Address
	WHERE AdminInfoID = @adminInfoID AND ContactsID = @oldContactID

END

GO
