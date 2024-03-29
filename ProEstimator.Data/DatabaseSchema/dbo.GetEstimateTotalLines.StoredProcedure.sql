USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


  
  
CREATE PROCEDURE [dbo].[GetEstimateTotalLines] --2335252,0   
 @AdminInfoID Int,      
 @Betterment Bit = 0      
AS      
SET NOCOUNT ON     

-- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.
	-- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component
	IF 1=0 BEGIN
      SET FMTONLY OFF
    END 
      
	    
	-- 11/9/2018 - Ezra - Using the new GetTotal function to get this data and convert it to the field schema needed by the existing report.
	
	DECLARE @Supplement INT = (SELECT LockLevel FROM EstimationData WHERE AdminInfoID = @AdminInfoID)

	create TABLE #totals 
	(
		SortOrder			int
		, Type				varchar(100)
		, Note				varchar(100)
		, Amount			float
	)
	INSERT INTO #totals
	EXEC EstimateReport_GetTotals @AdminInfoID = @AdminInfoID, @SupplementVersion = @Supplement

	SELECT [Type] AS LineDesc, Note AS OtherInfo, ROUND(ISNULL(Amount, 0), 2) AS Total, @AdminInfoID AS AdminInfoID, 0 AS Taxable
	FROM #totals

	/*

 DECLARE @Deductible Money      
 DECLARE @Payment Money      
 DECLARE @PaymentText VarChar(20)     
 Declare @paymentmade money     
      
      
  --This code is for adding multuple payments    
      
  Select  @paymentmade =SUM(Amount) from PaymentInfo     
  where AdminInfoID=@AdminInfoID group by AdminInfoID     
  --==================================================    
     
      
 SELECT @Deductible = ISNULL(Deductible, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID    
      
 SELECT @PaymentText = ItemText      
 FROM AdminInfoTOContacts WITH (NOLOCK)      
 INNER JOIN ContactItems WITH (NOLOCK) ON (ContactItems.ContactsID = AdminInfoTOContacts.ContactsID)      
 WHERE AdminInfoTOContacts.AdminInfoID = @AdminInfoID AND      
  ContactItems.ContactItemTypeID = 55      
      
 IF Dbo.CheckNumeric(@PaymentText) = 1       
 BEGIN      
  SELECT @Payment = CONVERT(Money, @PaymentText)      
 END       
 SELECT @Payment = ISNULL(@Payment,0)      
      
 CREATE TABLE [dbo].[#GetSubTotals2] (      
  [SupplementVersion2] TinyInt,      
  [RateName] [varchar] (25) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,      
  [CapType] [varchar] (28) COLLATE SQL_Latin1_General_CP1_CI_AS NULL ,      
  [Cap] [Real],      
  [FinalLineItemTotal] [float] NULL ,      
  [AdminInfoID] [int] NOT NULL ,      
  [BettermentType] [varchar] (1) NOT NULL ,      
  [BettermentAmount] [money] NOT NULL,
  [Taxable] [int] NULL ,      
  [Rate] [real] NULL ,      
  [Hours] [float] NULL      
 ) ON [PRIMARY]      
      
 --INSERT INTO #GetSubTotals2      
  EXECUTE spGetSubTotals @AdminInfoID = @AdminInfoID    

 -----------------------------------------------------------------------------------------------------------------------------
--SELECT * FROM #GetSubTotals2
      
 SELECT FinalLineItemTotal, AdminInfoID, BettermentType, Taxable      
 INTO #GetSubTotals      
 FROM #GetSubTotals2  
 
 -- Add a line for PDR

 UNION 
 SELECT 
SUM(PDR_Rate.Amount * CASE WHEN ISNULL(PDR_Multiplier.Value, 0) = 0 THEN 1 ELSE PDR_Multiplier.Value / 100 END) AS FinalLineItemTotal,
PDR_EstimateData.AdminInfoID, '' AS BettermentType, PDR_RateProfile.Taxable

FROM PDR_EstimateData

-- Join in PDR panels with selections
JOIN PDR_EstimateDataPanel ON PDR_EstimateDataPanel.AdminInfoID = PDR_EstimateData.AdminInfoID
	AND (PDR_EstimateDataPanel.QuantityID > 0 OR PDR_EstimateDataPanel.OversizedDents > 0)

-- Join the rate profile data
JOIN PDR_RateProfile ON PDR_EstimateData.RateProfileID = PDR_RateProfile.ID
JOIN PDR_Rate ON 
	PDR_RateProfile.ID = PDR_Rate.RateProfileID 
	AND PDR_Rate.Size = PDR_EstimateDataPanel.SizeID
	AND PDR_Rate.PanelID = PDR_EstimateDataPanel.PanelID
	AND PDR_Rate.Quantity = PDR_EstimateDataPanel.QuantityID

JOIN PDR_Panel ON PDR_EstimateDataPanel.PanelID = PDR_Panel.PanelID
LEFT OUTER JOIN PDR_Multiplier ON PDR_EstimateDataPanel.Multiplier = PDR_Multiplier.ID
WHERE PDR_EstimateData.AdminInfoID = @AdminInfoID
GROUP BY PDR_EstimateData.AdminInfoID, PDR_RateProfile.Taxable

 --SELECT * FROM #GetSubTotals   
 
 SELECT  
	CASE 
		WHEN ISNULL(Taxable,0) = 1 THEN 'Taxable Amount'      
		ELSE 'Nontaxable Amount'      
	END 'LineDesc',      
	CONVERT(VARCHAR(100), '') 'OtherInfo',      
	Sum(ISNULL(FinalLineItemTotal, 0)) 'Total',      
	AdminInfoID,      
	ISNULL(Taxable,0) 'Taxable'   
	
	   
INTO #TotalsLocal      
FROM #GetSubTotals GetSubTotals      
WHERE AdminInfoID = @AdminInfoID AND (ISNULL(BettermentType, '') <> '' OR @Betterment = 0)
GROUP BY ISNULL(Taxable,0), AdminInfoID      
ORDER BY ISNULL(Taxable,0) DESC      

--SELECT * FROM #TotalsLocal
       
SELECT *      
INTO #Totals2      
FROM #TotalsLocal      

UNION ALL      

SELECT 'Tax' 'LineDesc',      
	CASE  
		WHEN  
			ISNULL(CustomerProfilesMisc.SecondTaxRateStart,0) > 0 AND       
			ISNULL(CustomerProfilesMisc.SecondTaxRate,0) > 0 AND       
			Totals.Taxable <> 0 AND      
			Totals.Total > CustomerProfilesMisc.SecondTaxRateStart
			       
			THEN  CAST(Round(CustomerProfilesMisc.TaxRate, 3) AS VARCHAR) + '% for first $' +       
				dbo.Formatmoney(CustomerProfilesMisc.SecondTaxRateStart) + ', ' +    
				CAST(Round(CustomerProfilesMisc.SecondTaxRate, 3) AS VARCHAR) + '% after'    
				  
		ELSE CAST(Round(CustomerProfilesMisc.TaxRate, 3) AS VARCHAR) + '%'       
	END 'OtherInfo',      
	CASE  
		WHEN  ISNULL(CustomerProfilesMisc.SecondTaxRateStart,0) > 0 AND       
			ISNULL(CustomerProfilesMisc.SecondTaxRate,0) > 0 AND       
			Totals.Taxable <> 0 AND      
			Totals.Total > CustomerProfilesMisc.SecondTaxRateStart
		       
			THEN  CustomerProfilesMisc.SecondTaxRateStart * (CustomerProfilesMisc.TaxRate/100) +       
				(Totals.Total - CustomerProfilesMisc.SecondTaxRateStart) * (CustomerProfilesMisc.SecondTaxRate/100)      
			ELSE Totals.Total * (CustomerProfilesMisc.TaxRate/100)      
	END 'Total',      
	Totals.AdminInfoID,      
	0      
FROM #TotalsLocal Totals      
	INNER JOIN AdminInfo  ON (AdminInfo.id = Totals.AdminInfoID)      
	INNER JOIN CustomerProfiles  ON (CustomerProfiles.id = AdminInfo.CustomerProfilesID)      
	INNER JOIN CustomerProfilesMisc ON  (CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id)      
WHERE Totals.Taxable = 1      
  
--SELECT * FROM #Totals2

DECLARE @bettermentAmount MONEY = (SELECT SUM(BettermentAmount) FROM #GetSubTotals2)
 
--SELECT * FROM #GetSubTotals2
--SELECT * FROM #TotalsLocal  

SELECT distinct * FROM #Totals2      
WHERE LineDesc = 'Taxable Amount'      
      
UNION ALL        
SELECT distinct *      
FROM #Totals2      
WHERE NOT LineDesc IN ('Taxable Amount', 'Nontaxable Amount')      
      
UNION ALL      
SELECT *      
FROM #Totals2      
WHERE LineDesc = 'Nontaxable Amount'      

UNION ALL       
SELECT 'Grand Total',      
	'',      
	Sum(ISNULL(Total,0)),      
	@AdminInfoID,      
	0      
FROM #Totals2 T      
INNER JOIN AdminInfo ON (AdminInfo.id = T.AdminInfoID)      
WHERE T.AdminInfoID = @AdminInfoID AND 
(      
	CASE 
		WHEN @Betterment = 0 THEN ISNULL(-@Deductible,0)		
		ELSE ISNULL(@Deductible,0)      
	END >= 0 
	OR      
	CASE 
		WHEN @Betterment = 0 THEN ISNULL(-@Payment,0)      
		ELSE ISNULL(@Payment,0)      
	END >= 0 
	OR      
	CONVERT(Money,REPLACE(REPLACE(AdminInfo.BettermentTotal,'$',''),',','')) <> 0 
)      
GROUP BY T.AdminInfoID      
HAVING Sum(ISNULL(Total,0)) IS not NULL      

UNION ALL      
SELECT 'Less Deductible',      
	'',      
	ISNULL(-@Deductible,0),      
	@AdminInfoID,      
	0      
WHERE @Betterment = 0 AND ISNULL(@Deductible,0) >0      
       
       
--This code is for showing payment made on the report    
         
--UNION ALL      
--SELECT 'Payment Made',      
-- '',      
-- ISNULL(-@paymentmade,0),      
-- @AdminInfoID,      
-- 0      
--WHERE @Betterment = 0 AND      
-- ISNULL(@paymentmade,0) > 0      
--========================================    
        
UNION ALL      
	SELECT 'Deductible',      
	'',      
	ISNULL(@Deductible,0),      
	@AdminInfoID,      
	0      
WHERE @Betterment <> 0 AND ISNULL(@Deductible,0) > 0      
        
UNION ALL
SELECT 'Betterment', '', @bettermentAmount * -1, @AdminInfoID, 0
WHERE @bettermentAmount > 0

UNION ALL      
SELECT 'Net Total',      
	'',      
	Sum(ISNULL(T.Total,0)) +      
	CASE 
		WHEN @Betterment = 0 THEN ISNULL(-@Deductible,0)      
		ELSE ISNULL(@Deductible,0)      
	END -     
       
--This code is for deducting payment made from estimate    
       
-- - CASE WHEN @Betterment = 0 THEN ISNULL(-@paymentmade,0)      
-- ELSE ISNULL(@paymentmade,0)      
--END-     
--========    
       
	CASE 
		WHEN @Betterment = 0 THEN       
			CASE 
				WHEN ISNULL(@Deductible,0) > 0 THEN 0      
				ELSE ISNULL(@Payment,0)      
			END      
		ELSE 0      
	END-      
	CASE 
		WHEN @Betterment <> 0 THEN CONVERT(Money,REPLACE(REPLACE(AdminInfo.BettermentTotal,'$',''),',',''))      
		ELSE 0      
	END
	- @bettermentAmount,      
	@AdminInfoID,      
	0      
FROM #Totals2 T      
INNER JOIN AdminInfo ON  (AdminInfo.id = T.AdminInfoID)      
WHERE T.AdminInfoID = @AdminInfoID      
GROUP BY T.AdminInfoID, AdminInfo.BettermentTotal      
       
 DROP TABLE #GetSubTotals      
 DROP TABLE #GetSubTotals2      
 DROP TABLE #TotalsLocal      
 DROP TABLE #Totals2   

 */
GO
