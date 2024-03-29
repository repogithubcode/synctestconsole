USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[aaaGetDataForNewSiteUserAccounts]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT Logins.ID AS LoginID, Logins.Password, tbl_ContactPerson.Email, ISNULL(tbl_ContactPerson.FirstName, '') + ' ' + ISNULL(tbl_ContactPerson.LastName, '') AS Name
	FROM Logins
	LEFT OUTER JOIN tbl_ContactPerson ON Logins.ContactID = tbl_ContactPerson.ContactID
	LEFT OUTER JOIN SiteUsers ON Logins.ID = SiteUsers.LoginID
	WHERE Email IS NOT NULL 
		AND Disabled = 0
		AND Email NOT IN 
		(
			SELECT Email
			FROM
			(
				SELECT tbl_ContactPerson.Email, COUNT(*) Total
				FROM Logins
				LEFT OUTER JOIN tbl_ContactPerson ON Logins.ContactID = tbl_ContactPerson.ContactID
				WHERE Email IS NOT NULL 
					AND Disabled = 0
				GROUP BY tbl_ContactPerson.Email
			) Base
			WHERE Total > 1
		)
		AND SiteUsers.ID IS NULL

	ORDER BY Email

END
GO
