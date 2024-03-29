USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[GetLatestLineItemMod] (@id Int)
RETURNS Int AS  
BEGIN
	DECLARE @NewID Int
	DECLARE @ModifiesID Int
	
	SELECT @NewID = ID
	FROM EstimationLineItems WITH (NOLOCK)
	WHERE ModifiesID = @ID

	IF @NewID IS NOT NULL
		SELECT @NewID = FocusWrite.dbo.GetLatestLineItemMod(@NewID)
	ELSE
		SELECT @NewID = @ID

	RETURN (@NewID)
END



GO
