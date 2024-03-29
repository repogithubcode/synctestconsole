USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[GridColumnInfoLoginMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[GridColumnInfoID] [int] NOT NULL,
	[LoginID] [int] NOT NULL,
	[Visible] [bit] NOT NULL,
	[SortOrderIndex] [int] NULL,
 CONSTRAINT [PK_GridColumnInfoLoginMapping] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[GridColumnInfoLoginMapping]  WITH CHECK ADD  CONSTRAINT [FK_GridColumnInfoGridColumnInfoLoginMapping] FOREIGN KEY([GridColumnInfoID])
REFERENCES [dbo].[GridColumnInfo] ([ID])
GO
ALTER TABLE [dbo].[GridColumnInfoLoginMapping] CHECK CONSTRAINT [FK_GridColumnInfoGridColumnInfoLoginMapping]
GO
