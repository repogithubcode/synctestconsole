USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO



CREATE FUNCTION [dbo].[FormatNumber] (@pValue Decimal(38,10), @pDecimals TinyInt)
RETURNS VarChar(25) AS  
BEGIN
    DECLARE @TempValue VarChar(20)
    DECLARE @OutValue VarChar(20)
    DECLARE @Fraction Int
    DECLARE @i Int

    SELECT @pValue = Round(@pValue,@pDecimals)
    
    SELECT @Fraction = ABS((ROUND(@pValue - ROUND(@pValue,0,1),@pDecimals)) * POWER(10,@pDecimals))
    SELECT @OutValue = '.' + RIGHT('0000000000'+CONVERT(VarChar(10),@Fraction),@pDecimals)
    
    SELECT @TempValue = CONVERT(VarChar(20),@pValue)
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
    Return (@OutValue)
END



GO
