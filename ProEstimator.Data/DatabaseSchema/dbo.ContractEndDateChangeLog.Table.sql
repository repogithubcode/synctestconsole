USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractEndDateChangeLog](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ContractID] [int] NULL,
	[TrialID] [int] NULL,
	[OldDate] [date] NULL,
	[NewDate] [date] NULL,
	[SalesRepID] [int] NULL,
	[TimeStamp] [datetime] NULL,
 CONSTRAINT [PK_ContractEndDateChangeLog] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
