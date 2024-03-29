USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/28/2019
-- Description:	Return True if the passes line item ID has been deleted by the passed supplement
-- =============================================
CREATE FUNCTION [dbo].[IsLineDeletedBySupplement]
(
	  @LineItemID			int
	, @Supplement			int
)
RETURNS bit
AS
BEGIN
	
	DECLARE @ParentID INT
	DECLARE @ParentSupplement INT 
	DECLARE @ParentDescription VARCHAR(50)

	SELECT 
		  @ParentID = id
		, @ParentSupplement = SupplementVersion
		, @ParentDescription = ActionDescription
	FROM EstimationLineItems
	WHERE ModifiesID = @LineItemID

	IF @ParentID IS NULL
		RETURN 0

	IF @ParentDescription LIKE 'Delete%' AND @Supplement >= @ParentSupplement
		RETURN 1

	return 1
	--RETURN dbo.IsLineDeletedBySupplement(@ParentID, @Supplement)

END
GO
