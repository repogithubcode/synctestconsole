USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteCustomerProfileEditHistoryByAdminInfo]
	@admininfoid int
AS
DELETE FROM CustomerProfileEditHistory
FROM CustomerProfileEditHistory cpeh
inner join customerprofiles cp
inner join admininfo a 
on cp.id = a.customerprofilesid
on cp.id = cpeh.customerprofileid
where a.id = @admininfoid


GO
