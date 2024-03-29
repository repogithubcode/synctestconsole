USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE FUNCTION [dbo].[CapFirstSection] (
	@String VarChar(5000)		)  
RETURNS VarChar(5000) AS  
BEGIN 
	DECLARE @TempString VarChar(5000)
	DECLARE @SpaceAt Int
	DECLARE @SlashAt Int
	DECLARE @DashAt Int

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

	SELECT @SlashAt = CHARINDEX('/',@TempString)
	WHILE @SlashAt>0
	BEGIN
		SELECT @TempString = 	LEFT(@TempString,@SlashAt) + 
					UPPER(SUBSTRING(@TempString,@SlashAt+1,1)) + 
					SUBSTRING(@TempString,@SlashAt+2,LEN(@TempString))
		SELECT @SlashAt = CHARINDEX('/',@TempString, @SlashAt+1)
	END

	SELECT @DashAt = CHARINDEX('-',@TempString)
	WHILE @DashAt>0
	BEGIN
		SELECT @TempString = 	LEFT(@TempString,@DashAt) + 
					UPPER(SUBSTRING(@TempString,@DashAt+1,1)) + 
					SUBSTRING(@TempString,@DashAt+2,LEN(@TempString))
		SELECT @DashAt = CHARINDEX('-',@TempString, @DashAt+1)
	END

	IF @TempString LIKE 'Srs %'
		SELECT @TempString = REPLACE(@TempString,'Srs ','SRS ')
	IF @TempString LIKE '% Srs %'
		SELECT @TempString = REPLACE(@TempString,' Srs ',' SRS ')
	IF @TempString LIKE 'Abs/%'
		SELECT @TempString = REPLACE(@TempString,'Abs/','ABS/')
	IF @TempString LIKE '% Abs/%'
		SELECT @TempString = REPLACE(@TempString,' Abs/',' ABS/')
	IF @TempString LIKE 'Usa %'
		SELECT @TempString = REPLACE(@TempString,'Usa ','USA ')
	IF @TempString LIKE '% Usa %'
		SELECT @TempString = REPLACE(@TempString,' Usa ',' USA ')
	IF @TempString LIKE 'Awd %'
		SELECT @TempString = REPLACE(@TempString,'Awd ','AWD ')
	IF @TempString LIKE '% Awd %'
		SELECT @TempString = REPLACE(@TempString,' Awd ',' AWD ')
	IF @TempString LIKE '2wd %'
		SELECT @TempString = REPLACE(@TempString,'2wd ','2WD ')
	IF @TempString LIKE '% 2wd %'
		SELECT @TempString = REPLACE(@TempString,' 2wd ',' 2WD ')
	IF @TempString LIKE '4wd %'
		SELECT @TempString = REPLACE(@TempString,'4wd ','4WD ')
	IF @TempString LIKE '% 4wd %'
		SELECT @TempString = REPLACE(@TempString,' 4wd ',' 4WD ')

	RETURN(@TempString)
END




GO
