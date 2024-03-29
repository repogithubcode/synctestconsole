USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 1/4/2018
-- Description:	Get stripe record
-- =============================================
CREATE PROCEDURE [Admin].[Get_AutoPay] 
	-- Add the parameters for the stored procedure here
	@loginId INT = NULL
AS
BEGIN
SELECT [ID]
      ,[LoginID]
      ,[StripeCustomerID]
      ,[StripeCardID]
      ,[CardLast4]
      ,[CardExpiration]
      ,[DeleteFlag]
      ,[CardError]
      ,[ErrorMessage]
      ,[PlaidAccessToken]
      ,[PlaidItemID]
      ,[StripeBankAccountToken]
      ,[AutoPay]
  FROM [dbo].[StripeInfo] with(nolock)
  WHERE LoginID = @loginId
END
GO
