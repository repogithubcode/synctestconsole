USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Log](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[TimeDate] [datetime] NOT NULL,
	[UserID] [int] NULL,
	[URL] [varchar](200) NULL,
	[HTTP_USER_AGENT] [varchar](250) NULL,
	[SERVER_ADDR] [varchar](25) NULL,
	[USER_ADDR] [varchar](25) NULL,
	[USER_HOST] [varchar](100) NULL
) ON [PRIMARY]
GO
