USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[QB_Export_Summary] 
	  @LoginID			INT 
	, @StartDate		DATETIME 
	, @EndDate			DATETIME 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    -- Create a table to store the final results. 
	Create TABLE #Results  
	( 
		  Date					datetime 
		, EstimateID			int 
		, CustomerName			varchar(100) 
		, CustomerAddress1		varchar(500) 
		, CustomerAddress2		varchar(500) 
		, CustomerCity			varchar(100) 
		, CustomerState			varchar(50) 
		, CustomerZip			varchar(10) 
		, CustomerPhone			varchar(30) 
		, CustomerEmail			varchar(100) 
		, VIN					varchar(30) 
		, ItemDescription		varchar(200) 
		, Quantity				real 
		, Rate					real 
		, Taxed					varchar(1) 
		, Amount				money 
		, ItemName				varchar(50) 
		, ClosedRODate			datetime 
		, StateTaxID			varchar(5) 
		, InsuranceCompany		varchar(200) 
		, InsuranceClaimDetails	varchar(100) 
		, QBInvoiceID			int
		, Notes					varchar(100) 
		, DirectLineItemTotal	real
		, DiscountMarkupLineItemTotal	real   
	) 
 
	INSERT INTO #Results 
	EXEC QB_Export @LoginID = @LoginID, @StartDate = @StartDate, @EndDate = @EndDate 
 
	SELECT DISTINCT --* 
		  Results.EstimateID 
		, ExportHistory.ExportDate 
		, COUNT(*) AS ExportRows 
		, CustomerName 
		, VIN 
		, InsuranceCompany 
		, ClosedRODate 
		, InsuranceClaimDetails 
		, CASE WHEN ExportHistory.Supplement <> ISNULL(EstimationData.LockLevel, 0) THEN 1 ELSE 0 END AS HasChanges 
		, Results.QBInvoiceID 
		--, ExportHistory.Supplement AS ExportedSupplement 
		--, ISNULL(EstimationData.LockLevel, 0) AS CurrentSupplement
		, Notes
		, DirectLineItemTotal
		, DiscountMarkupLineItemTotal
	FROM #Results AS Results 
	LEFT OUTER JOIN 
	( 
		SELECT QBExportEstimateLog.EstimateID, MAX(QBExportLog.TimeStamp) AS ExportDate, ISNULL(MAX(Supplement), 0) AS Supplement 
		FROM QBExportEstimateLog 
		LEFT OUTER JOIN QBExportLog ON QBExportEstimateLog.ExportLogID = QBExportLog.ID 
		GROUP BY EstimateID 
	) AS ExportHistory ON Results.EstimateID = ExportHistory.EstimateID 
	LEFT OUTER JOIN EstimationData ON Results.EstimateID = EstimationData.AdminInfoID 
	GROUP BY 
		  Results.EstimateID 
		, ExportHistory.ExportDate 
		, CustomerName 
		, CustomerAddress1 
		, CustomerAddress2 
		, CustomerCity 
		, CustomerState 
		, CustomerZip 
		, CustomerPhone 
		, CustomerEmail 
		, VIN 
		, InsuranceCompany 
		, ClosedRODate 
		, InsuranceClaimDetails 
		, ExportHistory.Supplement 
		, EstimationData.LockLevel 
		, Results.QBInvoiceID
		, Notes
		, DirectLineItemTotal
		, DiscountMarkupLineItemTotal 
 
END 
GO
