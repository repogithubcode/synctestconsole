USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[_DatabaseMigrations](
	[DateOfExecution] [datetime] NULL,
	[MigrationFileName] [varchar](255) NOT NULL
) ON [PRIMARY]
GO
