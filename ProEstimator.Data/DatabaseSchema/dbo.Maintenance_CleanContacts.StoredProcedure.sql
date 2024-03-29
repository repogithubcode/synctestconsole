USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/12/2021
-- =============================================
CREATE PROCEDURE [dbo].[Maintenance_CleanContacts]
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DECLARE @AdminInfoID INT

	DECLARE db_cursor CURSOR FOR

	SELECT TOP 10 AdminInfoID
	FROM (
		SELECT AdminInfoID, COUNT(*) AS Total, MAX(DateAdded) AS DateAdded
		FROM tbl_ContactPerson
		WHERE ContactSubTypeID = 3 AND AdminInfoID <> 0
		GROUP BY AdminInfoID
	)
	AS Base
	LEFT OUTER JOIN AdminInfo ON Base.AdmininfoID = AdminInfo.ID
	WHERE Total > 10

	OPEN db_cursor
	FETCH NEXT FROM db_cursor INTO @AdminInfoID

	WHILE @@FETCH_STATUS = 0  
	BEGIN  
		DELETE
		FROM tbl_ContactPerson
		WHERE 
			tbl_ContactPerson.AdminInfoID = @AdminInfoID 
			AND ContactSubTypeID = 3
			AND ContactID > 
			(
				SELECT MIN(ContactID)
				FROM tbl_ContactPerson
				WHERE tbl_ContactPerson.AdminInfoID = @AdminInfoID AND ContactSubTypeID = 3
			)

		FETCH NEXT FROM db_cursor INTO @AdminInfoID 
	END 

	CLOSE db_cursor  
	DEALLOCATE db_cursor 
END
GO
