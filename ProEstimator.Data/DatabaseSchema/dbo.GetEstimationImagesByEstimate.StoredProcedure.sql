USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetEstimationImagesByEstimate]
	@adminInfoID			int,
	@includeDeleted			bit = 0
AS
BEGIN
	SELECT *
	FROM EstimationImages
	WHERE 
		AdminInfoID = @adminInfoID
		AND
		(
			@includeDeleted = 1
			OR ISNULL(EstimationImages.Deleted, 0) = 0
		)
	Order By OrderNo asc, id desc
END
GO
