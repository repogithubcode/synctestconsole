USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Panels](
	[ID] [int] NOT NULL,
	[PanelName] [varchar](100) NULL,
	[SortOrder] [int] NULL,
	[Symmetry] [bit] NULL,
	[SectionLinkRuleID] [int] NULL,
	[PrimarySectionLinkRuleID] [int] NULL,
	[PrimaryPanelLinkRuleID] [int] NULL
) ON [PRIMARY]
GO
