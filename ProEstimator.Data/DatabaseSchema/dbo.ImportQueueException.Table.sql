USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ImportQueueException](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoginId] [int] NULL,
	[Message] [nvarchar](max) NULL,
	[CreateDate] [datetime] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
