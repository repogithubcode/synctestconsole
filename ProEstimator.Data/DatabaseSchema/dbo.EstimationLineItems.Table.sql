USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstimationLineItems](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[EstimationDataID] [int] NOT NULL,
	[StepID] [int] NULL,
	[PartNumber] [varchar](50) NULL,
	[PartSource] [varchar](10) NULL,
	[ActionCode] [varchar](20) NULL,
	[Description] [varchar](80) NULL,
	[Price] [money] NULL,
	[Qty] [int] NULL,
	[LaborTime] [real] NULL,
	[PaintTime] [real] NULL,
	[Other] [real] NULL,
	[ImageID] [int] NULL,
	[ActionDescription] [varchar](80) NULL,
	[PartOfOverhaul] [bit] NULL,
	[PartSourceVendorID] [int] NULL,
	[BettermentParts] [bit] NULL,
	[SubletPartsFlag] [bit] NULL,
	[BettermentMaterials] [bit] NULL,
	[SubletOperationFlag] [bit] NULL,
	[SupplementVersion] [tinyint] NULL,
	[LineNumber] [int] NULL,
	[UniqueSequenceNumber] [int] NULL,
	[ModifiesID] [int] NULL,
	[ACDCode] [char](1) NULL,
	[CustomerPrice] [real] NULL,
	[AutomaticCharge] [bit] NULL,
	[SourcePartNumber] [varchar](25) NULL,
	[SectionID] [int] NULL,
	[VehiclePosition] [varchar](5) NULL,
	[Barcode] [varchar](10) NULL,
	[dbPrice] [money] NULL,
	[AutoAdd] [bit] NULL,
	[Suppress] [bit] NULL,
	[AutoAddBarcodeParent] [varchar](10) NULL,
	[Date_Entered] [datetime] NOT NULL,
	[BettermentType] [char](10) NULL,
	[BettermentValue] [float] NULL,
	[IsPartsQuantity] [bit] NULL,
	[IsLaborQuantity] [bit] NULL,
	[IsPaintQuantity] [bit] NULL,
	[PresetShellID] [int] NULL,
	[ParentLineID] [int] NULL,
	[IsOtherCharges] [bit] NULL,
	[LaborIncluded] [bit] NULL,
 CONSTRAINT [PK_EstimationLineItems] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationLineItems_6_1614628795__K45] ON [dbo].[EstimationLineItems]
(
	[ParentLineID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationLineItems_7_1614628795__K2_K1_21] ON [dbo].[EstimationLineItems]
(
	[EstimationDataID] ASC,
	[id] ASC
)
INCLUDE([SupplementVersion]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationLineItems_7_1614628795__K24_K2_K1_K32] ON [dbo].[EstimationLineItems]
(
	[ModifiesID] ASC,
	[EstimationDataID] ASC,
	[id] ASC,
	[Barcode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationLineItems_9_1614628795__K1_K22_K2] ON [dbo].[EstimationLineItems]
(
	[id] ASC,
	[LineNumber] ASC,
	[EstimationDataID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationLineItems_9_1614628795__K2_K1_K22] ON [dbo].[EstimationLineItems]
(
	[EstimationDataID] ASC,
	[id] ASC,
	[LineNumber] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [EstimationDataID] ON [dbo].[EstimationLineItems]
(
	[EstimationDataID] DESC,
	[Barcode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [EstimationDataID_ActionCode] ON [dbo].[EstimationLineItems]
(
	[EstimationDataID] ASC,
	[ActionCode] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [id_Modifiesid] ON [dbo].[EstimationLineItems]
(
	[id] ASC,
	[ModifiesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_EstimationLineItems_EstimationDataID] ON [dbo].[EstimationLineItems]
(
	[EstimationDataID] ASC
)
INCLUDE([SupplementVersion],[ModifiesID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ModifiesID] ON [dbo].[EstimationLineItems]
(
	[ModifiesID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [ModifiesID desc] ON [dbo].[EstimationLineItems]
(
	[ModifiesID] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  CONSTRAINT [DF_EstimationLineItems_Qty]  DEFAULT ((1)) FOR [Qty]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  CONSTRAINT [DF_EstimationLineItems_AutomaticCharge]  DEFAULT (0) FOR [AutomaticCharge]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  CONSTRAINT [DF_EstimationLineItems_SourcePartNumber]  DEFAULT ('') FOR [SourcePartNumber]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  CONSTRAINT [DF_EstimationLineItems_AutoAdd]  DEFAULT (0) FOR [AutoAdd]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  CONSTRAINT [DF_EstimationLineItems_Suppress]  DEFAULT (0) FOR [Suppress]
GO
ALTER TABLE [dbo].[EstimationLineItems] ADD  DEFAULT (getdate()) FOR [Date_Entered]
GO
EXEC sys.sp_addextendedproperty @name=N'MS_Description', @value=N'Add/Change/Delete for Supplements' , @level0type=N'SCHEMA',@level0name=N'dbo', @level1type=N'TABLE',@level1name=N'EstimationLineItems', @level2type=N'COLUMN',@level2name=N'ACDCode'
GO
