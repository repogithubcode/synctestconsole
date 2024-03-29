USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteEstimationNotesByAdminInfo]
	@admininfoid int
AS
DELETE FROM EstimationNotes
FROM EstimationNotes en
inner join estimationlineitems eli
inner join estimationdata ed
on ed.id = eli.estimationdataid
on eli.id = en.estimationlineitemsid
where ed.admininfoid = @admininfoid


GO
