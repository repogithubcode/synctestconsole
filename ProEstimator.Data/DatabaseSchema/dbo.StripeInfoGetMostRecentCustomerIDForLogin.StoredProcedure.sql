USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[StripeInfoGetMostRecentCustomerIDForLogin] 
	@loginID		int
AS 
BEGIN 

	DECLARE @Result VARCHAR(50) = ''

	SET @Result =
	(
		SELECT TOP 1 ISNULL(StripeCustomerID, '') AS StripeCustomerID
		FROM StripeInfo
		WHERE LoginID = @loginID
		ORDER BY ID DESC 
	)

	SELECT ISNULL(@Result, '')
END 
GO
