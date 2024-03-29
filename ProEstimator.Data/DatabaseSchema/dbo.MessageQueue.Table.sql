USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[MessageQueue](
	[msgid] [int] IDENTITY(1,1) NOT NULL,
	[msgType] [int] NOT NULL,
	[msgValue1] [varchar](50) NULL,
	[msgValue2] [varchar](50) NULL,
	[msgValue3] [varchar](50) NULL,
	[msgValue4] [varchar](50) NULL,
	[msgRequestQID] [int] NULL,
	[msgAddDate] [datetime] NOT NULL,
	[msgAddBy] [int] NOT NULL,
	[msgChgDate] [datetime] NULL,
	[msgChgBy] [int] NULL,
	[msgRowVersion] [timestamp] NOT NULL,
	[msgDeleteDate] [datetime] NULL,
	[msgStatus] [int] NOT NULL,
	[msgStatusLong]  AS (case [msgStatus] when 1 then 'Queued' when 2 then 'In Progress' when 3 then 'Complete' else 'Error' end),
	[msgViewable] [bit] NOT NULL,
	[msgFileDeleted] [bit] NOT NULL,
 CONSTRAINT [PK_MessageQueue] PRIMARY KEY CLUSTERED 
(
	[msgid] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_MessageQueue_9_807725980__K7_K1_3_4_5_6] ON [dbo].[MessageQueue]
(
	[msgRequestQID] ASC,
	[msgid] ASC
)
INCLUDE([msgValue1],[msgValue2],[msgValue3],[msgValue4]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MessageQueue] ON [dbo].[MessageQueue]
(
	[msgDeleteDate] ASC,
	[msgType] ASC,
	[msgAddDate] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_MessageQueue_1] ON [dbo].[MessageQueue]
(
	[msgRequestQID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[MessageQueue] ADD  CONSTRAINT [DF_MessageQueue_msgAddDate]  DEFAULT (getdate()) FOR [msgAddDate]
GO
ALTER TABLE [dbo].[MessageQueue] ADD  CONSTRAINT [DF_MessageQueue_msgAddBy]  DEFAULT (99999) FOR [msgAddBy]
GO
ALTER TABLE [dbo].[MessageQueue] ADD  CONSTRAINT [DF_MessageQueue_msgStatus]  DEFAULT (1) FOR [msgStatus]
GO
ALTER TABLE [dbo].[MessageQueue] ADD  CONSTRAINT [DF_MessageQueue_msgViewable]  DEFAULT (1) FOR [msgViewable]
GO
ALTER TABLE [dbo].[MessageQueue] ADD  CONSTRAINT [DF_MessageQueue_msgFileDeleted]  DEFAULT (0) FOR [msgFileDeleted]
GO
