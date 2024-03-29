USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[ContractPriceLevels](
	[ContractPriceLevelID] [int] IDENTITY(1,1) NOT NULL,
	[PriceLevel] [int] NOT NULL,
	[ContractTermID] [int] NOT NULL,
	[PaymentAmount] [decimal](9, 2) NOT NULL,
	[Active] [bit] NOT NULL,
 CONSTRAINT [PK_ContractPriceLevels] PRIMARY KEY CLUSTERED 
(
	[ContractPriceLevelID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY],
 CONSTRAINT [UQ_ContractPriceLevels] UNIQUE NONCLUSTERED 
(
	[PriceLevel] ASC,
	[ContractTermID] ASC,
	[PaymentAmount] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
ALTER TABLE [dbo].[ContractPriceLevels] ADD  CONSTRAINT [DF_ContractPriceLevels_Active]  DEFAULT (1) FOR [Active]
GO
