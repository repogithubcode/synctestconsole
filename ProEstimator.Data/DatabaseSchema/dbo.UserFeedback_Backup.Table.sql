USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[UserFeedback_Backup](
	[Name] [nvarchar](50) NULL,
	[Subject] [nvarchar](50) NULL,
	[ContactNo] [nvarchar](50) NULL,
	[Email] [nvarchar](50) NULL,
	[Description] [nvarchar](2000) NULL,
	[LoginID] [int] NOT NULL,
	[MainModule] [nvarchar](50) NULL,
	[SubModule] [nvarchar](500) NULL,
	[CreatedDate] [datetime] NULL,
	[IsActive] [bit] NULL,
	[BrowserDetails] [nvarchar](2000) NULL,
	[DevicePlatform] [nvarchar](2000) NULL,
	[Url] [nvarchar](500) NULL,
	[FeedbackID] [bigint] IDENTITY(1,1) NOT NULL,
	[UserID] [int] NULL,
 CONSTRAINT [PK_UserFeedback] PRIMARY KEY CLUSTERED 
(
	[FeedbackID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[UserFeedback_Backup]  WITH CHECK ADD  CONSTRAINT [FK_UserFeedback_UserFeedback] FOREIGN KEY([FeedbackID])
REFERENCES [dbo].[UserFeedback_Backup] ([FeedbackID])
GO
ALTER TABLE [dbo].[UserFeedback_Backup] CHECK CONSTRAINT [FK_UserFeedback_UserFeedback]
GO
