USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteCustomerProfilePresetsByAdminInfo]
	@admininfoid int
AS
DELETE FROM CustomerProfilePresets 
FROM customerprofilepresets cpp
inner join customerprofiles cp
inner join admininfo a
on cp.id = a.customerprofilesid
on cpp.customerprofilesid = cp.id
where a.id = @admininfoid


GO
