USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
 
CREATE PROCEDURE [dbo].[spGetSubTotals_v] 
	@AdminInfoID Int 
	,@SupplementVersion int = 0 
AS 
 
/* Create the Temp tables that will be used */ 
	CREATE TABLE [dbo].[#GetParts] ( 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[PartSource] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL , 
		[Type] [varchar] (56) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[Price] [money] NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[BettermentAmount] [money] NOT NULL , 
		[Sublet] [int] NOT NULL , 
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
	CREATE TABLE [dbo].[#GetLabor] ( 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[LaborTypesID] [tinyint] NULL , 
		[LaborType] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[Description] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
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
		[Taxable] [int] NOT NULL , 
		[IncludeIn] [tinyint] NULL, 
		[ActionCode] varchar(20), 
		[LaborTotal] [money] NULL 
	) ON [PRIMARY] 
	CREATE TABLE [dbo].[#GetPartsTotals] ( 
		[LineItemTotal] [real] NULL , 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[BettermentType] [varchar] (1) NOT NULL,  
		[BettermentAmount] [money] NOT NULL , 
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[DiscountMarkup] [real] NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[Taxable] [bit] NULL 
	) ON [PRIMARY] 
	CREATE TABLE [dbo].[#GetLaborSums] ( 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[LaborCost] [money] NULL , 
		[LaborTime] [float] NULL , 
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[Rate] [real] NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[DiscountMarkup] [real] NULL , 
		[Taxable] [int] NOT NULL , 
		[IncludeIn] [tinyint] NULL, 
		[LaborTotal] [money] NULL 
	) ON [PRIMARY] 
	CREATE TABLE [dbo].[#GetLaborTotals] ( 
		[LineItemTotal] [float] NULL , 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[BettermentAmount] [money] NOT NULL, 
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[DiscountMarkup] [real] NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[Taxable] [int] NOT NULL , 
		[Rate] [real] NULL , 
		[Hours] [float] NULL , 
		[Cost] [money] NULL 
	) ON [PRIMARY] 
	CREATE TABLE [dbo].[#GetSupplies] ( 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL , 
		[LaborTypesID] [tinyint] NULL , 
		[LaborType] [varchar] (20) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[Description] [varchar] (255) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
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
	CREATE TABLE [dbo].[#GetSuppliesSums] ( 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[LaborCost] [money] NULL , 
		[LaborTime] [float] NULL , 
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[Rate] [real] NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[DiscountMarkup] [real] NULL , 
		[Taxable] [bit] NULL , 
		[IncludeIn] [tinyint] NULL 
	) ON [PRIMARY] 
	CREATE TABLE [dbo].[#GetSuppliesTotals] ( 
		[LineItemTotal] [float] NULL , 
		[AdminInfoID] [int] NOT NULL , 
		[RateTypesID] [tinyint] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[DiscountMarkup] [real] NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[Taxable] [bit] NULL , 
		[Rate] [real] NULL , 
		[Hours] [float] NULL , 
		[Cost] [money] NULL 
	) ON [PRIMARY] 
		 
	CREATE TABLE [dbo].[#GetSubTotals] ( 
		[SupplementVersion2] [int] NOT NULL , 
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL , 
		[CapType] [tinyint] NULL , 
		[Cap] [real] NULL , 
		[FinalLineItemTotal] [float] NULL , 
		[AdminInfoID] [int] NOT NULL , 
		[BettermentType] [varchar] (1) NOT NULL , 
		[BettermentAmount] [money] NOT NULL,  
		[Taxable] [int] NULL , 
		[Rate] [real] NULL, 
		[Hours] [float] NULL 
	) ON [PRIMARY] 
 
	 
 
--Get Data 
	-- Total Parts  
	EXECUTE CalcParts_v @AdminInfoID,@SupplementVersion 
 
	--SELECT * FROM #GetParts 
 
	-- Get Parts Totals 
	EXECUTE CalcPartsTotals 
 
	-- Total Labor  
	EXECUTE CalcLabor_v @AdminInfoID,@SupplementVersion 
 
	-- Get Labor SUme 
	EXECUTE CalcLaborSums_v 
 
	-- Get Labor Totals 
	EXECUTE CalcLaborTotals_v 
 
	-- GetSupplies 
	EXECUTE CalcSupplies 
 
	--GetSuppliesSums 
	EXECUTE CalcSuppliesSums 
 
	--GetSuppliesTotals 
	EXECUTE CalcSuppliesTotals 
 
	--GetSubTotals 
	EXECUTE CalcSubTotals 
	 
	--SELECT * FROM #GetLabor 
	--SELECT * FROM #GetLaborSums 
	--SELECT * FROM #GetLaborTotals 
	 
	--SELECT * FROM #GetParts 
	--SELECT * FROM #GetPartsTotals 
		 
	--SELECT * FROM #GetSupplies 
	--SELECT * FROM #GetSuppliesSums 
	--SELECT * FROM #GetSuppliesTotals 
 
	--SELECT * FROM #GetSubTotals 
	 
	IF object_id('tempdb..#GetSubTotals2') IS NOT NULL 
	BEGIN print 'yes' 
	   INSERT INTO #GetSubTotals2([SupplementVersion2],[RateName],[CapType],[Cap],[FinalLineItemTotal],[AdminInfoID],[BettermentType], [BettermentAmount],[Taxable],[Rate],	[Hours]) 
	   select [SupplementVersion2],[RateName],[CapType],[Cap],[FinalLineItemTotal],[AdminInfoID],[BettermentType],[BettermentAmount],[Taxable],[Rate],	[Hours] from #GetSubTotals 
    END 
    ELSE 
    BEGIn print 'no' 
		select [SupplementVersion2],[RateName],[CapType],[Cap],[FinalLineItemTotal],[AdminInfoID],[BettermentType],[BettermentAmount],[Taxable],[Rate],[Hours] from #GetSubTotals 
    END 
 
	DROP TABLE #GetLabor 
	DROP TABLE #GetLaborSums 
	DROP TABLE #GetLaborTotals 
	DROP TABLE #GetParts 
	DROP TABLE #GetPartsTotals 
	DROP TABLE #GetSupplies 
	DROP TABLE #GetSuppliesSums 
	DROP TABLE #GetSuppliesTotals 
 
 
 
 
GO
