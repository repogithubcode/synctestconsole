USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 8/17/23
-- Description:	Copy an overlap line, used when a supplement line is created and the overlap needs to be copied.
-- =============================================
CREATE PROCEDURE [dbo].[Overlaps_CopyForSupplement]
	  @LineItemID			int
	, @NewLineID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO EstimationOverlap
	(
		  EstimationLineItemsID1 
		, EstimationLineItemsID2 
		, OverlapAdjacentFlag 
		, Amount 
		, SectionOverlapsID 
		, Minimum 
		, UserOverride 
		, UserAccepted 
		, UserResponded 
		, SupplementLevel 
	)
	SELECT 
		  @NewLineID 
		, EstimationLineItemsID2 
		, OverlapAdjacentFlag 
		, Amount 
		, SectionOverlapsID 
		, Minimum 
		, UserOverride 
		, UserAccepted 
		, UserResponded 
		, SupplementLevel 

	FROM EstimationOverlap 
	WHERE EstimationLineItemsID1 = @LineItemID 

END
GO
