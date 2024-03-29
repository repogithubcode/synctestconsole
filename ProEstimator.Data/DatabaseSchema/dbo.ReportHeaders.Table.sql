USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ReportHeaders](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[Header] [varchar](100) NULL,
	[SortOrder] [int] NULL,
	[ReportType] [varchar](50) NULL,
 CONSTRAINT [PK_ReportHeaders] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
