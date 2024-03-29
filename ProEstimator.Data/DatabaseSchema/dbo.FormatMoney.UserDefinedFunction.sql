USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[FormatMoney] (@Value Money)
RETURNS VarChar(25) AS  
BEGIN
    DECLARE @TempValue VarChar(20)
    DECLARE @OutValue VarChar(20)
    DECLARE @Cents Int
    DECLARE @i Int
    
    SELECT @Cents = ABS((@Value - ROUND(@Value,0,1)) * 100)
    SELECT @OutValue = '.' + RIGHT('0'+CONVERT(VarChar(2),@Cents),2)
    
    SELECT @TempValue = CONVERT(VarChar(20),@Value)
    SELECT @TempValue = REVERSE(LEFT(@TempValue,CHARINDEX('.',@TempValue)-1))
    SELECT @i = 1
    WHILE @i <= LEN(@TempValue)
    BEGIN
        if CONVERT(Real,(@i-1))/3 = ROUND(CONVERT(Real,(@i-1))/3,0,1) AND @i <> 1
            SELECT @OutValue = ',' + @OutValue
        SELECT @OutValue = SUBSTRING(@TempValue,@i,1) + @OutValue
        SELECT @i = @i + 1
    END
    SELECT @OutValue=REPLACE(@OutValue,'-,','-')
    SELECT @OutValue=REPLACE(@OutValue,'*','0')
    Return (@OutValue)
END



GO
