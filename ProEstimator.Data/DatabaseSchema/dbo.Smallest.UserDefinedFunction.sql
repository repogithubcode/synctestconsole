USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[Smallest] 
(
	@Value1			float,
	@Value2			float
)
RETURNS float
AS
BEGIN

	IF @Value1 < @Value2
		RETURN @Value1
	
	RETURN @Value2

END
GO
