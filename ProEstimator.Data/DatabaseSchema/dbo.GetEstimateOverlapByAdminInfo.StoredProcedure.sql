USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[GetEstimateOverlapByAdminInfo]
@AdminInfoId int
AS
SELECT eo.[id]
	,[EstimationLineItemsID1]
	,[EstimationLineItemsID2]
	,[OverlapAdjacentFlag]
	,[Amount]
	,[SectionOverlapsID]
	,[Minimum]
	,[UserOverride]
	,[UserAccepted]
	,[UserResponded]
	,[SupplementLevel]
FROM EstimationOverlap eo WITH(NOLOCK)
INNER JOIN EstimationLineItems eli WITH(NOLOCK) on eli.id = estimationlineitemsid1
INNER JOIN EstimationData e WITH(NOLOCK) on e.id = eli.estimationdataid
WHERE e.admininfoid = @AdminInfoId
UNION
SELECT 	eo.[id]
	,[EstimationLineItemsID1]
	,[EstimationLineItemsID2]
	,[OverlapAdjacentFlag]
	,[Amount]
	,[SectionOverlapsID]
	,[Minimum]
	,[UserOverride]
	,[UserAccepted]
	,[UserResponded]
	,[SupplementLevel]
FROM EstimationOverlap eo WITH(NOLOCK)
INNER JOIN EstimationLineItems eli2 WITH(NOLOCK) on eli2.id = estimationlineitemsid2
INNER JOIN estimationdata e2 WITH(NOLOCK) on eli2.estimationdataid = e2.id
WHERE e2.admininfoid = @AdminInfoId


GO
