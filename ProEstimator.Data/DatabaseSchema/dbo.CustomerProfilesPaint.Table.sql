USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CustomerProfilesPaint](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[CustomerProfilesID] [int] NOT NULL,
	[AllowDeductions] [bit] NULL,
	[AdjacentDeduction] [real] NULL,
	[NonAdjacentDeduction] [real] NULL,
	[EdgeInteriorTimes] [bit] NULL,
	[MajorClearCoat] [real] NULL,
	[MajorThreeStage] [real] NULL,
	[MajorTwoTone] [real] NULL,
	[OverlapClearCoat] [real] NULL,
	[OverlapThreeStage] [real] NULL,
	[OverLapTwoTone] [real] NULL,
	[ClearCoatCap] [real] NULL,
	[DeductFinishOverlap] [bit] NULL,
	[TotalClearcoatWithPaint] [bit] NULL,
	[NoClearcoatCap] [bit] NULL,
	[ThreeStageInner] [bit] NULL,
	[ThreeStagePillars] [bit] NULL,
	[ThreeStateInterior] [bit] NULL,
	[TwoToneInner] [bit] NULL,
	[TwoTonePillars] [bit] NULL,
	[TwoToneInterior] [bit] NULL,
	[Blend] [real] NULL,
	[Underside] [real] NULL,
	[Edging] [real] NULL,
	[ManualPaintOverlap] [bit] NOT NULL,
	[AutomaticOverlap] [bit] NOT NULL,
	[ThreeTwoBlend] [real] NULL,
 CONSTRAINT [PK_CustomerProfilesPaint] PRIMARY KEY CLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, FILLFACTOR = 70) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_CustomerProfilesPaint_7_61295328__K2_K1_3_4_5_6_7_8_9_10_11_12_13_14_15_16_17_18_19_20_21_22_23_24_25_26_27_28] ON [dbo].[CustomerProfilesPaint]
(
	[CustomerProfilesID] ASC,
	[id] ASC
)
INCLUDE([AllowDeductions],[AdjacentDeduction],[NonAdjacentDeduction],[EdgeInteriorTimes],[MajorClearCoat],[MajorThreeStage],[MajorTwoTone],[OverlapClearCoat],[OverlapThreeStage],[OverLapTwoTone],[ClearCoatCap],[DeductFinishOverlap],[TotalClearcoatWithPaint],[NoClearcoatCap],[ThreeStageInner],[ThreeStagePillars],[ThreeStateInterior],[TwoToneInner],[TwoTonePillars],[TwoToneInterior],[Blend],[Underside],[Edging],[ManualPaintOverlap],[AutomaticOverlap],[ThreeTwoBlend]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[CustomerProfilesPaint] ADD  CONSTRAINT [DF_CustomerProfilesPaint_ManualPaintOverlap]  DEFAULT (0) FOR [ManualPaintOverlap]
GO
ALTER TABLE [dbo].[CustomerProfilesPaint] ADD  CONSTRAINT [DF_CustomerProfilesPaint_AutomaticOverlap]  DEFAULT ((0)) FOR [AutomaticOverlap]
GO
