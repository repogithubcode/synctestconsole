USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractAddOn](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ContractID] [int] NULL,
	[ContractPriceLevelID] [int] NULL,
	[AddOnType] [tinyint] NULL,
	[StartDate] [date] NULL,
	[Active] [bit] NULL,
	[IsDeleted] [bit] NULL,
	[Quantity] [tinyint] NULL,
 CONSTRAINT [PK_ContractAddOn] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
