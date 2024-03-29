USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[tbl_ContactPerson](
	[ContactID] [int] IDENTITY(1,1) NOT NULL,
	[AdmininfoID] [int] NULL,
	[FirstName] [nvarchar](50) NULL,
	[MiddleName] [nvarchar](50) NULL,
	[LastName] [nvarchar](50) NULL,
	[Email] [nvarchar](75) NULL,
	[Phone2] [nvarchar](50) NULL,
	[Phone1] [nvarchar](50) NULL,
	[FaxNumber] [nvarchar](50) NULL,
	[BusinessName] [nvarchar](100) NULL,
	[Notes] [nvarchar](1500) NULL,
	[Title] [nvarchar](50) NULL,
	[SaveCustomer] [bit] NULL,
	[Extension1] [varchar](10) NULL,
	[Extension2] [varchar](10) NULL,
	[ContactTypeID] [tinyint] NULL,
	[ContactSubTypeID] [smallint] NULL,
	[DateAdded] [datetime] NULL,
	[PhoneNumberType1] [nchar](10) NULL,
	[PhoneNumberType2] [nchar](10) NULL,
	[Phone3] [nvarchar](50) NULL,
	[PhoneNumberType3] [nchar](10) NULL,
	[Extension3] [varchar](10) NULL,
	[SecondaryEmail] [varchar](75) NULL,
 CONSTRAINT [PK_tbl_ContactPerson] PRIMARY KEY CLUSTERED 
(
	[ContactID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
SET ANSI_PADDING ON
GO
CREATE NONCLUSTERED INDEX [_dta_index_tbl_ContactPerson_6_1789077305__K1_K13_K10_K7_K5_K9] ON [dbo].[tbl_ContactPerson]
(
	[ContactID] ASC,
	[BusinessName] ASC,
	[Phone1] ASC,
	[LastName] ASC,
	[FirstName] ASC,
	[Phone2] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_tbl_ContactPerson_7_1789077305__K3_K19_K20_5_7] ON [dbo].[tbl_ContactPerson]
(
	[AdmininfoID] ASC,
	[ContactTypeID] ASC,
	[ContactSubTypeID] ASC
)
INCLUDE([FirstName],[LastName]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [IX_tbl_ContactPerson_AdmininfoID] ON [dbo].[tbl_ContactPerson]
(
	[AdmininfoID] ASC
)
INCLUDE([FirstName],[LastName],[Email],[Phone2],[Phone1],[FaxNumber],[BusinessName],[ContactSubTypeID]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
