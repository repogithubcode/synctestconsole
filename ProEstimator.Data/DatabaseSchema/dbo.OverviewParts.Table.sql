USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OverviewParts](
	[AdminInfoID] [int] NOT NULL,
	[RateTypesID] [tinyint] NULL,
	[RateName] [varchar](25) NULL,
	[PartSource] [varchar](10) NOT NULL,
	[Type] [varchar](56) NULL,
	[Price] [money] NULL,
	[Betterment] [int] NOT NULL,
	[Sublet] [int] NOT NULL,
	[EMSCode1] [varchar](10) NULL,
	[EMSBetterment] [varchar](10) NULL,
	[EMSSublet] [varchar](10) NULL,
	[Rate] [real] NULL,
	[CapType] [tinyint] NULL,
	[Cap] [real] NULL,
	[DiscountMarkup] [real] NULL,
	[Taxable] [bit] NULL,
	[IncludeIn] [tinyint] NULL
) ON [PRIMARY]
GO
