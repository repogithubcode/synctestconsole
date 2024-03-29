USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[StartMenu](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[ParentID] [int] NOT NULL,
	[ItemOrder] [tinyint] NULL,
	[DisplayText] [nvarchar](100) NOT NULL,
	[ClickAction] [varchar](100) NOT NULL,
	[ShowWhenNoEstimate] [bit] NOT NULL,
	[ShowOnlyWhenNoEstimate] [bit] NULL,
	[AdminRequired] [bit] NULL,
	[FWAdminRequired] [bit] NULL,
	[CSS] [varchar](50) NULL,
	[CSSOver] [varchar](50) NULL,
	[CSSSelected] [varchar](50) NULL,
	[ProfileRequired] [bit] NULL,
	[Language_ID] [int] NOT NULL,
 CONSTRAINT [PK_StartMenu] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[StartMenu] ADD  CONSTRAINT [DF_StartMenu_ShowWhenNoEstimate]  DEFAULT ((1)) FOR [ShowWhenNoEstimate]
GO
ALTER TABLE [dbo].[StartMenu] ADD  CONSTRAINT [DF_StartMenu_FWAdminRequired]  DEFAULT ((0)) FOR [FWAdminRequired]
GO
ALTER TABLE [dbo].[StartMenu] ADD  CONSTRAINT [DF_StartMenu_ProfileRequired]  DEFAULT ((0)) FOR [ProfileRequired]
GO
ALTER TABLE [dbo].[StartMenu] ADD  CONSTRAINT [DF_StartMenu_Language_ID]  DEFAULT ((1)) FOR [Language_ID]
GO
