USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:  	T. Gibson
-- Create date: 11/7/2017
-- Description:	Update Autopay
-- =============================================
CREATE PROCEDURE [Admin].[Update_AutoPay] @loginId int = 0,
@autoPay bit = 0
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
      NULL
    WHERE NOT EXISTS (SELECT
      1
    FROM [dbo].[StripeInfo]
    WHERE LoginID = @loginId);

  UPDATE [dbo].[StripeInfo]
  SET AutoPay = @autoPay
  WHERE LoginId = @loginId
END

GO
