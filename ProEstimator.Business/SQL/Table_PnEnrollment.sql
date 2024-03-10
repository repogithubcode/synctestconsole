SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[PnEnrollment](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[LoginId] [int] NOT NULL,
	[ShopId] [nvarchar](50) NULL,
	[ShopUri] [nvarchar](max) NULL,
	[RequestId] [nvarchar](50) NULL,
	[CreateDate] [date] NULL
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

