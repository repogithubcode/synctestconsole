USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteEstimationLineLaborByAdminInfo]
	@admininfoid int
AS
DELETE FROM EstimationLineLabor 
FROM estimationlinelabor ell
inner join estimationlineitems eli
inner join estimationdata ed
on ed.id = eli.estimationdataid
on eli.id = ell.estimationlineitemsid
where ed.admininfoid = @admininfoid


GO
