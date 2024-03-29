USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[UpdatePromo]
	@PromoID int,
	@PromoCode varchar(50),
	@PromoAmount decimal(6,2),
	@StartDate datetime,
	@EndDate datetime
AS

/*
EXECUTE UpdatePromo
	@PromoID = 1000,
	@PromoCode = 'ABC123',
	@PromoAmount = 25,
	@StartDate = '4/1/2012',
	@EndDate = '5/1/2012'
*/

SET NOCOUNT ON

SET @EndDate = Convert(varchar(10), @EndDate, 101) + ' 23:59:59.99'

UPDATE	Promo
SET		PromoCode = @PromoCode,
		PromoAmount = @PromoAmount,
		StartDate = @StartDate,
		EndDate = @EndDate
WHERE	PromoID = @PromoID


GO
