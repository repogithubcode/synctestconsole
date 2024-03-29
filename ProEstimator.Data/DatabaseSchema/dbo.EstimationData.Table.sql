USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[EstimationData](
	[id] [int] IDENTITY(1,1) NOT NULL,
	[AdminInfoID] [int] NOT NULL,
	[EstimationDate] [datetime] NULL,
	[DateOfLoss] [datetime] NULL,
	[CoverageType] [tinyint] NULL,
	[EstimateLocation] [varchar](100) NULL,
	[TransactionLevel] [varchar](2) NULL,
	[LockLevel] [tinyint] NULL,
	[LastLineNumber] [smallint] NULL,
	[EstimationLineItemIDLocked] [int] NULL,
	[Note] [varchar](5000) NULL,
	[PrintNote] [bit] NULL,
	[AssignmentDate] [datetime] NULL,
	[ReportTextHeader] [varchar](50) NULL,
	[AlternateIdentitiesID] [int] NULL,
	[SupplementVersion] [tinyint] NULL,
	[NextUniqueSequenceNumber] [int] NULL,
	[ClaimNumber] [varchar](50) NULL,
	[PolicyNumber] [varchar](50) NULL,
	[Deductible] [money] NULL,
	[InsuranceCompanyName] [varchar](50) NULL,
	[ClaimantSameAsOwner] [bit] NULL,
	[InsuredSameAsOwner] [bit] NULL,
	[EstimatorID] [int] NULL,
	[RepairFacilityVendorID] [int] NULL,
	[ImageSize] [varchar](50) NULL,
	[PurchaseOrderNumber] [varchar](50) NULL,
	[InsuranceCompanyID] [int] NULL,
	[AdjusterID] [int] NULL,
	[ClaimRepID] [int] NULL,
	[PrintInsured] [bit] NULL,
	[RepairDays] [tinyint] NULL,
	[HoursInDay] [tinyint] NOT NULL,
	[FacilityRepairOrder] [varchar](50) NULL,
	[FacilityRepairInvoice] [varchar](50) NULL,
	[CreditCardFeePercentage] [decimal](18, 2) NULL,
	[TaxedCreditCardFee] [bit] NULL,
 CONSTRAINT [PK_EstimationData] PRIMARY KEY NONCLUSTERED 
(
	[id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]
GO
CREATE CLUSTERED INDEX [idx_EstimationData_1] ON [dbo].[EstimationData]
(
	[AdminInfoID] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationData_47_1893581784__K2D_K1_3_8] ON [dbo].[EstimationData]
(
	[AdminInfoID] DESC,
	[id] ASC
)
INCLUDE([EstimationDate],[LockLevel]) WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
CREATE NONCLUSTERED INDEX [_dta_index_EstimationData_9_1893581784__K2_K1_K10] ON [dbo].[EstimationData]
(
	[AdminInfoID] ASC,
	[id] ASC,
	[EstimationLineItemIDLocked] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
GO
ALTER TABLE [dbo].[EstimationData] ADD  CONSTRAINT [DF_EstimationData_EstimationDate]  DEFAULT (getdate()) FOR [EstimationDate]
GO
ALTER TABLE [dbo].[EstimationData] ADD  CONSTRAINT [DF_EstimationData_LastLineNumber]  DEFAULT (0) FOR [LastLineNumber]
GO
ALTER TABLE [dbo].[EstimationData] ADD  CONSTRAINT [DF_EstimationData_NextUniqueSequenceNumber]  DEFAULT (1) FOR [NextUniqueSequenceNumber]
GO
ALTER TABLE [dbo].[EstimationData] ADD  CONSTRAINT [DF__Estimatio__Hours__30823DF3]  DEFAULT ((5)) FOR [HoursInDay]
GO
