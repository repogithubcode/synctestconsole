USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE FUNCTION [dbo].[RoundUp](@Val DECIMAL(32,16), @Digits INT)
RETURNS DECIMAL(32,16)
AS
BEGIN
    RETURN CASE WHEN ABS(@Val - ROUND(@Val, @Digits, 1)) * POWER(10, @Digits+1) = 5 
                THEN CEILING(@Val * POWER(10,@Digits))/POWER(10, @Digits)
                ELSE ROUND(@Val, @Digits)
                END
END


GO
