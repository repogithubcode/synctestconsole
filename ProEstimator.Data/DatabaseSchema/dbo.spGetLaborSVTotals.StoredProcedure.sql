USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetLaborSVTotals]
	@AdminInfoID Int
AS
	CREATE TABLE [dbo].[#GetLaborSVSums] (
		[SupplementVersion2] [tinyint] NOT NULL ,
		[AdminInfoID] [int] NOT NULL ,
		[RateTypesID] [tinyint] NOT NULL ,
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[BettermentType] [varchar] (1) NOT NULL ,
		[BettermentValue] [float] NOT NULL,
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
	
	--INSERT INTO #GetLaborSVSums
		EXECUTE spGetLaborSVSums @AdminInfoID = @AdminInfoID

	SELECT CASE 	WHEN 	ISNULL(s.BettermentType, '') <> '' 
				THEN ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
			WHEN 	ISNULL(s.CapType,2) = 1 AND 
				ISNULL(s.Cap,0) < ISNULL(s.Hours,0) 
				THEN ISNULL(s.Rate,0) * ISNULL(s.Cap,0) + ISNULL(s.Cost,0) 
			WHEN 	ISNULL(s.CapType,2) = 0 AND 
				ISNULL(s.Cap,0) < ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
				THEN ISNULL(s.Cap,0)
			ELSE	ISNULL(s.Rate,0) * ISNULL(s.Hours,0) + ISNULL(s.Cost,0) 
		END 'LineItemTotal',
			SupplementVersion2,
			AdminInfoID,
			RateTypesID,
			RateName,
			BettermentType,
			BettermentValue,
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
		SELECT SUM(LineItemTotal) 'LineItemTotal',
			SupplementVersion2,
			AdminInfoID,
			RateTypesID,
			RateName,
			BettermentType,
			BettermentValue,
			EMSCode1,
			EMSBetterment,
			DiscountMarkup,
			CapType,
			Cap,
			Taxable,
			Rate,
			SUM(Hours) 'Hours',
			SUM(Cost) 'Cost'
		FROM (
			SELECT DISTINCT
				
				CASE 	WHEN ISNULL(GetLaborSums.BettermentType, '') <> ''  
									THEN ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 1 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0) 
									THEN ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.Cap,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 0 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
									THEN ISNULL(GetLaborSums.Cap,0)
					ELSE				ISNULL(GetLaborSums2.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
				END 'LineItemTotal',
				GetLaborSums2.SupplementVersion2,
			
				GetLaborSums2.AdminInfoID,
				GetLaborSums2.RateTypesID,
				GetLaborSums2.RateName,
				GetLaborSums.BettermentType,
				GetLaborSums.BettermentValue,
				GetLaborSums2.EMSCode1,
				GetLaborSums2.EMSBetterment,
				GetLaborSums2.DiscountMarkup,
				CASE 	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' THEN GetLaborSums2.CapType
					ELSE NULL
				END 'CapType', 
				CASE 	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' THEN GetLaborSums2.Cap
					ELSE 0
				END 'Cap',
				GetLaborSums2.Taxable,
				GetLaborSums2.Rate,
				CASE	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' AND ISNULL(GetLaborSums.CapType,2) = 1 AND ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0)
						THEN ISNULL(GetLaborSums.Cap,0)
					ELSE	ISNULL(GetLaborSums.LaborTime,0) 
				END 'Hours',
				ISNULL(GetLaborSums.LaborCost,0) 'Cost'
			FROM #GetLaborSVSums GetLaborSums
			INNER JOIN #GetLaborSVSums GetLaborSums2 ON
				(GetLaborSums2.AdminInfoID = GetLaborSums.AdminInfoID AND
				 GetLaborSums2.RateTypesID = GetLaborSums.IncludeIn)
			WHERE ISNULL(GetLaborSums.IncludeIn,0) > 0
			
			UNION ALL
			
			SELECT DISTINCT
				
				CASE 	
					WHEN ISNULL(GetLaborSums.BettermentType, '') <> ''
						THEN ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 1 AND 
						ISNULL(GetLaborSums.Cap,0) < GetLaborSums.LaborTime 
						THEN ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.Cap,0) + ISNULL(GetLaborSums.LaborCost,0) 
					WHEN 	ISNULL(GetLaborSums.CapType,2) = 0 AND 
						ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
						THEN GetLaborSums.Cap
					ELSE				ISNULL(GetLaborSums.Rate,0) * ISNULL(GetLaborSums.LaborTime,0) + ISNULL(GetLaborSums.LaborCost,0) 
				END 'LineItemTotal',
				GetLaborSums.SupplementVersion2,
			
				GetLaborSums.AdminInfoID,
				GetLaborSums.RateTypesID,
				GetLaborSums.RateName,
				GetLaborSums.BettermentType,
				GetLaborSums.BettermentValue,
				GetLaborSums.EMSCode1,
				GetLaborSums.EMSBetterment,
				GetLaborSums.DiscountMarkup,
				CASE 	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' THEN GetLaborSums.CapType
					ELSE NULL
				END 'CapType', 
				CASE 	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' THEN GetLaborSums.Cap
					ELSE 0
				END 'Cap',
				GetLaborSums.Taxable,
				GetLaborSums.Rate,
				CASE	WHEN ISNULL(GetLaborSums.BettermentType, '') = '' AND ISNULL(GetLaborSums.CapType,2) = 1 AND ISNULL(GetLaborSums.Cap,0) < ISNULL(GetLaborSums.LaborTime,0)
						THEN ISNULL(GetLaborSums.Cap,0)
					ELSE	ISNULL(GetLaborSums.LaborTime,0) 
				END 'Hours',
				ISNULL(GetLaborSums.LaborCost,0) 'Cost'
			FROM #GetLaborSVSums GetLaborSums
			WHERE ISNULL(GetLaborSums.IncludeIn,0) = 0	) T 
		GROUP BY
			AdminInfoID,
			SupplementVersion2,
			RateTypesID,
			RateName,
			BettermentType,
			BettermentValue,
			EMSCode1,
			EMSBetterment,
			DiscountMarkup,
			CapType,
			Cap,
			Taxable,	
			Rate	) s

	DROP TABLE #GetLaborSVSums


GO
