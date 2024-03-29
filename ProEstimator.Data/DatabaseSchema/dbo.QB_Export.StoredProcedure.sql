USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[QB_Export] 
	  @LoginID			INT 
	, @StartDate		DATETIME 
	, @EndDate			DATETIME 
AS 
BEGIN 
 
	-- Create a table to store the final results. 
	create TABLE #Results  
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
 
	-- Get a cursor for all estimates that will be in the final table 
	DECLARE allEstimates CURSOR 
	FOR 
	SELECT EstimationData.AdminInfoID, EstimationData.LockLevel 
	FROM AdminInfo 
	LEFT OUTER JOIN EstimationData ON AdminInfo.ID = EstimationData.AdminInfoID 
	LEFT OUTER JOIN JobStatusHistory ON JobStatusHistory.ID = (SELECT MAX(ID) FROM JobStatusHistory WHERE AdminInfoID = AdminInfo.ID) 
	WHERE  
		--AdminInfo.ID =  7886449   
		AdminInfo.CreatorID = @LoginID 
		AND CAST(JobStatusHistory.ActionDate AS DATE) BETWEEN @StartDate AND @EndDate 
		AND ISNULL(AdminInfo.Deleted, 0) = 0 
		AND ISNULL(JobStatusHistory.JobStatusID, 0) = 3  
 
	DECLARE @AdminInfoID INT 
	DECLARE @Supplements INT 
 
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
 
	OPEN allEstimates 
 
	FETCH NEXT FROM allEstimates INTO @AdminInfoID, @Supplements 
 
	WHILE @@FETCH_STATUS = 0 
		BEGIN 
 
			DECLARE @EstimateDate DATETIME 
			DECLARE @ClosedDate DATETIME 
			DECLARE @TaxID VARCHAR(5) = 'FLO' 
			DECLARE @InsuranceCompany VARCHAR(100) 
			DECLARE @InsuranceClaimDetails VARCHAR(200) 
			DECLARE @CustomerName VARCHAR(50)  
			DECLARE @CustomerAddress1 VARCHAR(500) 
			DECLARE @CustomerAddress2 VARCHAR(500) 
			DECLARE @CustomerCity VARCHAR(100) 
			DECLARE @CustomerState VARCHAR(50) 
			DECLARE @CustomerZip VARCHAR(10) 
			DECLARE @CustomerPhone VARCHAR(50) 
			DECLARE @CustomerEmail VARCHAR(100) 
			DECLARE @VIN VARCHAR(20)  
 			DECLARE @QBInvoiceID INT

			SELECT 
				  @EstimateDate = EstimationDate 
				, @InsuranceCompany = InsuranceCompanyName 
				, @InsuranceClaimDetails = ISNULL 
				  ( 
					ISNULL(EstimationData.ClaimNumber, '')  
					+ CASE WHEN ISNULL(InsuranceAdjuster.FirstName, '') <> '' AND ISNULL(EstimationData.ClaimNumber, '')  <> '' THEN ', ' END + ISNULL(InsuranceAdjuster.FirstName, '') 
					+ CASE WHEN ISNULL(InsuranceAdjuster.LastName, '') <> '' THEN ' ' + ISNULL(InsuranceAdjuster.LastName, '') END 
					+ CASE WHEN ISNULL(InsuranceAdjuster.Phone1, '') <> '' THEN ', ' + ISNULL(InsuranceAdjuster.Phone1, '') END  
				  , '')  
				, @CustomerName = dbo.HtmlEncode(ISNULL(ContactOwner.FirstName, '') + ' ' + ISNULL(ContactOwner.LastName, ''))				 
				, @CustomerAddress1 = ISNULL(ContactAddress.Address1, '') 
				, @CustomerAddress2 = ISNULL(ContactAddress.Address2, '') 
				, @CustomerCity = ISNULL(ContactAddress.City, '') 
				, @CustomerState = ISNULL(ContactAddress.State, '') 
				, @CustomerZip = ISNULL(ContactAddress.zip, '') 
				, @CustomerPhone = ISNULL(ContactOwner.Phone1, '') 
				, @CustomerEmail = ISNULL(ContactOwner.Email, '') 
				, @VIN = VehicleInfo.Vin 
				, @QBInvoiceID = (CASE WHEN ISNULL(QuickbookInvoiceID, 0) > 0 THEN QuickbookInvoiceID ELSE NULL END)
			FROM AdminInfo 
			LEFT OUTER JOIN EstimationData ON AdminInfo.id = EstimationData.AdminInfoID 
			LEFT OUTER JOIN VehicleInfo ON VehicleInfo.EstimationDataId = EstimationData.id 
			LEFT OUTER JOIN tbl_ContactPerson AS InsuranceAdjuster ON  AdminInfo.id = InsuranceAdjuster.AdminInfoID AND InsuranceAdjuster.ContactTypeID = 1 AND InsuranceAdjuster.ContactSubTypeID = 11  
			LEFT OUTER JOIN Customer ON AdminInfo.CustomerID = Customer.ID 
			LEFT OUTER JOIN tbl_ContactPerson AS ContactOwner ON Customer.ContactID = ContactOwner.ContactID 
			LEFT OUTER JOIN tbl_Address AS ContactAddress ON Customer.AddressID = ContactAddress.AddressID 
			LEFT OUTER JOIN MappingEstIdQbInvoiceID ON EstimationData.AdminInfoID = MappingEstIdQbInvoiceID.AdminInfoID
			WHERE AdminInfo.id = @AdminInfoID 
 
			SELECT @TaxID = State 
			FROM AdminInfo 
			LEFT OUTER JOIN Logins ON AdminInfo.CreatorID = Logins.ID
			LEFT OUTER JOIN tbl_Address ON Logins.ContactID = tbl_Address.ContactsID	
			WHERE AdminInfo.ID = @AdminInfoID 
 
			SELECT @ClosedDate = MAX(ActionDate) FROM JobStatusHistory WHERE AdminInfoID = @AdminInfoID AND JobStatusID = 3 
			
			EXEC FillProcessedLinesIfNeeded @AdminInfoID = @AdminInfoID;

			INSERT INTO #EstimateSubTotals  
			SELECT * FROM dbo.GetEstimateSubTotalsFunction(@AdminInfoID, @Supplements,  0, 0)  
 
			-- Taxable
			INSERT INTO #Results 
			SELECT 
				  @EstimateDate AS Date 
				, @AdminInfoiD AS EstimateID 
				, @CustomerName AS CustomerName 
				, @CustomerAddress1 AS CustomerAddress1 
				, @CustomerAddress2 AS CustomerAddress2 
				, @CustomerCity AS CustomerCity 
				, @CustomerState AS CustomerState 
				, @CustomerZip AS CustomerZip 
				, @CustomerPhone AS CustomerPhone 
				, @CustomerEmail AS CustomerEmail 
				, @VIN AS VIN 
				, RateName AS ItemDescription 
				, CASE WHEN RateName LIKE '%Parts' OR SortOrder LIKE 'C%' OR SortOrder = 'BPDR'  THEN 1 ELSE ISNULL(Hours, 0) END AS Quantity 
				, CASE WHEN RateName LIKE '%Parts' OR SortOrder LIKE 'C%' OR SortOrder = 'BPDR'  THEN 
						(CASE WHEN DiscountMarkupLineItemTotal < 0 THEN DirectLineItemTotal ELSE FinalLineItemTotal END) 
				  ELSE ISNULL(Rate, 0) END AS Rate 
				, CASE WHEN ISNULL(Taxable, 0) = 0 THEN 'N' ELSE 'Y' END AS Taxed 
				, (CASE WHEN DiscountMarkupLineItemTotal < 0 THEN DirectLineItemTotal ELSE FinalLineItemTotal END) AS Amount 
				, RateName AS ItemName 
				, @ClosedDate AS ClosedRODate 
				, @TaxID AS StateTaxID 
				, @InsuranceCompany AS InsuranceCompany 
				, @InsuranceClaimDetails AS InsuranceClaimDetails 
				, @QBInvoiceID AS QBInvoiceID
				, Notes
				, DirectLineItemTotal
				, DiscountMarkupLineItemTotal
			FROM  (SELECT TOP 50000 * FROM #EstimateSubTotals WHERE Taxable = 1) SubTotals 

			-- Non-taxable
			INSERT INTO #Results 
			SELECT 
				  @EstimateDate AS Date 
				, @AdminInfoiD AS EstimateID 
				, @CustomerName AS CustomerName 
				, @CustomerAddress1 AS CustomerAddress1 
				, @CustomerAddress2 AS CustomerAddress2 
				, @CustomerCity AS CustomerCity 
				, @CustomerState AS CustomerState 
				, @CustomerZip AS CustomerZip 
				, @CustomerPhone AS CustomerPhone 
				, @CustomerEmail AS CustomerEmail 
				, @VIN AS VIN 
				, RateName AS ItemDescription 
				, CASE WHEN RateName LIKE '%Parts' OR SortOrder LIKE 'C%' OR SortOrder = 'BPDR'  THEN 1 ELSE ISNULL(Hours, 0) END AS Quantity 
				, CASE WHEN RateName LIKE '%Parts' OR SortOrder LIKE 'C%' OR SortOrder = 'BPDR'  THEN 
						(CASE WHEN DiscountMarkupLineItemTotal < 0 THEN DirectLineItemTotal ELSE FinalLineItemTotal END) 
				  ELSE ISNULL(Rate, 0) END AS Rate 
				, CASE WHEN ISNULL(Taxable, 0) = 0 THEN 'N' ELSE 'Y' END AS Taxed 
				, (CASE WHEN DiscountMarkupLineItemTotal < 0 THEN DirectLineItemTotal ELSE FinalLineItemTotal END) AS Amount 
				, RateName AS ItemName 
				, @ClosedDate AS ClosedRODate 
				, @TaxID AS StateTaxID 
				, @InsuranceCompany AS InsuranceCompany 
				, @InsuranceClaimDetails AS InsuranceClaimDetails 
				, @QBInvoiceID AS QBInvoiceID
				, Notes
				, DirectLineItemTotal
				, DiscountMarkupLineItemTotal
			FROM  (SELECT TOP 50000 * FROM #EstimateSubTotals WHERE Taxable = 0) SubTotals

			DECLARE @totalAmount AS FLOAT
			SET @totalAmount = 0
			DECLARE @isTaxable AS CHAR(1)

			SELECT @totalAmount = @totalAmount + ISNULL(FinalLineItemTotal,0), @isTaxable = CASE WHEN ISNULL(Taxable, 0) = 0 THEN 'N' ELSE 'Y' END
			FROM #EstimateSubTotals

			INSERT INTO #Results(Date,EstimateID,CustomerName,CustomerAddress1,CustomerAddress2,CustomerCity,CustomerState,
			CustomerZip,CustomerPhone,CustomerEmail,VIN,
			ItemDescription,Quantity,Rate,Taxed,Amount,ItemName,ClosedRODate,StateTaxID,InsuranceCompany,InsuranceClaimDetails,QBInvoiceID)
			SELECT @EstimateDate,@AdminInfoiD,@CustomerName,@CustomerAddress1,@CustomerAddress2,@CustomerCity,
			@CustomerState,@CustomerZip,@CustomerPhone,@CustomerEmail,@VIN,
			'Processing Fee',@totalAmount,(CreditCardFeePercentage / 100),
			CASE WHEN ISNULL(TaxedCreditCardFee, 0) = 0 THEN 'N' ELSE 'Y' END
			,CONVERT(decimal(10,2), @totalAmount * (CreditCardFeePercentage / 100)),
			'Processing Fee',@ClosedDate,
			@TaxID,@InsuranceCompany,@InsuranceClaimDetails,@QBInvoiceID
			FROM EstimationData WHERE AdminInfoID = @AdminInfoiD

			INSERT INTO #Results(Date,EstimateID,CustomerName,CustomerAddress1,CustomerAddress2,CustomerCity,CustomerState,
			CustomerZip,CustomerPhone,CustomerEmail,VIN,
			ItemDescription,Quantity,Rate,Taxed,Amount,ItemName,ClosedRODate,StateTaxID,InsuranceCompany,InsuranceClaimDetails,QBInvoiceID)
			SELECT @EstimateDate,@AdminInfoiD,@CustomerName,@CustomerAddress1,@CustomerAddress2,@CustomerCity,
			@CustomerState,@CustomerZip,@CustomerPhone,@CustomerEmail,@VIN,
			'Customer Payment Made',Amount,1,'N',Amount,'Payment Made',@ClosedDate,
			@TaxID,@InsuranceCompany,@InsuranceClaimDetails,@QBInvoiceID
			FROM PaymentInfo WHERE AdminInfoID = @AdminInfoiD
			ORDER BY PaymentID ASC

			DELETE FROM #EstimateSubTotals 
 
			FETCH NEXT FROM allEstimates INTO @AdminInfoID, @Supplements 
		END 
 
	CLOSE allEstimates 
	DEALLOCATE allEstimates 

	UPDATE	#Results SET Notes = NULL, DirectLineItemTotal = NULL , DiscountMarkupLineItemTotal = NULL
	WHERE	DiscountMarkupLineItemTotal >= 0
 
	SELECT * 
	FROM #Results   
	WHERE ((Amount > 0 AND ItemName NOT IN ('Paint Labor','Paint Supplies')) OR
			(Amount > 0 AND ItemName IN ('Paint Labor','Paint Supplies'))
			OR ItemName = 'Subtotal')
 
END 
GO
