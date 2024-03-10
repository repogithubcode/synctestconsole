USE [FocusWrite]
GO
/****** Object:  Table [dbo].[VendorType]    Script Date: 10/12/2023 10:22:12 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VendorType](
	[ID] [tinyint] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](30) NULL,
 CONSTRAINT [PK_VendorType] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[VendorType] ON 
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (1, N'After')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (2, N'After')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (3, N'Alternate')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (4, N'Insurance')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (5, N'LKQ')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (6, N'OEM')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (7, N'Reman')
GO
INSERT [dbo].[VendorType] ([ID], [Name]) VALUES (8, N'Repair')
GO
SET IDENTITY_INSERT [dbo].[VendorType] OFF
GO
