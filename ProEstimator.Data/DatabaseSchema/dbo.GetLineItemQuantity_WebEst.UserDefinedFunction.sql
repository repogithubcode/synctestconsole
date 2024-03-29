USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/20/2018
-- Description: A copy for importing.
-- ============================================= 
CREATE FUNCTION [dbo].[GetLineItemQuantity_WebEst] (@lineItemID int) 
RETURNS int 
AS 
BEGIN 
	DECLARE @quantity int = (SELECT ISNULL(Qty, 0) FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationLineItems WHERE id = @lineItemID) 
	DECLARE @modifiedLineItemID INT = (SELECT ISNULL(ModifiesID, -1) FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationLineItems WHERE id = @lineItemID) 
 
	IF @modifiedLineItemID > -1  
		BEGIN 
			SET @quantity = @quantity - dbo.GetLineItemQuantity_WebEst(@modifiedLineItemID) 
		END 
 
	RETURN ISNULL(@quantity, 0) 
END 
GO
