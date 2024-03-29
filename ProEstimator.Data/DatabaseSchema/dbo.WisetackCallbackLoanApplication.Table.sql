USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[WisetackCallbackLoanApplication](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[TransactionID] [nvarchar](200) NOT NULL,
	[CallbackDate] [datetime] NOT NULL,
	[MessageID] [nvarchar](200) NULL,
	[ChangedStatus] [nvarchar](100) NOT NULL,
	[ActionsRequired] [nvarchar](2000) NULL,
	[RequestedLoanAmount] [nvarchar](100) NOT NULL,
	[ApprovedLoanAmount] [nvarchar](100) NULL,
	[SettledLoanAmount] [nvarchar](100) NULL,
	[ProcessingFee] [nvarchar](100) NULL,
	[MaximumAmount] [nvarchar](100) NULL,
	[TransactionPurpose] [nvarchar](200) NULL,
	[ServiceCompletedOn] [datetime] NOT NULL,
	[TilaAcceptedOn] [datetime] NULL,
	[LoanCreatedAt] [datetime] NULL,
	[EventType] [nvarchar](100) NULL,
	[ExpirationDate] [datetime] NULL,
	[PrequalID] [nvarchar](200) NULL,
	[CustomerID] [nvarchar](200) NULL,
 CONSTRAINT [PK_WisetackCallbackLoanApplication] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [idx_WisetackCallbackLoanApplication_TransactionID] ON [dbo].[WisetackCallbackLoanApplication]
(
	[TransactionID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
