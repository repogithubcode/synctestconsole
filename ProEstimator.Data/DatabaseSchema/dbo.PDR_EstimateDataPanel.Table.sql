USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[PDR_EstimateDataPanel](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[AdminInfoID] [int] NULL,
	[PanelID] [int] NULL,
	[QuantityID] [int] NULL,
	[SizeID] [int] NULL,
	[OversizedDents] [int] NULL,
	[Multiplier] [int] NULL,
	[Description] [varchar](300) NULL,
	[Expanded] [bit] NULL,
	[LineNumber] [int] NULL,
	[CustomCharge] [money] NULL,
 CONSTRAINT [PK_PDR_EstimateDataPanel] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PDR_EstimateDataPanel_AdminInfoID] ON [dbo].[PDR_EstimateDataPanel]
(
	[AdminInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_PDR_EstimateDataPanel_AdminInfoID_OversizedDents] ON [dbo].[PDR_EstimateDataPanel]
(
	[AdminInfoID] ASC,
	[OversizedDents] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
