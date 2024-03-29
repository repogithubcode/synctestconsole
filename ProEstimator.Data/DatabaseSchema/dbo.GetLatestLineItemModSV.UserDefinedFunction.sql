USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



-- Gets the latest version, less then or equal to the passed supplement version.
CREATE FUNCTION [dbo].[GetLatestLineItemModSV] (@id Int, @SupplementVersion TinyInt)
RETURNS Int AS  
BEGIN
	DECLARE @NewID Int
	DECLARE @ModifiesID Int
	
	SELECT @NewID = ID
	FROM EstimationLineItems WITH (NOLOCK)
	WHERE ModifiesID = @ID AND
		SupplementVersion <= @SupplementVersion

	IF @NewID IS NOT NULL
		SELECT @NewID = FocusWrite.dbo.GetLatestLineItemModSV(@NewID,@SupplementVersion)
	ELSE
		SELECT @NewID = @ID

	RETURN (@NewID)
END



GO
