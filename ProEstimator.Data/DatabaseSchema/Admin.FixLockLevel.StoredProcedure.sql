USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [Admin].[FixLockLevel]
	@LoginID		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;


	UPDATE EstimationData
	SET LockLevel = ISNULL(SupplementVersion, 0)
	FROM AdminInfo
	JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.ID
	WHERE CreatorID = @LoginID AND LockLevel = 99

END
GO
