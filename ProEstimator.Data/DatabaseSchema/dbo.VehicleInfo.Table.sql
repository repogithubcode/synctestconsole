USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleInfo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[EstimationDataId] [int] NOT NULL,
	[VehicleID] [int] NULL,
	[AdditionalOptions] [varchar](1000) NULL,
	[Color] [varchar](100) NULL,
	[ColorCode] [varchar](25) NULL,
	[MilesIn] [decimal](9, 1) NULL,
	[MilesOut] [decimal](9, 1) NULL,
	[License] [varchar](15) NULL,
	[State] [varchar](20) NULL,
	[Condition] [varchar](500) NULL,
	[ExtColor] [varchar](100) NULL,
	[ExtColorCode] [int] NULL,
	[IntColor] [varchar](100) NULL,
	[IntColorCode] [int] NULL,
	[TrimLevel] [varchar](20) NULL,
	[Vin] [varchar](25) NULL,
	[VinDecode] [varchar](2000) NULL,
	[InspectionDate] [datetime] NULL,
	[ExtColorCodeChar] [varchar](25) NULL,
	[IntColorCodeChar] [varchar](25) NULL,
	[ProductionDate] [varchar](15) NULL,
	[BodyType] [int] NULL,
	[Service_Barcode] [varchar](10) NULL,
	[DefaultPaintType] [int] NULL,
	[DriveType] [int] NULL,
	[VehicleValue] [money] NULL,
	[Year] [int] NULL,
	[MakeID] [int] NULL,
	[ModelID] [int] NULL,
	[SubModelID] [int] NULL,
	[EngineType] [int] NULL,
	[TransmissionType] [int] NULL,
	[paintcode] [nvarchar](50) NULL,
	[POI_ID] [int] NULL,
	[StockNumber] [varchar](50) NULL,
	[POILabel1] [tinyint] NULL,
	[POIOption1] [tinyint] NULL,
	[POICustom1] [varchar](500) NULL,
	[POILabel2] [tinyint] NULL,
	[POIOption2] [tinyint] NULL,
	[POICustom2] [varchar](500) NULL,
	[ExtColor2] [varchar](100) NULL,
	[PaintCode2] [varchar](50) NULL,
 CONSTRAINT [PK_VehicleInfo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [_dta_index_VehicleInfo_6_1957582012__K2_K17_K1_K36_3_9_27] ON [dbo].[VehicleInfo]
(
	[EstimationDataId] ASC,
	[Vin] ASC,
	[id] ASC,
	[StockNumber] ASC
)
INCLUDE([VehicleID],[License],[VehicleValue]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_VehicleInfo_7_1957582012__K2_K1_3_4_5_6_7_8_9_10_11_12_13_14_15_16_17_18_19_20_21_22_23_24_25_26_27_28_29_30_31_32_] ON [dbo].[VehicleInfo]
(
	[EstimationDataId] ASC,
	[id] ASC
)
INCLUDE([VehicleID],[AdditionalOptions],[Color],[ColorCode],[MilesIn],[MilesOut],[License],[State],[Condition],[ExtColor],[ExtColorCode],[IntColor],[IntColorCode],[TrimLevel],[Vin],[VinDecode],[InspectionDate],[ExtColorCodeChar],[IntColorCodeChar],[ProductionDate],[BodyType],[Service_Barcode],[DefaultPaintType],[DriveType],[VehicleValue],[Year],[MakeID],[ModelID],[SubModelID],[EngineType],[TransmissionType],[paintcode],[POI_ID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [VehicleInfo5] ON [dbo].[VehicleInfo]
(
	[EstimationDataId] ASC,
	[VehicleID] ASC,
	[Vin] ASC,
	[BodyType] ASC,
	[DriveType] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
GO
