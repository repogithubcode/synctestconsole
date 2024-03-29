USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportEmail](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[SentStamp] [datetime] NULL,
	[DeleteStamp] [datetime] NULL,
	[EstimateID] [int] NULL,
	[Subject] [varchar](500) NULL,
	[Body] [text] NULL,
	[ToAddresses] [varchar](500) NULL,
	[CCAddresses] [varchar](500) NULL,
	[PhoneNumbers] [varchar](500) NULL,
	[Errors] [varchar](2000) NULL,
	[EmailID] [int] NULL,
 CONSTRAINT [PK_Email] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_ReportEmail_6_2133462570__K4_K2_1_3_5_7_8_9_10] ON [dbo].[ReportEmail]
(
	[EstimateID] ASC,
	[SentStamp] ASC
)
INCLUDE([ID],[DeleteStamp],[Subject],[ToAddresses],[CCAddresses],[PhoneNumbers],[Errors]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
