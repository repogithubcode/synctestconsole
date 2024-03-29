USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
  
  
  
CREATE PROCEDURE [dbo].[EMS_Export_GetTotals2]  
	@AdminInfoID Int  
AS  
  
SET NOCOUNT ON  
SET FMTONLY OFF  


declare @CreatorID int

select @CreatorID = CreatorID from AdminInfo as a where id = @AdminInfoID

if @CreatorID = 56027
begin
 exec [dbo].[EMS_Export_GetTotals2_201] @AdminInfoID 
end 
else 
begin

  
  create table #totals   
 ( 
	SortOrder		int, 
	Type			varchar(20), 
	Note			varchar(50), 
	Amount			money 
 ) 
 
create TABLE #previousTotals  
 ( 
	SortOrder		int, 
	Type			varchar(20), 
	Note			varchar(50), 
	Amount			money 
 ) 
 if 1 =1
 begin
 DECLARE @suppVersion INT = (SELECT ISNULL(LockLevel, 0) FROM EstimationData WHERE AdminInfoID = @AdminInfoID) 
  
INSERT INTO #totals 
EXEC  [dbo].[EstimateReport_GetTotals]  
	@AdminInfoID = @AdminInfoID, 
	@SupplementVersion = @suppVersion,  
	@ForEOR = 1,  
	@AlwaysIncludeGT = 1    
 
 
 
IF @suppVersion > 0  
	BEGIN 
		DECLARE @previousSupp INT = @suppVersion - 1 
 
		 INSERT INTO #previousTotals 
		 EXEC  [dbo].[EstimateReport_GetTotals]  
			@AdminInfoID = @AdminInfoID, 
			@SupplementVersion = @previousSupp,  
			@ForEOR = 1,  
			@AlwaysIncludeGT = 1 
 
			 
	END 
 
SELECT  
	ROUND(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Grand Total'), 0), 2) AS G_TTL_AMT, 
	ROUND(ISNULL((SELECT Amount FROM #totals WHERE type = 'Betterment'), 0) * -1, 2) AS G_BETT_AMT, 
	0 G_RPD_AMT,  
	ROUND(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Less Deductible'), 0) * -1, 2) AS G_DED_AMT,  
	round(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Grand Total'), 0), 2)		 G_CUST_AMT,  
	0 G_AA_AMT,  
	ROUND(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Net Total'), 0), 2) AS N_TTL_AMT,  
	CASE WHEN @suppVersion > 0 THEN round(ISNULL((SELECT Amount FROM #previousTotals WHERE Type = 'Grand Total'), 0), 2) ELSE 0 END PREV_NET,  
	CASE WHEN @suppVersion > 0 THEN round(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Grand Total'), 0) - ISNULL((SELECT Amount FROM #previousTotals WHERE Type = 'Grand Total'), 0), 2) ELSE 0 END SUPP_AMT,  
	CASE WHEN @suppVersion > 0 THEN round(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Net Total'), 0) - ISNULL((SELECT Amount FROM #previousTotals WHERE Type = 'Net Total'), 0), 2) ELSE 0 END  N_SUPP_ANT,  
	round(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Grand Total'), 0), 2) G_UPD_AMT,  
	0 G_TTL_DISC,  
	ROUND(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Tax'), 0), 2) AS G_TAX,  
	CASE WHEN @suppVersion > 0 THEN ROUND(ISNULL((SELECT Amount FROM #totals WHERE Type = 'Tax'), 0) - ISNULL((SELECT Amount FROM #previousTotals WHERE Type = 'Tax'), 0), 2) ELSE 0 END AS GST_AMT   
 
--DECLARE @PolicyNumber VarChar(25)  
--DECLARE @ClaimNumber VarChar(25)  
  
--DECLARE @TaxRate Real  
--DECLARE @SecondTaxRate Real  
--DECLARE @SecondTaxRateStart Real  
--DECLARE @Deductible Money  
--DECLARE @DeductibleText VarChar(20)  
  
--SELECT @DeductibleText = ItemText  
--FROM AdminInfoTOContacts AdminInfoTOContacts WITH (NOLOCK)  
--INNER JOIN ContactItems ContactItems WITH (NOLOCK) ON  
--	(ContactItems.ContactsID = AdminInfoTOContacts.ContactsID)  
--WHERE AdminInfoTOContacts.AdminInfoID = @AdminInfoID AND  
--	ContactItems.ContactItemTypeID = 40  
  
--IF dbo.CheckNumeric(@DeductibleText) = 1	  
--BEGIN  
--	SELECT @Deductible = CONVERT(Money, @DeductibleText)  
--END	  
--SELECT @Deductible = ISNULL(@Deductible,0)  
  
--CREATE TABLE #Totals (	LineDesc VarChar(500) NULL,  
--			OtherInfo VarChar(500) NULL,  
--			Total Real,  
--			AdminInfoID Int,  
--			Taxable Bit)  
--INSERT INTO #Totals  
--EXECUTE GetEstimateTotalLines @AdminInfoID = @AdminInfoID, @Betterment =0  
  
  
--CREATE TABLE #TotalsB (	LineDesc VarChar(500) NULL,  
--			OtherInfo VarChar(500) NULL,  
--			Total Real,  
--			AdminInfoID Int,  
--			Taxable Bit)  
--INSERT INTO #TotalsB  
--EXECUTE GetEstimateTotalLines @AdminInfoID = @AdminInfoID, @Betterment =1  
  
  
--SELECT @TaxRate = CustomerProfilesMisc.TaxRate,  
--	@SecondTaxRate = CustomerProfilesMisc.SecondTaxRate,  
--	@SecondTaxRateStart = CustomerProfilesMisc.SecondTaxRateStart  
--FROM AdminInfo AdminInfo WITH (NOLOCK)  
--INNER JOIN CustomerProfiles CustomerProfiles WITH (NOLOCK) ON  
--	(CustomerProfiles.id = AdminInfo.CustomerProfilesID)  
--INNER JOIN CustomerProfilesMisc CustomerProfilesMisc WITH (NOLOCK) ON  
--	(CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.ID)  
--WHERE AdminInfo.Id = @AdminInfoID  
  
--CREATE TABLE #EMSTotals2 (  
--	SubTotal Money,  
--	TaxableTotal Money,  
--	NonTaxableTotal Money,  
--	TaxRate Real,  
--	SecondTaxRateStart Money,  
--	SecondTaxRate Real,  
--	TaxTotal Money,  
--	Total Money)  
  
--INSERT INTO #EMSTotals2  
--	SELECT 	( SELECT SUM(Total) FROM #Totals WHERE LineDesc IN ('Taxable Amount','Nontaxable Amount') ) 'SubTotal',  
--		( SELECT Total FROM #Totals WHERE LineDesc = 'Taxable Amount') 'TaxableTotal',  
--		( SELECT Total FROM #Totals WHERE LineDesc = 'Nontaxable Amount') 'NonTaxableTotal',  
--		@TaxRate,  
--		@SecondTaxRateStart,  
--		@SecondTaxRate,  
--		( SELECT Total FROM #Totals WHERE LineDesc = 'Tax') 'TaxTotal',  
--		( SELECT Total FROM #Totals WHERE LineDesc = 'Net Total') 'Total'  
  
  
--CREATE TABLE #EMSTotals2Betterment (  
--	SubTotal Money,  
--	TaxableTotal Money,  
--	NonTaxableTotal Money,  
--	TaxRate Real,  
--	SecondTaxRateStart Money,  
--	SecondTaxRate Real,  
--	TaxTotal Money,  
--	Total Money)  
  
--INSERT INTO #EMSTotals2Betterment  
--	SELECT 	( SELECT SUM(Total) FROM #TotalsB WHERE LineDesc IN ('Taxable Amount','Nontaxable Amount') ) 'SubTotal',  
--		( SELECT Total FROM #TotalsB WHERE LineDesc = 'Taxable Amount') 'TaxableTotal',  
--		( SELECT Total FROM #TotalsB WHERE LineDesc = 'Nontaxable Amount') 'NonTaxableTotal',  
--		@TaxRate,  
--		@SecondTaxRateStart,  
--		@SecondTaxRate,  
--		( SELECT Total FROM #TotalsB WHERE LineDesc = 'Tax') 'TaxTotal',  
--		( SELECT Total FROM #TotalsB WHERE LineDesc = 'Net Total') 'Total'  
  
  
--SELECT  
--	round(isnull(T1.SubTotal,ISNULL(T1.Total,0) + ISNULL(T2.Total,0)	) + 	ISNULL(T1.TaxTotal,0) + ISNULL(T2.TaxTotal,0),2)		G_TTL_AMT,  
--	round(isnull(T2.Total,0),2)			G_BETT_AMT,  
--	0 G_RPD_AMT,  
--	round(@Deductible,2)			 G_DED_AMT,  
--	round(isnull(T2.Total,0),2) + round(isnull(@Deductible,0),2)		 G_CUST_AMT,  
--	0 G_AA_AMT,  
--	round(ISNULL(T1.Total,0),2) + round(ISNULL(T2.Total,0),2)		N_TTL_AMT,  
--	0 PREV_NET,  
--	0 SUPP_AMT,  
--	0 N_SUPP_ANT,  
--	0 G_UPD_AMT,  
--	0 G_TTL_DISC,  
--	ISNULL(T1.TaxTotal,0) + ISNULL(T2.TaxTotal,0)	G_TAX,  
--	0 GST_AMT   
--FROM #EMSTotals2 T1  
--INNER JOIN #EMSTotals2Betterment T2 ON (1=1)  
  
--DROP TABLE #EMSTotals2  
--DROP TABLE #EMSTotals2Betterment  
--DROP TABLE #Totals  
--DROP TABLE #TotalsB  
  
  
  
  end
  end
  
  
GO
