USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE   PROCEDURE [dbo].[GetLevelTerms]
	@ContractTypeID int = NULL,
	@PriceLevel int = NULL,
	@ContractPriceLevelID int = NULL,
	@PriceLevelActive bit = NULL,
	@ContractID int = NULL,
	@ShowCurrentFirst bit = 1,
	@IncludeFreeTerms bit = 0
AS

/*

-- Frame Data prices
EXECUTE GetLevelTerms
	@ContractTypeID = 2

-- EMS prices
EXECUTE GetLevelTerms
	@ContractTypeID = 5

-- Active Estimating prices
EXECUTE GetLevelTerms
	@ContractTypeID = 1,
	@PriceLevelActive = 1

-- Active Estimating prices, including current plan for a particular contract
EXECUTE GetLevelTerms
	@PriceLevel = 7,
	@PriceLevelActive = 1,
	@ContractID = 946

-- Specific Level/Term
EXECUTE GetLevelTerms
	@ContractPriceLevelID = 81

*/


DECLARE	@IncludeContractPriceLevelID int
SELECT	@IncludeContractPriceLevelID = NextContractPriceLevelID FROM dbo.fnGetContract(0, @ContractID, 1)


SELECT		ContractPriceLevelID,
			PriceLevel,
			TermDescription,
			PaymentDescription,
			( CASE WHEN PriceLevelActive = 1 THEN LevelTerm ELSE ( LevelTerm + '  (EXPIRED)' ) END ) AS LevelTerm,
			NumberOfPayments,
			DepositRequired,
			DepositAmount,
			PaymentAmount,
			TermTotal,
			ForceAutoPay,
			PriceLevelActive
FROM		vwContractPriceLevelTerms
WHERE		ContractTypeID = isNull(@ContractTypeID, ContractTypeID)
AND			(
				ContractPriceLevelID = @IncludeContractPriceLevelID	-- Include current plan in results
				OR (
					TermActive = ( CASE WHEN @ContractPriceLevelID IS NOT NULL THEN TermActive ELSE 1 END )	-- Only TermActive = 1 should be returned, unless a specific @ContractPriceLevelID is passed to SP
					AND PriceLevel = isNull(@PriceLevel, PriceLevel)
					AND ContractPriceLevelID = isNull(@ContractPriceLevelID, ContractPriceLevelID)
					AND PriceLevelActive = isNull(@PriceLevelActive, PriceLevelActive)
					AND PriceLevel >= ( CASE WHEN @IncludeFreeTerms = 1 THEN -1 ELSE 0 END )	-- PriceLevel of -1 is free/demo
				)
			)
ORDER BY	( CASE WHEN ContractPriceLevelID = @IncludeContractPriceLevelID AND @ShowCurrentFirst = 1 THEN 0 ELSE 1 END ),
			PriceLevelActive DESC,
			PriceLevel DESC,
			TermTotal

GO
