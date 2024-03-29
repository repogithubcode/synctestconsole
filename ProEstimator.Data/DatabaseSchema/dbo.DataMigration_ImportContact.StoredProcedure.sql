USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DataMigration_ImportContact] 
	@adminInfoID		int,
	@contactTypeID		int = 1,
	@contactSubTypeID	int,
	@contactSubTypeTargetID	int = 0,
	@importAddress		bit = 0
AS
BEGIN

	IF (SELECT ISNULL(COUNT(*), 0) FROM tbl_ContactPerson WHERE AdminInfoID = @adminInfoID AND ContactTypeID = @contactTypeID AND ContactSubTypeID = @contactSubTypeID) = 0
		BEGIN
			CREATE TABLE #blank (Empty varchar(50));
			INSERT INTO #blank (Empty) Values ('empty');

			-- Get all fields for the contact in an easy to use table
			CREATE TABLE #ownerFields2 (T varchar(50), Q varchar(50), D varchar(50))
			INSERT INTO #ownerFields2
				SELECT DISTINCT ItemText, Qualifier, ContactItemTypes.Description
				FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo
				LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoTOContacts ON AdminInfoTOContacts.AdminInfoID = AdminInfo.id
				LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts ON Contacts.ID = AdminInfoTOContacts.ContactsID AND ContactTypeID = @contactTypeID AND ContactSubTypeID = @contactSubTypeID
				JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ON ContactItems.ContactsID = Contacts.id
				JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItemTypes ON ContactItemTypes.id = ContactItems.ContactItemTypeID
				WHERE AdminInfo.id = @adminInfoID 

			-- Create a ContactPerson record for the customer
			INSERT INTO tbl_ContactPerson (AdminInfoID, FirstName, LastName, Email, Phone1, Phone2, FaxNumber, BusinessName, ContactTypeID, ContactSubTypeID)
			SELECT @adminInfoID, firstName.T, lastName.T, email.T, REPLACE(phone1.T, '-', ''), REPLACE(phone2.T, '-', ''), REPLACE(fax.T, '-', ''), biz.T, @contactTypeID, CASE WHEN @contactSubTypeTargetID > 0 THEN @contactSubTypeTargetID ELSE @contactSubTypeID END
			FROM #blank
			LEFT OUTER JOIN #ownerFields2 AS firstName ON firstName.D = 'First'
			LEFT OUTER JOIN #ownerFields2 AS lastName ON lastName.D = 'Last'
			LEFT OUTER JOIN #ownerFields2 AS email ON email.D = 'Email Address'
			LEFT OUTER JOIN #ownerFields2 AS phone1 ON phone1.D = 'Number' AND ISNULL(phone1.Q, '') = CASE WHEN @importAddress = 1 THEN 'CP' ELSE '' END
			LEFT OUTER JOIN #ownerFields2 AS phone2 ON phone2.D = 'Number' AND ISNULL(phone2.Q, '') = CASE WHEN @importAddress = 1 THEN 'HP' ELSE 'nope' END
			LEFT OUTER JOIN #ownerFields2 AS fax ON fax.D = 'Fax'
			LEFT OUTER JOIN #ownerFields2 AS biz ON biz.D = 'Alias'

			DECLARE @contactID int = (SELECT CAST(SCOPE_IDENTITY() AS INT))

			IF @importAddress = 1 
				BEGIN
					-- Create an Address record for the customer
					INSERT INTO tbl_Address (AdminInfoID, ContactsID, Address1, Address2, City, State, Country, zip, TimeZone)
					SELECT @adminInfoID, @contactID, address1.T, address2.T, city.T, state.T, '', zip.T, timeZone.T
					FROM #blank
					LEFT OUTER JOIN #ownerFields2 AS address1 ON address1.D = 'Line 1'
					LEFT OUTER JOIN #ownerFields2 AS address2 ON address2.D = 'Line 2'
					LEFT OUTER JOIN #ownerFields2 AS city ON city.D = 'City'
					LEFT OUTER JOIN #ownerFields2 AS state ON state.D = 'State'
					LEFT OUTER JOIN #ownerFields2 AS zip ON zip.D = 'Zip'
					LEFT OUTER JOIN #ownerFields2 AS timeZone ON timeZone.D = 'TimeZone'
				END

			DROP TABLE #ownerFields2
			DROP TABLE #blank
		END

END
GO
