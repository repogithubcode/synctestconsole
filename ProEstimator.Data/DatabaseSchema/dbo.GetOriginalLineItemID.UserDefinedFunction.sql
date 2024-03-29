USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 4/10/2018
-- Description:	A line item can modify another line in a supplement.  Many supplements can modify the same line.
--				Recursevely look up the Modifies chain and return the base line's ID
-- =============================================
CREATE FUNCTION [dbo].[GetOriginalLineItemID]
(
	@LineItemID			int
)
RETURNS int
AS
BEGIN
	
	DECLARE @ModifiesID INT = (SELECT ISNULL(ModifiesID, 0) FROM EstimationLineItems WHERE ID = @LineItemID)

	IF @ModifiesID > 0
		return dbo.GetOriginalLineItemID(@ModifiesID)

	return @LineItemID
END
GO
