USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerProfileRates](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerProfilesID] [int] NOT NULL,
	[RateType] [tinyint] NOT NULL,
	[Rate] [real] NULL,
	[CapType] [tinyint] NULL,
	[Cap] [real] NULL,
	[Taxable] [bit] NULL,
	[DiscountMarkup] [real] NULL,
	[IncludeIn] [tinyint] NULL,
 CONSTRAINT [PK_CustomerProfileRates] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY],
 CONSTRAINT [IX_CustomerProfileRates] UNIQUE NONCLUSTERED 
(
	[CustomerProfilesID] ASC,
	[RateType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_CustomerProfileRates_7_638625318__K2_K3_K1_4_5_6_7_8_9] ON [dbo].[CustomerProfileRates]
(
	[CustomerProfilesID] ASC,
	[RateType] ASC,
	[id] ASC
)
INCLUDE([Rate],[CapType],[Cap],[Taxable],[DiscountMarkup],[IncludeIn]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
