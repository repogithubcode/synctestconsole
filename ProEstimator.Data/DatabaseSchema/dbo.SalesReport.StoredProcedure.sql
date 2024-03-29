USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


--SalesReport-03.sql
--Adding a parameter @CustomerIDs
CREATE PROCEDURE [dbo].[SalesReport] 
	@LoginsID Int, 
	@StartDate DateTime, 
	@EndDate DateTime, 
	@EstimatorID Int,
	@CustomerIDs Varchar(8000)
AS 
	SET NOCOUNT ON 
 
	 -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables. 
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component 
	 IF 1=0 BEGIN 
       SET FMTONLY OFF 
     END

-- Create a sales report table  
Create TABLE #SalesReport  
( 
	  AdminInfoID		int 
	, Estimator			varchar(100) 
	, CustomerName		varchar(100) 
	, Parts				money 
	, BodyLabor			money 
	, PaintLabor		money 
	, FrameLabor		money 
	, MechanicalLabor	money 
	, PaintSupplies		money 
	, CCFee				money
	, Misc				money 
	, Tax				money 
	, Total				money 
	, EstimationDate	date
	, IDWorkOrderNumber varchar(100)
) 

DECLARE @IncludeEmpty BIT = 0

IF SUBSTRING(@CustomerIDs, 0, 4) = ',0,'
	BEGIN
		SET @IncludeEmpty = 1
	END

-- Create a cursor to loop through all Estimates to show in the report 
DECLARE @cursor CURSOR	    
SET @cursor = CURSOR FOR    
 
	SELECT  
		  AdminInfo.ID AS AdminInfoID 
		, ISNULL(EstimationData.LockLevel, 0) AS SupplementLevel 
		, ISNULL(tbl_ContactPerson.FirstName, '') + ' ' + ISNULL(tbl_ContactPerson.LastName, '') AS CustomerName 
		, ISNULL(AuthorFirstName, '') + ' ' + ISNULL(AuthorLastName, '') AS EstimatorName 
		, JobStatusHistory.ActionDate
		, AdminInfo.WorkOrderNumber AS WorkOrderNumber
	FROM AdminInfo 
	JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.ID 
 
	-- Join just the most recent "closed" job status 
	JOIN JobStatusHistory ON JobStatusHistory.id =  
	( 
		SELECT TOP 1 id  
		FROM JobStatusHistory  
		WHERE AdminInfoID = AdminInfo.ID 
		ORDER BY id DESC 
	)  

	LEFT OUTER JOIN EstimatorsData ON EstimationData.EstimatorID = EstimatorsData.EstimatorID
 
	LEFT OUTER JOIN Customer ON AdminInfo.CustomerID = Customer.ID 
	LEFT OUTER JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID 
 
	---- Join just the first contact customer record.   
	---- There should only be 1 any time, but at the time of this writing there is an open bug where sometimes an estimate has 2 customer contact  
	---- records which would duplicate this estimate 
	--LEFT OUTER JOIN tbl_ContactPerson ON tbl_ContactPerson.ContactID =  
	--( 
	--	SELECT TOP 1 ContactID 
	--	FROM tbl_ContactPerson 
	--	WHERE AdminInfoID = AdminInfo.ID AND ContactTypeID = 1 AND ContactSubTypeID = 4 
	--) 
	WHERE  
		CreatorID = @LoginsID
		AND 
		(
			@CustomerIDs IN (',0', '0') 
			OR CustomerID IN (SELECT * FROM STRING_SPLIT(@CustomerIDs, ',') Where LTRIM(value) <> '')
			OR (@IncludeEmpty = 1 AND ISNULL(tbl_ContactPerson.FirstName, '') + ISNULL(tbl_ContactPerson.LastName, '') = '')
		)
		AND AdminInfo.Deleted = 0 
		AND JobStatusHistory.JobStatusID = 3 
		AND JobStatusHistory.ActionDate BETWEEN @StartDate AND DATEADD(day, 1, @EndDate) 
		AND EstimationData.EstimatorID = (CASE WHEN @EstimatorID > 0 THEN @EstimatorID ELSE EstimationData.EstimatorID END)
 
	ORDER BY JobStatusHistory.ActionDate 
 
-- Loop through the estimate list, getting sub total and total data and adding each estimate to the return list 
DECLARE @AdminInfoID INT 
DECLARE @SupplementVersion INT 
DECLARE @CustomerName VARCHAR(100) 
DECLARE @EstimatorName VARCHAR(100) 
DECLARE @EstimationDate DATE 
DECLARE @WorkOrderNumber INT
 
Create TABLE #EstimateSubTotals    
(  
	RateName				varchar(50),   
	FinalLineItemTotal		float,   
	Taxable					bit,   
	Rate					float,   
	Hours					float,   
	SortOrder				varchar(50), 
	Notes					varchar(300), 
	CapType					int, 
	Cap						real,
	DirectLineItemTotal		real,
	DiscountMarkupLineItemTotal	decimal(8,2)
)  
 
Create TABLE #EstimateTotals  
( 
	  SortOrder				int 
	, [Type]				varchar(50) 
	, Note					varchar(50) 
	, Amount				money 
) 
 
OPEN @cursor    
FETCH NEXT FROM @cursor    
INTO @AdminInfoID, @SupplementVersion, @CustomerName, @EstimatorName, @EstimationDate, @WorkOrderNumber    
 
WHILE @@FETCH_STATUS = 0    
	BEGIN    
		-- Get the Sub Totals and Totals tables for the estimate 
		IF NOT EXISTS (SELECT * FROM ProcessedLines WHERE EstimateID = @AdminInfoID)
		BEGIN

			EXEC FillProcessedLines @AdminInfoID = @AdminInfoID

		END

		INSERT INTO #EstimateSubTotals SELECT * FROM dbo.GetEstimateSubTotalsFunction(@AdminInfoID, @SupplementVersion, 0, 0) 
		INSERT INTO #EstimateTotals EXEC EstimateReport_GetTotals @AdminInfoID = @AdminInfoID, @SupplementVersion = @SupplementVersion 
 
		--SELECT * FROM #EstimateSubTotals 
		--SELECT * FROM #EstimateTotals 
 
		INSERT INTO #SalesReport 
		( 
			  AdminInfoID	 
			, Estimator
			, CustomerName	 
			, Parts 
			, BodyLabor	 
			, PaintLabor 
			, FrameLabor 
			, MechanicalLabor 
			, PaintSupplies 
			, CCFee
			, Misc 
			, Tax 
			, Total 
			, EstimationDate 
			, IDWorkOrderNumber
		) 
		VALUES  
		( 
			  @AdminInfoID 
			, @EstimatorName
			, @CustomerName 
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName LIKE '%Parts%'), 0)  
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName = 'Body Labor' OR RateName = 'Cleanup Labor' OR RateName = 'Detail Labor' OR RateName = 'Other Labor' OR RateName = 'Electrical Labor' OR RateName = 'Glass Labor' OR RateName = 'Structure Labor' OR RateName = 'Aluminum Labor'), 0)  
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName = 'Paint Labor' OR RateName = 'Clearcoat Labor'), 0)  
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName = 'Frame Labor'), 0)  
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName = 'Mechanical Labor'), 0)  
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName = 'Paint Supplies' OR RateName = 'Clearcoat Supplies'), 0)
			, ISNULL((SELECT SUM(Amount) FROM #EstimateTotals WHERE Type = 'Processing Fee') , 0) 			
			, ISNULL((SELECT SUM(FinalLineItemTotal) FROM #EstimateSubTotals WHERE RateName IN ('Body Supplies', 'Nontaxed', 'Taxed', 'Sublet', 'Towing', 'Storage')), 0)  
			, ISNULL((SELECT SUM(Amount) FROM #EstimateTotals WHERE Type LIKE '%Tax') , 0) 
			, ISNULL((SELECT Amount FROM #EstimateTotals WHERE Type = 'Grand Total') , 0) 
			, @EstimationDate
			, CAST(@AdminInfoID AS VARCHAR(100)) + '/' + CAST(@WorkOrderNumber AS VARCHAR(100))
		) 
 
		DELETE FROM #EstimateSubTotals 
		DELETE FROM #EstimateTotals 
 
		FETCH NEXT FROM @cursor    
	INTO @AdminInfoID, @SupplementVersion, @CustomerName, @EstimatorName, @EstimationDate, @WorkOrderNumber    
END
CLOSE @cursor
DEALLOCATE @cursor

SELECT * FROM #SalesReport 
GO
