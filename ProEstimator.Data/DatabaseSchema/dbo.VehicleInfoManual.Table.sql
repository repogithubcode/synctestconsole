USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleInfoManual](
	[VehicleInfoID] [int] NOT NULL,
	[Country] [varchar](50) NULL,
	[Manufacturer] [varchar](50) NULL,
	[Make] [varchar](50) NULL,
	[Model] [varchar](50) NULL,
	[ModelYear] [varchar](50) NULL,
	[SubModel] [varchar](50) NULL,
	[Service_Barcode] [varchar](10) NULL,
	[ManualSelection] [bit] NULL,
 CONSTRAINT [PK_VehicleInfoManual] PRIMARY KEY CLUSTERED 
(
	[VehicleInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_VehicleInfoManual_6_2025774274__K1_4_5_6_7_9] ON [dbo].[VehicleInfoManual]
(
	[VehicleInfoID] ASC
)
INCLUDE([Make],[Model],[ModelYear],[SubModel],[ManualSelection]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
