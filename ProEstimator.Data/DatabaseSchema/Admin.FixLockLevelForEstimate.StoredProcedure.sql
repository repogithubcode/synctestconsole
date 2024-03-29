USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 4/18/2019
-- Description:	Limited to estimate level
-- =============================================
CREATE PROCEDURE [Admin].[FixLockLevelForEstimate]
	@LoginID		int,
	@AdminInfoId	int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE EstimationData
	SET LockLevel = ISNULL(SupplementVersion, 0)
	FROM AdminInfo a
	JOIN EstimationData ON EstimationData.AdminInfoID = a.ID
	WHERE CreatorID = @LoginID 
	AND LockLevel = 99
	AND a.id = @AdminInfoId

END
GO
