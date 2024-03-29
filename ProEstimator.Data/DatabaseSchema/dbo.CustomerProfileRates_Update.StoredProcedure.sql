USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/29/2018 
-- Description:	Get all rates for a profile 
-- ============================================= 
CREATE PROCEDURE [dbo].[CustomerProfileRates_Update] 
	  @ID					INT 
	, @CustomerProfilesID	INT 
	, @RateType				TINYINT 
	, @Rate					REAL 
	, @CapType				TINYINT 
	, @Cap					REAL 
	, @Taxable				BIT 
	, @DiscountMarkup		REAL 
	, @IncludeIn			TINYINT 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    UPDATE CustomerProfileRates 
	SET 
		  CustomerProfilesID = @CustomerProfilesID 
		, RateType	= @RateType 
		, Rate = @Rate 
		, CapType= @CapType 
		, Cap = @Cap 
		, Taxable = @Taxable 
		, DiscountMarkup = @DiscountMarkup 
		, IncludeIn = @IncludeIn 
	WHERE id = @ID 
END 
GO
