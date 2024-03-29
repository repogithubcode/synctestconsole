USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AddOnPreset](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[ProfileID] [int] NULL,
	[PresetShellID] [int] NULL,
	[Labor] [real] NULL,
	[Refinish] [real] NULL,
	[Charge] [money] NULL,
	[OtherTypeOverride] [varchar](20) NULL,
	[OtherCharge] [money] NULL,
	[Active] [bit] NULL,
	[AutoSelect] [bit] NULL,
 CONSTRAINT [PK_AddOnPreset] PRIMARY KEY CLUSTERED 
(
	[ID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [NonClusteredIndex-20210918-092649] ON [dbo].[AddOnPreset]
(
	[ProfileID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
