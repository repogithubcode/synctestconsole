USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[TimedEventData](
	[EventName] [varchar](100) NULL,
	[LastExecution] [datetime] NULL
) ON [PRIMARY]
GO
