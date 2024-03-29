USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[OrganizationInfo](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[OrgInfoContactsID] [int] NOT NULL,
	[FederalTaxID] [varchar](15) NULL,
	[RegistrationNumber] [varchar](25) NULL,
	[CompanyType] [varchar](20) NULL,
	[PaidContract] [bit] NULL,
	[ExpireDate] [datetime] NULL,
	[GraphicsExpireDate] [datetime] NULL,
	[ManualExpireDate] [datetime] NULL,
	[CustomerNumber] [varchar](20) NULL,
	[Disabled] [bit] NULL,
	[ShowRepairShopProfiles] [bit] NULL,
	[AllowAlternateIdentities] [bit] NULL,
	[AllowRWofInspAssignDates] [bit] NULL,
	[Appraiser] [bit] NULL,
	[LastEstimateNumber] [int] NULL,
	[LastReportNumber] [int] NULL,
	[LastWorkOrderNumber] [int] NULL,
	[DateCreated] [datetime] NOT NULL,
	[DelPWRequired] [bit] NOT NULL,
	[DelPW] [varchar](30) NULL,
	[ShowLaborTimeWO] [bit] NOT NULL,
	[Language_ID] [int] NOT NULL,
	[CompanyName] [varchar](50) NULL,
	[LicenseNumber] [varchar](50) NULL,
	[PrintLicenseNumber] [bit] NULL,
	[BarNumber] [varchar](50) NULL,
	[PrintBarNumber] [bit] NULL,
	[PrintFederalTaxID] [bit] NULL,
	[CompanyID] [varchar](50) NULL,
	[PrintRegistrationNumber] [bit] NULL,
	[UseTaxID] [bit] NULL,
	[UseDefaultRateProfile] [bit] NULL,
	[LogoImageType] [varchar](5) NULL,
	[ProfileLocked] [bit] NULL,
	[UseDefaultPDRRateProfile] [bit] NULL,
	[parts_now] [bit] NULL,
	[OverlapAdmin] [bit] NULL,
 CONSTRAINT [PK_OrganizationInfo] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 90) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [idx_OrganizationInfo_2] ON [dbo].[OrganizationInfo]
(
	[id] ASC,
	[OrgInfoContactsID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_Disabled]  DEFAULT (0) FOR [Disabled]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_ShowRepairShopProfiles]  DEFAULT (0) FOR [ShowRepairShopProfiles]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_AllowAlternateIdentities]  DEFAULT ((0)) FOR [AllowAlternateIdentities]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_DateCreated]  DEFAULT (getdate()) FOR [DateCreated]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_DelPWRequired]  DEFAULT ((0)) FOR [DelPWRequired]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_ShowLaborTimeWO]  DEFAULT ((1)) FOR [ShowLaborTimeWO]
GO
ALTER TABLE [dbo].[OrganizationInfo] ADD  CONSTRAINT [DF_OrganizationInfo_Language_ID]  DEFAULT ((1)) FOR [Language_ID]
GO
