USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StripeInfoGetByLogin] 
	@loginID		int,
	@deleteFlag		BIT = 0
AS 
BEGIN 
	SELECT * 
	FROM StripeInfo 
	WHERE LoginID = @loginID AND DeleteFlag = ISNULL(@deleteFlag,  DeleteFlag)
	ORDER BY ID DESC 
END 
GO
