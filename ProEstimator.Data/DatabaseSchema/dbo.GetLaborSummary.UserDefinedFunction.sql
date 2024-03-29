USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[GetLaborSummary]  
(  
	@AdminInfoID		int  
	, @ForPreview bit 
)  
RETURNS @LaborSummary TABLE   
(  
	 EstimationLineItemsID		int  
	,LaborSummaryPaintPanel		varchar(200)  
	,LaborTotalPaintPanel		real  
	,LaborSummaryPaint			varchar(200)  
	,LaborTotalPaint			real  
	,LaborTotalPaintDollars		real 
	,LaborTotalClearcoat		real 
	,ClearcoatLaborIncluded		bit 
	,ClearcoatSuppliesIncluded	bit 
	,LaborSummaryExtra			varchar(200)  
	,LaborTotalExtra			real  
	,LaborExtraType				varchar(50) 
	,LaborExtraIncludedInType	varchar(50) 
	,LaborExtraRemovedType		varchar(50) 
	,LaborExtraRemovedIncludedInType		varchar(50)  
	,LaborExtraRemovedHours		real  
	,LaborExtraRemovedTotal		real
	,LaborSummaryPaintPanelPrint	varchar(200)
	,LaborSummaryPaintLaborPrint	varchar(200)
	,LaborSummaryExtraPrint			varchar(200)
	,IsPartsQuantity				BIT
	,IsLaborQuantity				BIT
	,IsPaintQuantity				BIT
	,PartsQuantity					REAL
	,LaborQuantity					REAL
	,PaintQuantity					REAL
	,PaintMaterialsBetterment		REAL
	,LaborBetterment				REAL
	,PaintLaborBetterment			REAL
)   
BEGIN  
 
	DECLARE @estimateID INT = @AdminInfoID 
 
	DECLARE @EstimationDataID INT = (SELECT ID FROM EstimationData WHERE AdminInfoID = @estimateID)    
   
	DECLARE @CustomerProfilesID INT =     
	(    
		SELECT AdminInfo.CustomerProfilesID    
		FROM EstimationData    
		JOIN AdminInfo ON EstimationData.AdminInfoID = AdminInfo.ID    
		WHERE EstimationData.ID = @EstimationDataID     
	)    
 
	DECLARE @ClearCoatInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 21 
		) > 0 THEN 1 ELSE 0 END 
	) 
	DECLARE @ClearCoatSuppliesInPaint BIT =  
	( 
		SELECT  
		CASE WHEN  
		( 
			SELECT IncludeIn FROM CustomerProfileRates 
			WHERE CustomerProfilesID = @customerProfilesID 
			AND RateType = 22 
		) > 0 THEN 1 ELSE 0 END 
	) 
   
	DECLARE @DollarOrHours CHAR =    
		CASE WHEN @ForPreview = 1 THEN 'h' 
		ELSE 
			CASE WHEN (SELECT TOP 1 ISNULL(Dollars, 0) FROM CustomerProfilePrint WHERE CustomerProfilesID = @CustomerProfilesID) = 1 THEN 'd' ELSE 'h' END    
		END   
   
	DECLARE @LaborLines TABLE  
	(  
		  EstimationLineItemsID		int  
		, ModifiesID				int  
		, SupplementVersion			int  
		, LaborTypeID				tinyint  
		, LaborTypeName				varchar(50)  
		, LaborTime					real  
		, DeductionAmount			real  
		, Rate						real  
		, LaborCost					money  
		, Sub						varchar(10)  
	)  
  
	INSERT INTO @LaborLines     
	(  
		  EstimationLineItemsID	  
		, ModifiesID  
		, SupplementVersion	  
		, LaborTypeID  
		, LaborTypeName	  
		, LaborTime	  
		, DeductionAmount  
		, Rate  
		, LaborCost  
		, Sub  
	)  
	SELECT  
		EstimationLineItems.ID,    
		EstimationLineItems.ModifiesID,    
		ISNULL(EstimationLineItems.SupplementVersion, 0) AS SupplementVersion,    
 
		-- The "paint types" are all counted as one when comparing between supplements, on other words changing between paint types doesn't remove the original 
		-- paint type and add the new, only change the difference in time for any paint type. 
		CASE WHEN (EstimationLineLabor.LaborType IN (18, 19, 29)) THEN 16 ELSE EstimationLineLabor.LaborType END AS LaborTypeID,    
 
		ISNULL(LaborTypes.LaborType, '') AS LaborTypeName,    
		ISNULL(EstimationLineLabor.LaborTime, 0) AS LaborTime,    
		CASE     
			WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction     
			WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction     
			ELSE 0     
		END AS DeductionAmount,    
		ISNULL(ISNULL(CustomerProfileRates2.Rate, CustomerProfileRates.rate),0) AS Rate,    
		ISNULL(EstimationLineLabor.LaborCost, 0) AS LaborCost,    
		CASE WHEN EstimationLineLabor.SubletFlag <> 0 THEN '(Sub)' ELSE '' END AS Sub    
	FROM EstimationLineItems    
	LEFT JOIN EstimationLineLabor ON EstimationLineItems.ID  =    EstimationLineLabor.EstimationLineItemsID
	LEFT JOIN LaborTypes ON EstimationLineLabor.LaborType   =   LaborTypes.ID 
    
	LEFT JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND LaborTypes.RateTypesID    =   CustomerProfileRates.RateType
	LEFT JOIN CustomerProfileRates CustomerProfileRates2 ON CustomerProfileRates2.CustomerProfilesID = @CustomerProfilesID 
	AND  CustomerProfileRates.IncludeIn   =    CustomerProfileRates2.RateType 
	LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = @CustomerProfilesID     
    
	WHERE EstimationLineItems.EstimationDataID = @EstimationDataID    
		AND (EstimationLineLabor.LaborType NOT IN (20, 21, 22, 26, 27, 28)  OR EstimationLineLabor.Include = 1)    
		AND EstimationLineLabor.LaborType > 0 
 
	-- User a cursor to loop through all of the line item IDs    
	DECLARE @cursor CURSOR	    
	SET @cursor = CURSOR FOR    
		SELECT DISTINCT EstimationLIneItemsID     
		FROM @LaborLines    
    
	DECLARE @lineItemID INT    
    
	OPEN @cursor    
	FETCH NEXT FROM @cursor    
	INTO @lineItemID    
    
	WHILE @@FETCH_STATUS = 0    
	BEGIN    
		DECLARE @LaborSummaryPaintPanel VARCHAR(200) = ''    
		DECLARE @LaborSummaryPaintLabor VARCHAR(200) = ''   
		DECLARE @LaborSummaryExtra VARCHAR(200) = ''   		   

		DECLARE @LaborSummaryPaintPanelPrint	VARCHAR(200) = ''    
		DECLARE @LaborSummaryPaintLaborPrint	VARCHAR(200) = ''  
		DECLARE @LaborSummaryExtraPrint			VARCHAR(200) = ''   		

		DECLARE @PaintMaterialsBetterment		REAL = 0    
		DECLARE @LaborBetterment				REAL = 0  
		DECLARE @PaintLaborBetterment			REAL = 0  
    
		DECLARE @LaborTotalPaintPanel REAL    
		DECLARE @LaborTotalPaint REAL   
		DECLARE @LaborTotalPaintDollars REAL 
		DECLARE @LaborTotalClearcoat REAL   
		DECLARE @LaborTotalClearcoatDollars REAL 
		DECLARE @LaborTotalExtra REAL    
		DECLARE @LaborExtraType VARCHAR(50) 
		DECLARE @LaborExtraIncludedInType VARCHAR(50) 
 
		DECLARE @LaborExtraRemovedType VARCHAR(50) = '' 
		DECLARE @LaborExtraRemovedIncludedInType VARCHAR(50) 
		DECLARE @LaborExtraRemovedHours REAL   
		DECLARE @LaborExtraRemovedTotal REAL  

		DECLARE @isPartsQuantityFlag		BIT
		DECLARE @partsQuantity				REAL
		DECLARE @isLaborQuantityFlag		BIT
		DECLARE @laborQuantity				REAL
		DECLARE @isPaintQuantityFlag		BIT  
		DECLARE @paintQuantity				REAL

		SELECT @isPartsQuantityFlag = IsPartsQuantityFlag, @partsQuantity = PartsQuantity, @isLaborQuantityFlag = IsLaborQuantityFlag,
			   @laborQuantity = LaborQuantity, @isPaintQuantityFlag = IsPaintQuantityFlag, @paintQuantity = PaintQuantity
		FROM   [dbo].[GetLineItemQuantitySummary](@lineItemID)

		-- Paint Panel is the "Paint Time" value in the paint section of a line item.  The LaborType can be different depending on the Paint Type selection 
		-- but they are all considered Paint Panel 
		SELECT @LaborSummaryPaintPanel = @LaborSummaryPaintPanel +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
								THEN '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
								ELSE '' 
							END    
						END   
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29) 
			
		SELECT @PaintLaborBetterment = @PaintLaborBetterment +     
				CASE     
					WHEN @DollarOrHours = 'h'    
						THEN    
							CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
								THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1)
								ELSE 0    
							END    
					ELSE     
						CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
							THEN CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)
							ELSE 0
						END    
				END    
		FROM @LaborLines AS LaborLines  
		WHERE     
		LaborLines.EstimationLineItemsID = @lineItemID     
		AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)

		-- @LaborSummaryPaintPanelPrint
		SELECT @LaborSummaryPaintPanelPrint = @LaborSummaryPaintPanelPrint +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
								THEN '$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)) + ' ' + LaborLines.LaborTypeName + ', '    
								ELSE '' 
							END    
					END    
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    

		-- @PaintMaterialsBetterment
		SELECT @PaintMaterialsBetterment = @PaintMaterialsBetterment +     
				CASE     
					WHEN @DollarOrHours = 'h'    
						THEN    
							CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
								THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1)
								ELSE 0   
							END    
					ELSE     
						CASE WHEN   LaborLines.LaborTime - LaborLines.DeductionAmount <> 0  
							THEN CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity) 
							ELSE 0 
						END    
				END    
		FROM @LaborLines AS LaborLines  
		WHERE     
		LaborLines.EstimationLineItemsID = @lineItemID     
		AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)

		SELECT @LaborTotalPaintPanel =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount)  * @paintQuantity, 0)    
			FROM @LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (9, 16, 17, 18, 19, 29)    
		
		-- The clearcoat labor is calculated seperately from the other paint types 
		SELECT @LaborTotalClearcoat =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0) 
			FROM @LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID = 20 
		
		SELECT @LaborTotalClearcoatDollars =     
			ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0) 
			FROM @LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID = 20 
		 
		-- All other paint types are added together 
		SELECT @LaborSummaryPaintLabor = @LaborSummaryPaintLabor +     
			CASE     
				WHEN @DollarOrHours = 'h'    
					THEN    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0     
							THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
							ELSE ''    
						END    
				ELSE     
					'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + LaborLines.LaborTypeName + ', '    
			END    
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 
    
		-- @LaborSummaryPaintLaborPrint
		SELECT @LaborSummaryPaintLaborPrint = @LaborSummaryPaintLaborPrint +     
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0     
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + LaborLines.LaborTypeName + ', '    
									ELSE ''    
								END    
						ELSE     
							'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity)) + ' ' + LaborLines.LaborTypeName + ', '    
					END    
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (20, 21, 22, 26, 27, 28)    -- Note clearcoat is in this list for display 

		SELECT @LaborTotalPaint = ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @paintQuantity, 0)    
			FROM @LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)     
 
		SELECT @LaborTotalPaintDollars = ISNULL(SUM((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @paintQuantity), 0)  
			FROM @LaborLines AS LaborLines    
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (21, 22, 26, 27, 28)  
 
		-- Extra labor is the Labor Time value in section 2 of the line item details, grouped by Labor Type. 
		SELECT 
			  @LaborExtraType = LaborTypes.LaborType 
			, @LaborExtraIncludedInType = LaborTypesMainIncludeIn.LaborType 
			FROM @LaborLines AS LaborLines  
			LEFT JOIN LaborTypes ON LaborTypes.id = LaborLines.LaborTypeID 
			LEFT OUTER JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND LaborTypes.RateTypesID  = CustomerProfileRates.RateType
			LEFT OUTER JOIN LaborTypes LaborTypesMainIncludeIn ON LaborTypesMainIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
    
		SELECT @LaborSummaryExtra = @LaborSummaryExtra +     
			CASE     
				WHEN @DollarOrHours = 'h'    
					THEN    
						CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
							THEN CAST(ROUND(LaborLines.LaborTime - LaborLines.DeductionAmount, 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
							ELSE ''    
						END    
				ELSE     
					'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate)) + ' ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '     
			END    
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)     
    

		SELECT @LaborSummaryExtraPrint = @LaborSummaryExtraPrint +    
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN CAST(ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 1) AS VARCHAR(10)) + ' hrs. ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '    
									ELSE ''    
								END    
						ELSE     
							'$' + CONVERT(VARCHAR, CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @LaborQuantity)) + ' ' + ISNULL(@LaborExtraIncludedInType, @LaborExtraType) + ', '     
					END  
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)

		SELECT @LaborBetterment = @LaborBetterment +    
					CASE     
						WHEN @DollarOrHours = 'h'    
							THEN    
								CASE WHEN LaborLines.LaborTime - LaborLines.DeductionAmount <> 0    
									THEN ROUND((LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 1)     
									ELSE 0    
								END    
						ELSE     
							CONVERT(MONEY, (LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate * @LaborQuantity)      
					END  
		FROM @LaborLines AS LaborLines  
		WHERE     
			LaborLines.EstimationLineItemsID = @lineItemID     
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)

		SELECT @LaborTotalExtra =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount) * @LaborQuantity, 0)    
			FROM @LaborLines AS LaborLines  
			WHERE LaborLines.EstimationLineItemsID = @lineItemID AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)   
		 
		-- Deleted extra labor.  This is extra labor that was in the previous supplement but not this one 
		SELECT @LaborExtraRemovedType = LaborLines.LaborTypeName   
		FROM @LaborLines LaborLines 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		SELECT  
			@LaborExtraRemovedType = LaborLines.LaborTypeName 
			, @LaborExtraRemovedIncludedInType = LaborTypesIncludeIn.LaborType 
		FROM @LaborLines LaborLines 
		LEFT JOIN LaborTypes ON LaborLines.LaborTypeID = LaborTypes.id 
		LEFT OUTER JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND LaborTypes.RateTypesID = CustomerProfileRates.RateType 
		LEFT OUTER JOIN LaborTypes LaborTypesIncludeIn ON LaborTypesIncludeIn.RateTypesID = CustomerProfileRates.IncludeIn AND CustomerProfileRates.IncludeIn > 0 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
    
		SELECT @LaborExtraRemovedHours =     
			ISNULL(SUM(LaborLines.LaborTime - LaborLines.DeductionAmount), 0) * -1 
			FROM @LaborLines LaborLines 
			WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
				AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
				AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		SELECT @LaborExtraRemovedTotal =     
			((LaborLines.LaborTime - LaborLines.DeductionAmount) * LaborLines.Rate) * -1  
		FROM @LaborLines LaborLines 
		WHERE LaborLines.EstimationLineItemsID = (SELECT DISTINCT ModifiesID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID NOT IN (SELECT LaborTypeID FROM @LaborLines WHERE EstimationLineItemsID = @lineItemID) 
			AND LaborLines.LaborTypeID IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
 
		INSERT INTO @LaborSummary (    
			EstimationLineItemsID,    
			LaborSummaryPaintPanel,    
			LaborTotalPaintPanel,    
			LaborSummaryPaint,    
			LaborTotalPaint,   
			LaborTotalPaintDollars, 
			LaborTotalClearcoat, 
			ClearcoatLaborIncluded,  
			ClearcoatSuppliesIncluded, 
			LaborSummaryExtra,    
			LaborTotalExtra, 
			LaborExtraType, 
			LaborExtraIncludedInType, 
			LaborExtraRemovedType, 
			LaborExtraRemovedIncludedInType, 
			LaborExtraRemovedHours, 
			LaborExtraRemovedTotal,
			LaborSummaryPaintPanelPrint,
			LaborSummaryPaintLaborPrint,
			LaborSummaryExtraPrint
			,IsPartsQuantity
			,IsLaborQuantity
			,IsPaintQuantity
			,PartsQuantity
			,LaborQuantity
			,PaintQuantity
			,PaintMaterialsBetterment,
			LaborBetterment,
			PaintLaborBetterment
		)    
		VALUES    
		(    
			@lineItemID,    
			SUBSTRING(@LaborSummaryPaintPanel, 0, LEN(@LaborSummaryPaintPanel)),    
			ROUND(@LaborTotalPaintPanel, 1),    
			SUBSTRING(@LaborSummaryPaintLabor, 0, LEN(@LaborSummaryPaintLabor)),    
			ROUND(@LaborTotalPaint, 1),   
			ROUND(@LaborTotalPaintDollars + @LaborTotalClearcoatDollars, 1),  
			ROUND(@LaborTotalClearcoat, 1),   
			@ClearCoatInPaint, 
			@ClearCoatSuppliesInPaint, 
			SUBSTRING(@LaborSummaryExtra, 0, LEN(@LaborSummaryExtra)),    
			ROUND(@LaborTotalExtra, 1), 
			ISNULL(@LaborExtraType, ''), 
			ISNULL(@LaborExtraIncludedInType, ''), 
			ISNULL(@LaborExtraRemovedType, ''),  
			ISNULL(@LaborExtraRemovedIncludedInType, ''), 
			ROUND(ISNULL(@LaborExtraRemovedHours, 0), 1), 
			ROUND(ISNULL(@LaborExtraRemovedTotal, 0), 2),
			SUBSTRING(@LaborSummaryPaintPanelPrint, 0, LEN(@LaborSummaryPaintPanelPrint)), 		
			SUBSTRING(@LaborSummaryPaintLaborPrint, 0, LEN(@LaborSummaryPaintLaborPrint)), 
			SUBSTRING(@LaborSummaryExtraPrint, 0, LEN(@LaborSummaryExtraPrint)),
			@IsPartsQuantityFlag,
			@IsLaborQuantityFlag,
			@IsPaintQuantityFlag,
			@partsQuantity,
			@laborQuantity,
			@paintQuantity,
			@PaintMaterialsBetterment,
			@LaborBetterment,
			@PaintLaborBetterment
		)    
    
		FETCH NEXT FROM @cursor    
		INTO @lineItemID    
	END     
	  
	RETURN   
END  
GO
