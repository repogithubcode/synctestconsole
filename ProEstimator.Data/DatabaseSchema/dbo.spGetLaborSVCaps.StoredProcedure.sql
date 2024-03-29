USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetLaborSVCaps]
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

	SELECT DISTINCT
		GetLaborSums.SupplementVersion2,
		GetLaborSums.RateName + ' Capped at ' +
		CASE	WHEN ISNULL(GetLaborSums.CapType,2) = 1 THEN Dbo.FormatNumber(GetLaborSums.Cap,1) + ' Hour(s)'
			WHEN ISNULL(GetLaborSums.CapType,2) = 0 THEN Dbo.FormatMoney(GetLaborSums.Cap)
		END 'CapInfo',
		GetLaborSums.AdminInfoID 
	FROM #GetLaborSVSums GetLaborSums
	INNER JOIN GetLaborSVSums GetLaborSums2 ON
		(GetLaborSums2.AdminInfoID = GetLaborSums.AdminInfoID AND
		 GetLaborSums2.RateTypesID = GetLaborSums.IncludeIn)
	WHERE 	ISNULL(GetLaborSums.Betterment,0) = 0 AND
		( ( ISNULL(GetLaborSums.CapType,2) = 1 AND GetLaborSums.Cap < GetLaborSums.LaborTime ) OR
		  ( ISNULL(GetLaborSums.CapType,2) = 0 AND GetLaborSums.Cap < ISNULL(GetLaborSums.LaborTime,0) * ISNULL(GetLaborSums2.Rate,0) + ISNULL(GetLaborSums.LaborCost,0) ) 
		) AND
		ISNULL(GetLaborSums.IncludeIn,0) > 0

	DROP TABLE #GetLaborSVSums


GO
