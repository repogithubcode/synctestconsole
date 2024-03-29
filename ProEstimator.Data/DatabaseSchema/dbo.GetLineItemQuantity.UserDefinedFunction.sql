USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 6/16/2017 
-- Description: Gets the Quantity to show for a line item.  When a supplement changes a line item quantity, it adds a new line item with the difference in quantity 
--				This function sums up all of the quantity changes. 
-- ============================================= 
CREATE FUNCTION [dbo].[GetLineItemQuantity] (@lineItemID int) 
RETURNS int 
AS 
BEGIN 
	DECLARE @quantity int = (SELECT ISNULL(Qty, 0) FROM EstimationLineItems WHERE id = @lineItemID) 
	DECLARE @modifiedLineItemID INT = (SELECT ISNULL(ModifiesID, -1) FROM EstimationLineItems WHERE id = @lineItemID) 
 
	IF @modifiedLineItemID > -1  
		BEGIN 
			SET @quantity = @quantity + dbo.GetLineItemQuantity(@modifiedLineItemID) 
		END 
 
	RETURN ISNULL(@quantity, 0) 
END 
GO
