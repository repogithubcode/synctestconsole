USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 4/28/2017
-- =============================================
CREATE PROCEDURE [dbo].[GetEstimationImageByID]
	@id			int
AS
BEGIN
	SELECT *
	FROM EstimationImages
	WHERE id = @id
END

GO
