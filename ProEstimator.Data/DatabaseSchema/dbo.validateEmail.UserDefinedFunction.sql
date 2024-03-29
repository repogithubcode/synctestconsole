USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[validateEmail](@Email varchar(200))
RETURNS bit
AS

/*

SELECT dbo.validateEmail('test@test.com')	-- Valid
SELECT dbo.validateEmail(NULL)	-- Invalid
SELECT dbo.validateEmail('sesdafsafsda')	-- Invalid
SELECT dbo.validateEmail('test@test.commm')	-- Invalid
SELECT dbo.validateEmail('test@tes&=+t.com')	-- Invalid
SELECT dbo.validateEmail('test@@test.com')	-- Invalid

*/

BEGIN     

	DECLARE	@isValid bit,
			@i int,
			@AsciiValue int,
			@CountAt int
	
	-- Set variables
	SELECT	@Email = isNull(@Email, ''),
			@isValid = 1,
			@CountAt = 0


	-- Check format validity
	IF @Email LIKE '% %'	-- Email contains a space
		OR @Email NOT LIKE '_%@__%.__%'	-- Email not formatted correctly
		OR charIndex('.', Reverse(@Email), 0) NOT IN (3,4,5)	-- Domain extension is not 2, 3, or 4 characters (ie ".com" or ".uk" or ".info")
	BEGIN
		-- Email is invalid
		SET @isValid = 0
	END
	
	
	-- Make sure there is only one @
	SET @i = 1
	WHILE @i <= Len(@Email)
	BEGIN
		
		IF Substring(@Email, @i, 1) = '@'
			SET @CountAt = @CountAt + 1
		
		IF @CountAt > 1
		BEGIN
			SET @isValid = 0
			BREAK
		END
		
		SET @i = @i + 1
	END
	
	
	-- Check character validity
	SET @i = 1
	WHILE @i <= Len(@Email)
	BEGIN
		
		SET @AsciiValue = ASCII(Substring(Lower(@Email), @i, 1))
		
		IF @AsciiValue NOT BETWEEN 97 AND 122	-- a-z
			AND @AsciiValue NOT BETWEEN 48 AND 57	-- 0-9
			AND @AsciiValue NOT IN (45, 46, 64, 95, 126)	-- (-, ., @, _, ~)
		BEGIN
			SET @isValid = 0
			BREAK
		END
		
		SET @i = @i + 1
	END
	
	RETURN @isValid

END

GO
