USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerProfilePrint](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerProfilesID] [int] NOT NULL,
	[GraphicsQuality] [tinyint] NOT NULL,
	[NoHeaderLogo] [bit] NULL,
	[NoInsuranceSection] [bit] NULL,
	[NoPhotos] [bit] NULL,
	[FooterText] [varchar](8000) NULL,
	[PrintPrivateNotes] [bit] NULL,
	[PrintPublicNotes] [bit] NULL,
	[ContactOption] [varchar](20) NULL,
	[SupplementOption] [varchar](20) NULL,
	[OrderBy] [varchar](20) NULL,
	[UseBigLetters] [bit] NULL,
	[SeparateLabor] [bit] NULL,
	[EstimateNumber] [bit] NULL,
	[AdminInfoID] [bit] NULL,
	[Dollars] [bit] NULL,
	[GreyBars] [bit] NULL,
	[NoVehicleAccessories] [varchar](5000) NULL,
	[PrintPaymentInfo] [bit] NULL,
	[PrintEstimator] [bit] NULL,
	[PrintVendors] [bit] NULL,
	[NoFooterDateTimeStamp] [bit] NULL,
 CONSTRAINT [PK_CustomerProfilePrint] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_CustomerProfilePrint_7_1439396247__K2_K1_3_4_5_6_7_8_9_10_11_12_13_14_15_16_17_18_19_20_21] ON [dbo].[CustomerProfilePrint]
(
	[CustomerProfilesID] ASC,
	[id] ASC
)
INCLUDE([AdminInfoID],[ContactOption],[Dollars],[EstimateNumber],[FooterText],[GraphicsQuality],[GreyBars],[NoHeaderLogo],[NoInsuranceSection],[NoPhotos],[NoVehicleAccessories],[OrderBy],[PrintEstimator],[PrintPaymentInfo],[PrintPrivateNotes],[PrintPublicNotes],[SeparateLabor],[SupplementOption],[UseBigLetters]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_CustomerProfilePrint_7_1439396247__K2_K1_4_7] ON [dbo].[CustomerProfilePrint]
(
	[CustomerProfilesID] ASC,
	[id] ASC
)
INCLUDE([FooterText],[NoHeaderLogo]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_PrintPrivateNotes]  DEFAULT ((0)) FOR [PrintPrivateNotes]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_PrintPublicNotes]  DEFAULT ((1)) FOR [PrintPublicNotes]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_SeparateLabor]  DEFAULT ((0)) FOR [SeparateLabor]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_EstimateNumber]  DEFAULT ((1)) FOR [EstimateNumber]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_AdminInfoID]  DEFAULT ((1)) FOR [AdminInfoID]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_Dollars]  DEFAULT ((0)) FOR [Dollars]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_GreyBars]  DEFAULT ((0)) FOR [GreyBars]
GO
ALTER TABLE [dbo].[CustomerProfilePrint] ADD  CONSTRAINT [DF_CustomerProfilePrint_PrintPaymentInfo]  DEFAULT ((0)) FOR [PrintPaymentInfo]
GO
