USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetWebEstLoginInfo]
	@LoginID			int
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT ID AS LoginID, LoginName, Organization, Logins.SalesRepID, FirstName AS SalesRep_FirstName, LastName AS SalesRep_LastName
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Logins 
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.SalesRep ON Logins.SalesRepID = SalesRep.SalesRepID
	WHERE 
		Logins.ID = @LoginID 
		AND ISNULL(Logins.Disabled, 0) = 0 
END

GO
