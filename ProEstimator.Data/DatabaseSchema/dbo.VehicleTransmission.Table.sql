USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[VehicleTransmission](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransCode] [char](1) NULL,
	[Description] [varchar](50) NULL,
	[TransmissionID] [int] NULL
) ON [PRIMARY]
GO
