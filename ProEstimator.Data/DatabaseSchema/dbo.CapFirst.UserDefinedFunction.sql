USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE FUNCTION [dbo].[CapFirst] (
	@String VarChar(5000)		)  
RETURNS VarChar(5000) AS  
BEGIN 
	DECLARE @TempString VarChar(5000)
	DECLARE @SpaceAt Int

	SELECT @TempString = RTRIM(LTRIM(@String))
	SELECT @TempString = UPPER(SubString(@TempString,1,1)) + LOWER(SubString(@TempString,2,LEN(@TempString)))
	SELECT @SpaceAt = CHARINDEX(' ',@TempString)

	WHILE @SpaceAt>0
	BEGIN
		SELECT @TempString = 	LEFT(@TempString,@SpaceAt) + 
					UPPER(SUBSTRING(@TempString,@SpaceAt+1,1)) + 
					SUBSTRING(@TempString,@SpaceAt+2,LEN(@TempString))
		SELECT @SpaceAt = CHARINDEX(' ',@TempString, @SpaceAt+1)
	END

	RETURN(@TempString)
END




GO
