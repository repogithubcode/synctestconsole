USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/2/2019
-- Description:	Gets all Reports records for an Estimate
-- =============================================
CREATE PROCEDURE [dbo].[Report_GetByEstimate]
	@EstimateID		INT
	, @IncludeDeleted  BIT = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *
	FROM Reports
	WHERE 
		AdminInfoID = @EstimateID
		AND (DeleteStamp IS NULL OR @IncludeDeleted = 1)
END
GO
