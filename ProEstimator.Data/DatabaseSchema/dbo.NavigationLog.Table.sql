USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[NavigationLog](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
	[ControlButton] [varchar](50) NULL,
	[TimeOccurred] [datetime] NULL,
 CONSTRAINT [PK_NavigationLog] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[NavigationLog] ADD  CONSTRAINT [DF_NavigationLog_TimeOccurred]  DEFAULT (getdate()) FOR [TimeOccurred]
GO
