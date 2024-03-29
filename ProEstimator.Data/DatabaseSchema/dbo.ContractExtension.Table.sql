USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractExtension](
	[ContractExtID] [int] IDENTITY(1,1) NOT NULL,
	[LoginID] [int] NULL,
	[SalesRep] [int] NULL,
	[extendFrom] [datetime] NULL,
	[Days] [int] NULL,
	[ExtendTo] [datetime] NULL,
	[ExtendedDate] [datetime] NOT NULL,
	[TrialID] [int] NULL,
	[ContractID] [int] NULL
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_ContractExtension_LoginID] ON [dbo].[ContractExtension]
(
	[LoginID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ContractExtension] ADD  CONSTRAINT [DF_ContractExtension_ExtendedDate]  DEFAULT (getdate()) FOR [ExtendedDate]
GO
