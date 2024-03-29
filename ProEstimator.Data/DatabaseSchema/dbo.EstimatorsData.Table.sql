USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstimatorsData](
	[EstimatorID] [int] IDENTITY(1,1) NOT NULL,
	[AdminInfoID] [int] NULL,
	[AuthorFirstName] [varchar](50) NULL,
	[AuthorLastName] [varchar](50) NULL,
	[LoginID] [int] NULL,
	[OrderNo] [int] NULL,
	[Email] [nvarchar](150) NULL,
	[Phone] [nvarchar](100) NULL
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [_dta_index_EstimatorsData_c_9_1977838904__K1] ON [dbo].[EstimatorsData]
(
	[EstimatorID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimatorsData_7_1977838904__K5_K6_1_2_3_4] ON [dbo].[EstimatorsData]
(
	[LoginID] ASC,
	[OrderNo] ASC
)
INCLUDE([EstimatorID],[AdminInfoID],[AuthorFirstName],[AuthorLastName]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [Admin] ON [dbo].[EstimatorsData]
(
	[AdminInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
