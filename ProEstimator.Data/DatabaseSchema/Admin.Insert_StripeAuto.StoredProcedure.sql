USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 1/4/2018
-- Description:	Insert stripe record
-- =============================================
CREATE PROCEDURE [Admin].[Insert_StripeAuto] 
	-- Add the parameters for the stored procedure here
	@loginId INT = NULL,
	@autoPay BIT = FALSE
AS
BEGIN
	INSERT INTO [dbo].[StripeInfo] ([LoginID]
  , [StripeCustomerID]
  , [StripeCardID]
  , [CardLast4]
  , [CardExpiration]
  , [DeleteFlag]
  , [CardError]
  , [ErrorMessage]
  , [PlaidAccessToken]
  , [PlaidItemID]
  , [StripeBankAccountToken]
  , [AutoPay])
    SELECT
      @loginId,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      NULL,
      @autoPay
    WHERE NOT EXISTS (SELECT
      1
    FROM [dbo].[StripeInfo] with(nolock)
    WHERE LoginID = @loginId);
END
GO
