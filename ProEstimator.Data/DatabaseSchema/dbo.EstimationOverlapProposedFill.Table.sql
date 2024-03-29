USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstimationOverlapProposedFill](
	[ActionPart] [varchar](200) NULL,
	[Reason] [varchar](100) NULL,
	[BecauseOf] [varchar](200) NULL,
	[OverlapAmount] [varchar](8) NULL,
	[MinimumLabor] [varchar](8) NULL,
	[Labor Hours] [real] NULL,
	[Labor Hours Inc] [real] NULL,
	[ID] [int] NOT NULL,
	[Process] [varchar](50) NULL,
	[AdminInfoID] [int] NULL,
	[ActionID] [int] NULL,
	[ResultID] [int] NULL,
	[ActionCode] [int] NULL,
	[AcceptFlag] [bit] NULL,
	[BarcodeInc] [varchar](10) NULL
) ON [PRIMARY]
GO
