USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteCustomerProfilePresetsNotesByAdminInfo]
	@admininfoid int
AS
DELETE FROM CustomerProfilePresetsNotes
FROM Customerprofilepresetsnotes cppn
inner join customerprofilepresets cpp
inner join customerprofiles cp
inner join admininfo a
on cp.id = a.customerprofilesid
on cp.id = cpp.customerprofilesid 
on cpp.id = cppn.customerprofilepresetsid
where a.id = @admininfoid


GO
