USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/1/2017
-- =============================================
CREATE PROCEDURE [dbo].[GetEstimateImageLookup]
	@id			int
AS
BEGIN
	SELECT * FROM EstimationImagesLookup WHERE ID = @id
END

GO
