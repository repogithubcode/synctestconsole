USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Vendor](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[LoginsID] [int] NULL,
	[CompanyIDCode] [int] NULL,
	[CompanyName] [varchar](200) NULL,
	[FirstName] [varchar](50) NULL,
	[MiddleName] [varchar](50) NULL,
	[LastName] [varchar](50) NULL,
	[Email] [varchar](100) NULL,
	[MobileNumber] [varchar](20) NULL,
	[HomeNumber] [varchar](20) NULL,
	[WorkNumber] [varchar](20) NULL,
	[Address1] [varchar](100) NULL,
	[Address2] [varchar](100) NULL,
	[City] [varchar](50) NULL,
	[State] [varchar](25) NULL,
	[Zip] [varchar](15) NULL,
	[TimeZone] [varchar](50) NULL,
	[Universal] [bit] NOT NULL,
	[Type] [varchar](50) NULL,
	[FaxNumber] [varchar](50) NULL,
	[FileName] [varchar](50) NULL,
	[Deleted] [bit] NULL,
	[Extension] [varchar](10) NULL,
	[SUPPLEMENT] [nvarchar](500) NULL,
	[FederalTaxID] [varchar](50) NULL,
	[LicenseNumber] [varchar](50) NULL,
	[BarNumber] [varchar](50) NULL,
	[RegistrationNumber] [varchar](50) NULL,
	[TypeID] [tinyint] NULL,
 CONSTRAINT [PK_Vendor] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [IX_Vendor_CompanyIDCode_Type] ON [dbo].[Vendor]
(
	[CompanyIDCode] ASC,
	[Type] ASC
)
INCLUDE([LoginsID],[CompanyName],[Universal],[Deleted]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[Vendor] ADD  CONSTRAINT [DF_Vendor_Universal]  DEFAULT ((0)) FOR [Universal]
GO
