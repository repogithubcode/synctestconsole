USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AdminInfo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CreatorID] [int] NOT NULL,
	[Description] [varchar](255) NOT NULL,
	[CustomerProfilesID] [int] NULL,
	[GrandTotal] [varchar](20) NULL,
	[BettermentTotal] [varchar](20) NULL,
	[EstimateNumber] [int] NULL,
	[WorkOrderNumber] [int] NULL,
	[PrintDescription] [bit] NULL,
	[Archived] [bit] NULL,
	[Deleted] [bit] NOT NULL,
	[ClaimNumber] [varchar](50) NULL,
	[LastView] [datetime] NULL,
	[IsImported] [bit] NULL,
	[CustomerID] [int] NULL,
	[AddOnProfileID] [int] NULL,
	[WisetackTransactionID] [nvarchar](200) NULL,
	[WisetackPaymentLink] [nvarchar](200) NULL,
	[WisetackAmountToFinance] [nvarchar](100) NULL,
	[WisetackEstimatedDateOfCompletion] [datetime] NULL,
	[WisetackLoanAppMobileNumber] [nvarchar](50) NULL,
 CONSTRAINT [PK_AdminInfo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [idx_AdminInfo_WisetackTransactionID] ON [dbo].[AdminInfo]
(
	[WisetackTransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_AdminInfo] ON [dbo].[AdminInfo]
(
	[CreatorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 100) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ix_dba_CreatorID_Deleted] ON [dbo].[AdminInfo]
(
	[CreatorID] ASC,
	[Deleted] ASC
)
INCLUDE([Description],[CustomerProfilesID],[GrandTotal],[BettermentTotal],[EstimateNumber],[WorkOrderNumber],[Archived],[ClaimNumber],[LastView],[CustomerID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[AdminInfo] ADD  CONSTRAINT [DF_AdminInfo_PrintDescription]  DEFAULT (0) FOR [PrintDescription]
GO
ALTER TABLE [dbo].[AdminInfo] ADD  CONSTRAINT [DF_AdminInfo_Archived]  DEFAULT (0) FOR [Archived]
GO
ALTER TABLE [dbo].[AdminInfo] ADD  CONSTRAINT [DF_AdminInfo_Deleted]  DEFAULT ((0)) FOR [Deleted]
GO
