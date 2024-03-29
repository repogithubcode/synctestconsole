USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_company](
	[CompanyID] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](150) NULL,
	[Affiliation] [nvarchar](50) NULL,
	[IDCode] [nvarchar](50) NULL,
	[FederalTaxID] [nvarchar](50) NULL,
	[BarNumber] [nvarchar](50) NULL,
	[CompanyType] [nvarchar](50) NULL,
	[RegistrationNumber] [nvarchar](50) NULL,
	[WebAddress] [nvarchar](100) NULL,
	[VendorSourceID] [nvarchar](50) NULL,
	[AdmininfoID] [int] NULL,
	[ContactsID] [int] NOT NULL
) ON [PRIMARY]
GO
