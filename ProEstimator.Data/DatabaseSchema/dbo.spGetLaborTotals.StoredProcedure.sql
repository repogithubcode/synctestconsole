USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spGetLaborTotals]
	@AdminInfoID Int
AS

	CREATE TABLE [dbo].[#GetLaborSums] (
		[AdminInfoID] [int] NOT NULL ,
		[RateTypesID] [tinyint] NOT NULL ,
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Betterment] [int] NOT NULL ,
		[LaborCost] [money] NULL ,
		[LaborTime] [float] NULL ,
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Rate] [real] NULL ,
		[CapType] [tinyint] NULL ,
		[Cap] [real] NULL ,
		[DiscountMarkup] [real] NULL ,
		[Taxable] [int] NOT NULL ,
		[IncludeIn] [tinyint] NULL
	) ON [PRIMARY]

	--INSERT INTO #GetLaborSums
		EXECUTE spGetLaborSums @AdminInfoID = @AdminInfoID

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
				
				CASE 	WHEN 	ISNULL(GetLaborSums.Betterment,0) = 1 
									THEN ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 1 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0) 
									THEN ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.Cap,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 0 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
									THEN ISNULL(GetLaborSums.Cap,0)
					ELSE				ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
				END 'LineItemTotal',
			
				GetLaborSums2.AdminInfoID,
				GetLaborSums2.RateTypesID,
				GetLaborSums2.RateName,
				GetLaborSums.Betterment,
				GetLaborSums2.EMSCode1,
				GetLaborSums2.EMSBetterment,
				GetLaborSums2.DiscountMarkup,
				CASE 	WHEN ISNULL(GetLaborSums.Betterment,0) = 0 THEN GetLaborSums2.CapType
					ELSE NULL
				END 'CapType', 
				CASE 	WHEN ISNULL(GetLaborSums.Betterment,0) = 0 THEN GetLaborSums2.Cap
					ELSE 0
				END 'Cap',
				GetLaborSums2.Taxable,
				GetLaborSums2.Rate,
				CASE	WHEN 	ISNULL(GetLaborSums.Betterment,0) = 0 AND ISNULL(GetLaborSums.CapType,2) = 1 AND ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0)
						THEN ISNULL(GetLaborSums.Cap,0)
					ELSE	ISNULL(GetLaborSums.LaborTime,0) 
				END 'Hours',
				ISNULL(GetLaborSums.LaborCost,0) 'Cost'
			FROM #GetLaborSums GetLaborSums
			INNER JOIN #GetLaborSums GetLaborSums2 ON
				(GetLaborSums2.AdminInfoID = GetLaborSums.AdminInfoID AND
				 GetLaborSums2.RateTypesID = GetLaborSums.IncludeIn)
			WHERE ISNULL(GetLaborSums.IncludeIn,0) > 0
			
			UNION ALL
			
			SELECT DISTINCT
				
				CASE 	WHEN 	ISNULL(GetLaborSums.Betterment,0) = 1 
						THEN ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 1 AND 
						ISNULL(GetLaborSums.Cap,0) < GetLaborSums.LaborTime 
						THEN ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.Cap,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 0 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
						THEN GetLaborSums.Cap
					ELSE				ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
				END 'LineItemTotal',
			
				GetLaborSums.AdminInfoID,
				GetLaborSums.RateTypesID,
				GetLaborSums.RateName,
				GetLaborSums.Betterment,
				GetLaborSums.EMSCode1,
				GetLaborSums.EMSBetterment,
				GetLaborSums.DiscountMarkup,
				CASE 	WHEN ISNULL(GetLaborSums.Betterment,0) = 0 THEN GetLaborSums.CapType
					ELSE NULL
				END 'CapType', 
				CASE 	WHEN ISNULL(GetLaborSums.Betterment,0) = 0 THEN GetLaborSums.Cap
					ELSE 0
				END 'Cap',
				GetLaborSums.Taxable,
				GetLaborSums.Rate,
				CASE	WHEN 	ISNULL(GetLaborSums.Betterment,0) = 0 AND ISNULL(GetLaborSums.CapType,2) = 1 AND ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0)
						THEN ISNULL(GetLaborSums.Cap,0)
					ELSE	ISNULL(GetLaborSums.LaborTime,0) 
				END 'Hours',
				ISNULL(GetLaborSums.LaborCost,0) 'Cost'
			FROM #GetLaborSums GetLaborSums
			WHERE ISNULL(GetLaborSums.IncludeIn,0) = 0	) T 
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
			Rate	) s

	DROP TABLE #GetLaborSums


GO
