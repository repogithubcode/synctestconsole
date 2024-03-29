USE [FocusWrite]
GO
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO




CREATE FUNCTION [dbo].[Extract] (
	@ExtractFrom VarChar(8000), 
	@ElementNumber Int,
	@Separator VarChar(10)		)  
RETURNS VarChar(8000) AS  
BEGIN 
	DECLARE @Extracted VarChar(8000)
	DECLARE @First Int
	DECLARE @Second Int
	DECLARE @Count Int
	DECLARE @SeparatorLength Int
	
	SELECT @SeparatorLength = LEN(REPLACE(@Separator,' ','|'))
	
	IF @ElementNumber = 0
	BEGIN
		IF CHARINDEX(@Separator, @ExtractFrom)  >0
			SELECT @Extracted = SUBSTRING(@ExtractFrom, 1, CHARINDEX(@Separator, @ExtractFrom) - 1)
		ELSE
			SELECT @Extracted = @ExtractFrom
	END
	ELSE
	BEGIN
		SELECT @First = 0
		SELECT @Second = 0
		SELECT @Count = 0
		WHILE @Count <= @ElementNumber AND (@Second <> 0 and @Count>0 OR @Second = 0 AND @Count = 0)
		BEGIN
			SELECT @First = @Second
			SELECT @Second = CHARINDEX(@Separator, @ExtractFrom, @First + @SeparatorLength)
			SELECT @Count = @Count + 1
		END
		IF @Count > @ElementNumber
			IF @Second = 0
				SELECT @Extracted = SUBSTRING(@ExtractFrom, @First+@SeparatorLength, LEN(@ExtractFrom))
			ELSE
				SELECT @Extracted = SUBSTRING(@ExtractFrom, @First+@SeparatorLength,  @Second - @First - @SeparatorLength )
		ELSE
			SELECT @Extracted = ''
	END
	
	RETURN(@Extracted)
END




GO
