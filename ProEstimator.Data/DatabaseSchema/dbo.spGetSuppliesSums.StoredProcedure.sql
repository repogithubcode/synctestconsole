USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[spGetSuppliesSums]
	@AdminInfoID Int
AS
	CREATE TABLE [dbo].[#GetSupplies] (
		[AdminInfoID] [int] NOT NULL ,
		[RateTypesID] [tinyint] NOT NULL ,
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
		[LaborTypesID] [tinyint] NULL ,
		[LaborType] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Description] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Betterment] [int] NOT NULL ,
		[Sublet] [int] NOT NULL ,
		[LaborCost] [money] NULL ,
		[LaborTime] [float] NULL ,
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[EMSSublet] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Rate] [real] NULL ,
		[CapType] [tinyint] NULL ,
		[Cap] [real] NULL ,
		[DiscountMarkup] [real] NULL ,
		[Taxable] [bit] NULL ,
		[IncludeIn] [tinyint] NULL
	) ON [PRIMARY]
	
	--INSERT INTO #GetSupplies
		EXECUTE spGetSupplies @AdminInfoID = @AdminInfoID

    IF object_id('tempdb..#GetSuppliesSums') IS NOT NULL
	BEGIN
	   INSERT INTO #GetSuppliesSums([AdminInfoID],
		[RateTypesID],
		[RateName],
		[Betterment],
		[LaborCost],
		[LaborTime],
		[EMSCode1],
		[EMSBetterment],
		[Rate],
		[CapType],
		[Cap],
		[DiscountMarkup],
		[Taxable],
		[IncludeIn])
		SELECT AdminInfoID,RateTypesID,RateName,Betterment,Sum(isnull(LaborCost,0)) LaborCost,Sum(isnull(LaborTime,0)) LaborTime,EMSCode1,EMSBetterment,Rate,CapType,Cap,DiscountMarkup,Taxable,IncludeIn
		FROM #GetSupplies
		GROUP BY AdminInfoID,RateTypesID,RateName,Betterment,EMSCode1,EMSBetterment,Rate,CapType,Cap,DiscountMarkup,Taxable,IncludeIn
		
	END
	ELSE
	BEGIN
		SELECT AdminInfoID,RateTypesID,RateName,Betterment,Sum(isnull(LaborCost,0)) LaborCost,Sum(isnull(LaborTime,0)) LaborTime,EMSCode1,EMSBetterment,Rate,CapType,Cap,DiscountMarkup,Taxable,IncludeIn
		FROM #GetSupplies
		GROUP BY AdminInfoID,RateTypesID,RateName,Betterment,EMSCode1,EMSBetterment,Rate,CapType,Cap,DiscountMarkup,Taxable,IncludeIn
	END
	DROP TABLE #GetSupplies


GO
