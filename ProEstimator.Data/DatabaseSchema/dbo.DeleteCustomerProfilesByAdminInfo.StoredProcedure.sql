USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteCustomerProfilesByAdminInfo]
	@admininfoid int
AS
DELETE FROM CustomerProfiles
FROM customerprofiles cp
inner join admininfo a
on cp.id = a.customerprofilesid
where a.id = @admininfoid


GO
