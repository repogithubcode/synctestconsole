USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[LoginFailures](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[LoginName] [varchar](50) NULL,
	[Organization] [varchar](50) NULL,
	[Password] [varchar](255) NULL,
	[TimeDate] [datetime] NULL,
	[User_Addr] [varchar](25) NULL,
	[Reason] [varchar](100) NULL
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [idx_LoginFailures_1] ON [dbo].[LoginFailures]
(
	[TimeDate] DESC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[LoginFailures] ADD  CONSTRAINT [DF_LoginFailures_TimeDate]  DEFAULT (getdate()) FOR [TimeDate]
GO
