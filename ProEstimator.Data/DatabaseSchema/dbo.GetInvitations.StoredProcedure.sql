USE [FocusWrite]
GO
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[GetInvitations] AS
SELECT Name, Address, City, State, Zip, Phone, TimeDate
FROM LoginFailures LF WITH(NOLOCK), Invitations INV WITH(NOLOCK)
WHERE LF.Organization = INV.Phone
ORDER BY TimeDate DESC


GO
