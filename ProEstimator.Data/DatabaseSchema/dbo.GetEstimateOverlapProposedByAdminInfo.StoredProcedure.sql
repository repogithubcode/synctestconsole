USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[GetEstimateOverlapProposedByAdminInfo]
@AdminInfoId int
AS
SELECT 	[ActionPart]
	,[Reason]
	,[BecauseOf]
	,[OverlapAmount]
	,[MinimumLabor]
	,[Labor Hours]
	,[Labor Hours Inc]
	,[ID]
	,[Process]
	,[AdminInfoID]
	,[ActionID]
	,[ResultID]
	,[ActionCode]
	,[AcceptFlag]
	,[BarcodeInc]
FROM EstimationOverlapProposed WITH(NOLOCK)
where admininfoid = @AdminInfoId


GO
