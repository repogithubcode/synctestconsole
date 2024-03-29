USE [FocusWrite]
GO
SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE PROCEDURE [dbo].[pwdCheckWord]
	@Password VarChar(250),
	@Word VarChar(250),
	@CompLevel int OUTPUT,
	@MatchCount int OUTPUT
AS

DECLARE @i TinyInt

/* Check if Word is similar to the password */
SELECT @i = 1
SELECT @MatchCount = 0
SELECT @Password = REPLACE(@Password,' ','')
SELECT @Word = REPLACE(@Word,' ','')
WHILE @i <= LEN(@Word)-2
BEGIN
	IF CHARINDEX(SUBSTRING(@Word,@i,3),@Password)>0 OR CHARINDEX(REVERSE(SUBSTRING(@Word,@i,3)),@Password)>0
		SELECT @MatchCount = @MatchCount + 1
	SELECT @i = @i + 1
END

IF LEN(@Word) >2
	SELECT @CompLevel = ((@MatchCount*100) / (LEN(@Word)- 2))
ELSE
	SELECT @CompLevel = 0


GO
