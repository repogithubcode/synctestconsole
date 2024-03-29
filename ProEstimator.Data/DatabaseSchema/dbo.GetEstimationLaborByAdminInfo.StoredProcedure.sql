USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[GetEstimationLaborByAdminInfo]
@admininfoid int
AS
SELECT 	ell.[id]
	,[EstimationLineItemsID]
	,[LaborType]
	,[LaborSubType]
	,ell.[LaborTime]
	,[LaborCost]
	,[BettermentFlag]
	,[SubletFlag]
	,ell.[UniqueSequenceNumber]
	,ell.[ModifiesID]
	,[AdjacentDeduction]
	,[MajorPanel]
	,ell.[BettermentPercentage]
	,[dbLaborTime]
	,[AdjacentDeductionLock]
	,ell.[barcode]
	,[Lock]
FROM EstimationLineLabor ell WITH(NOLOCK)
inner join estimationlineitems eli WITH(NOLOCK)
inner join estimationdata ed WITH(NOLOCK)
on ed.id = eli.estimationdataid
on eli.id = ell.estimationlineitemsid
where ed.admininfoid = @admininfoid


GO
