USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstimatorsJobHistory](
	[JobID] [int] IDENTITY(1,1) NOT NULL,
	[AdminInfoID] [int] NULL,
	[EstimatorID] [int] NULL,
	[JobStatus] [varchar](40) NOT NULL,
	[JobDate] [date] NOT NULL
) ON [PRIMARY]
GO
