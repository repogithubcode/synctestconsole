USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[DeleteEstimatorsData]
	@EstimatorID int
AS
BEGIN

	UPDATE	SiteUsers
	SET		DefaultEstimatorID = NULL
	WHERE	DefaultEstimatorID = @EstimatorID

	DELETE
	FROM EstimatorsData 
	WHERE EstimatorID = @EstimatorID

END
GO
