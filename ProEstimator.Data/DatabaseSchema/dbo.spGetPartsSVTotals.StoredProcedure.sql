USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[spGetPartsSVTotals]
	@AdminInfoID Int
AS
	CREATE TABLE [dbo].[#GetPartsSV] (
		[SupplementVersion2] [tinyint] NOT NULL ,
		[AdminInfoID] [int] NOT NULL ,
		[RateTypesID] [tinyint] NULL ,
		[RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[PartSource] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NOT NULL ,
		[Type] [varchar] (56) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Price] [money] NULL ,		
		[Sublet] [int] NOT NULL ,
		[EMSCode1] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[EMSBetterment] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[EMSSublet] [varchar] (10) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,
		[Rate] [real] NULL ,
		[CapType] [tinyint] NULL ,
		[Cap] [real] NULL ,
		[DiscountMarkup] [real] NULL ,
		[Taxable] [bit] NULL ,
		[IncludeIn] [tinyint] NULL,
		[BettermentType] [varchar] (1) NOT NULL ,
		[BettermentValue] [float] NOT NULL
	) ON [PRIMARY]

	INSERT INTO #GetPartsSV
		EXECUTE spGetPartsSV @AdminInfoID = @AdminInfoID
		
	SELECT	SupplementVersion2,
		CASE 	WHEN ISNULL(CapType,2) = 0 AND Cap > Price AND ISNULL(BettermentType, '') = '' THEN Cap
			ELSE Price
		END 'LineItemTotal',
		AdminInfoID, RateTypesID, RateName, ISNULL(BettermentType, ''), EMSCode1, EMSBetterment, DiscountMarkup,
		CASE 	WHEN ISNULL(BettermentType, '') = '' THEN CapType
			ELSE NULL
		END 'CapType', 
		CASE 	WHEN ISNULL(BettermentType, '') = '' THEN Cap
			ELSE 0
		END 'Cap', 
		Taxable
	FROM #GetPartsSV GetPartsSV

	DROP TABLE #GetPartsSV


GO
