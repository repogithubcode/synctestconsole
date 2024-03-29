USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteCustomerProfilesMiscByAdminInfo]
	@admininfoid int
AS
DELETE FROM CustomerProfilesMisc
FROM CustomerProfilesMisc cpm
inner join customerprofiles cp
inner join admininfo a
on cp.id = a.customerprofilesid
on cpm.customerprofilesid = cp.id
where cp.admininfoid = @admininfoid


GO
