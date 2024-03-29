USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[GetAdminInfoAndEstimateData]
	@AdminInfoID	int
AS
BEGIN
	SELECT 
	   ai.[id]
      ,ai.[CreatorID]
      ,ai.[Description]
      ,ai.[CustomerProfilesID]
	  ,ai.[AddOnProfileID]
	  ,ai.[GrandTotal]
      ,ai.[BettermentTotal]
      ,ai.[EstimateNumber]
      ,ai.[WorkOrderNumber]
      ,ai.[PrintDescription]
      ,ai.[Archived]
      ,ai.[Deleted]
      ,ai.[LastView]
	  ,ai.[IsImported]
	  ,ai.[CustomerID]
      
      ,ed.[EstimationDate]
      ,ed.[DateOfLoss]
      ,ed.[CoverageType]
      ,ed.[EstimateLocation]
      ,ed.[TransactionLevel]
      ,ed.[LockLevel]
      ,ed.[LastLineNumber]
      ,ed.[EstimationLineItemIDLocked]
      ,ed.[Note]
      ,ed.[PrintNote]
      ,ed.[AssignmentDate]
      ,ed.[ReportTextHeader]
      ,ed.[AlternateIdentitiesID]
      ,ed.[SupplementVersion]
      ,ed.[NextUniqueSequenceNumber]
	  ,ed.[ClaimNumber]
      ,ed.[PolicyNumber]
      ,ed.[Deductible]
	  ,ed.[InsuranceCompanyID]
      ,ed.[InsuranceCompanyName]
      ,ed.[ClaimantSameAsOwner]
      ,ed.[InsuredSameAsOwner]
      ,ed.[EstimatorID]
	  ,ed.[RepairFacilityVendorID]
	  ,ed.[ImageSize]
	  ,ed.[PurchaseOrderNumber]
	  ,ed.[AdjusterID]
	  ,ed.[ClaimRepID]
	  ,ed.[PrintInsured]
	  ,ed.[RepairDays]
	  ,ed.[HoursInDay]
	  ,ed.[FacilityRepairOrder]
	  ,ed.[FacilityRepairInvoice]
	  ,ed.[CreditCardFeePercentage]
	  ,ed.[TaxedCreditCardFee]
	FROM AdminInfo ai
	LEFT OUTER JOIN EstimationData ed ON ed.AdminInfoID = @AdminInfoID
	WHERE ai.id = @AdminInfoID
END

GO
