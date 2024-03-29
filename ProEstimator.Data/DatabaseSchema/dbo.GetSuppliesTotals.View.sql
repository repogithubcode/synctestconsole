USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO





CREATE  VIEW [dbo].[GetSuppliesTotals]
AS
SELECT CASE 	WHEN 	ISNULL(s.Betterment,0) = 1 
			THEN ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
		WHEN 	ISNULL(s.CapType,2) = 1 AND 
			ISNULL(s.Cap,0) < ISNULL(s.Hours,0) 
			THEN ISNULL(s.Rate,0) * ISNULL(s.Cap,0) + ISNULL(s.Cost,0) 
		WHEN 	ISNULL(s.CapType,2) = 0 AND 
			ISNULL(s.Cap,0) < ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
			THEN ISNULL(s.Cap,0)
		ELSE	ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
	END 'LineItemTotal',
		AdminInfoID,
		RateTypesID,
		RateName,
		Betterment,
		EMSCode1,
		EMSBetterment,
		DiscountMarkup,
		CapType,
		Cap,
		Taxable,
		Rate,
		Hours,
		Cost
FROM (
	SELECT SUM(ISNULL(LineItemTotal,0)) 'LineItemTotal',
		AdminInfoID,
		RateTypesID,
		RateName,
		Betterment,
		EMSCode1,
		EMSBetterment,
		DiscountMarkup,
		CapType, 
		Cap,
		Taxable,
		Rate,
		SUM(ISNULL(Hours,0)) 'Hours',
		SUM(ISNULL(Cost,0)) 'Cost'
	FROM (
		SELECT DISTINCT
			
			CASE 	WHEN 	ISNULL(GetSuppliesSums.Betterment,0) = 1
					THEN ISNULL(GetSuppliesSums2.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
				WHEN 	ISNULL(GetSuppliesSums.CapType,2) = 1 AND ISNULL(GetSuppliesSums.Cap,0) > 0 AND
					ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums.LaborTime,0)
					THEN ISNULL(GetSuppliesSums2.Rate,0) * ISNULL(GetSuppliesSums.Cap,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
				WHEN 	ISNULL(GetSuppliesSums.CapType,2) = 0 AND ISNULL(GetSuppliesSums.Cap,0) > 0 AND
					ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums2.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
					THEN ISNULL(GetSuppliesSums.Cap,0)
		
				ELSE	ISNULL(GetSuppliesSums2.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
			END 'LineItemTotal',
		
			GetSuppliesSums2.AdminInfoID,
			GetSuppliesSums2.RateTypesID,
			GetSuppliesSums2.RateName,
			GetSuppliesSums.Betterment,
			GetSuppliesSums2.EMSCode1,
			GetSuppliesSums2.EMSBetterment,
			GetSuppliesSums2.DiscountMarkup,
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 THEN GetSuppliesSums2.CapType
				ELSE NULL
			END 'CapType', 
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 THEN GetSuppliesSums2.Cap
				ELSE 0
			END 'Cap',
			GetSuppliesSums2.Taxable,
			GetSuppliesSums2.Rate,
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 AND GetSuppliesSums.CapType = 1 AND ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums.LaborTime,0)
					THEN ISNULL(GetSuppliesSums.Cap,0)
				ELSE	ISNULL(GetSuppliesSums.LaborTime,0)
			END 'Hours',
			ISNULL(GetSuppliesSums.LaborCost,0) 'Cost'
		FROM GetSuppliesSums with(nolock)
		INNER JOIN GetSuppliesSums GetSuppliesSums2 with(nolock) ON
			(GetSuppliesSums2.AdminInfoID = GetSuppliesSums.AdminInfoID AND
			 GetSuppliesSums2.RateTypesID = GetSuppliesSums.IncludeIn)
		WHERE ISNULL(GetSuppliesSums.IncludeIn,0) > 0
		
		UNION ALL
		
		SELECT DISTINCT
			
			CASE 	WHEN 	ISNULL(GetSuppliesSums.Betterment,0) = 1 
					THEN ISNULL(GetSuppliesSums.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
	
				WHEN 	ISNULL(GetSuppliesSums.CapType,2) = 1 AND ISNULL(GetSuppliesSums.Cap,0) > 0 AND
					ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums.LaborTime,0) 
					THEN ISNULL(GetSuppliesSums.Rate,0) * ISNULL(GetSuppliesSums.Cap,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
	
				WHEN 	ISNULL(GetSuppliesSums.CapType,2) = 0 AND ISNULL(GetSuppliesSums.Cap,0) > 0 AND
					ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
					THEN ISNULL(GetSuppliesSums.Cap,0)
	
				ELSE			
					ISNULL(GetSuppliesSums.Rate,0) * ISNULL(GetSuppliesSums.LaborTime,0) + ISNULL(GetSuppliesSums.LaborCost,0) 
			END 'LineItemTotal',
			GetSuppliesSums.AdminInfoID,
			GetSuppliesSums.RateTypesID,
			GetSuppliesSums.RateName,
			GetSuppliesSums.Betterment,
			GetSuppliesSums.EMSCode1,
			GetSuppliesSums.EMSBetterment,
			GetSuppliesSums.DiscountMarkup,
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 THEN GetSuppliesSums.CapType
				ELSE NULL
			END 'CapType', 
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 THEN GetSuppliesSums.Cap
				ELSE 0
			END 'Cap',
			GetSuppliesSums.Taxable,
			GetSuppliesSums.Rate,
			CASE 	WHEN ISNULL(GetSuppliesSums.Betterment,0) = 0 AND GetSuppliesSums.CapType = 1 AND ISNULL(GetSuppliesSums.Cap,0) < ISNULL(GetSuppliesSums.LaborTime,0)
					THEN ISNULL(GetSuppliesSums.Cap,0)
				ELSE	ISNULL(GetSuppliesSums.LaborTime,0)
			END 'Hours',
			ISNULL(GetSuppliesSums.LaborCost,0) 'Cost'
		FROM GetSuppliesSums with(nolock)
		WHERE ISNULL(GetSuppliesSums.IncludeIn,0) = 0	) T
	GROUP BY
		AdminInfoID,
		RateTypesID,
		RateName,
		Betterment,
		EMSCode1,
		EMSBetterment,
		DiscountMarkup,
		CapType, 
		Cap,
		Taxable,
		Rate		) s




GO
