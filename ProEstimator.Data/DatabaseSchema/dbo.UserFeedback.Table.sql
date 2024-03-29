USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFeedback](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ActiveLoginID] [int] NOT NULL,
	[FeedbackText] [nvarchar](max) NOT NULL,
	[CreatedDate] [datetime] NOT NULL,
	[BrowserDetails] [nvarchar](2000) NULL,
	[DevicePlatform] [nvarchar](2000) NULL,
	[ActionNote] [nvarchar](max) NULL,
 CONSTRAINT [PK_UserFeedback2] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO
