USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/17/2017 
-- Description:	Get a promo code by ID 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetPromoCode] 
	@promoID			int 
AS 
BEGIN 
	SELECT * 
	FROM Promo 
	WHERE PromoID = @promoID 
END 
GO
