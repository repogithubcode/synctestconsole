USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContactItemTypes](
	[id] [smallint] NOT NULL,
	[Type] [varchar](25) NOT NULL,
	[Description] [varchar](25) NOT NULL,
	[SortOrder] [tinyint] NULL,
	[Printable] [bit] NULL
) ON [PRIMARY]
GO
