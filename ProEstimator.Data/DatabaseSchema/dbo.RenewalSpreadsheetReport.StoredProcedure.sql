USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/26/2021
-- =============================================
CREATE PROCEDURE [dbo].[RenewalSpreadsheetReport]
	@StartDate			DATE,
	@EndDate			DATE
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT  --*
		  Logins.ID AS CustomerID
		, tbl_Address.State
		, MIN(Invoices.DatePaid) AS SalesDate
		, Logins.CompanyName AS CustomerName
		, SUM(Invoices.InvoiceAmount) + SUM(Invoices.SalesTax) AS SalesPrice
		, ISNULL(Promo.PromoAmount, 0) AS Promo
		, ISNULL(ContractTerms.DepositAmount, 0) AS DownPayment
		, ISNULL(ContractPriceLevels.PaymentAmount, 0) AS MonthlyPayment
		, CASE WHEN AddOnFrame.ID IS NULL THEN 0 ELSE 1 END AS HasFrame  
		, CASE WHEN AddOnEMS.ID IS NULL THEN 0 ELSE 1 END AS HasEMS  
		, CASE WHEN AddOnMU.ID IS NULL THEN 0 ELSE 1 END AS HasMU  
		, ISNULL(SalesRep.SalesRepID, 0) AS SalesRepID
		, ISNULL(SalesRep.FirstName, '') AS SalesRep
	FROM Contracts
	LEFT OUTER JOIN Promo ON Contracts.PromoID = Promo.PromoID

	LEFT OUTER JOIN ContractPriceLevels ON Contracts.ContractPriceLevelID = ContractPriceLevels.ContractPriceLevelID
	LEFT OUTER JOIN ContractTerms ON ContractPriceLevels.ContractTermID = ContractTerms.ContractTermID

	LEFT OUTER JOIN ContractAddOn AS AddOnFrame ON Contracts.ContractID = AddOnFrame.ContractID AND AddOnFrame.IsDeleted = 0 AND AddOnFrame.Active = 1 AND AddOnFrame.AddOnType = 2  
	LEFT OUTER JOIN ContractAddOn AS AddOnEMS ON Contracts.ContractID = AddOnEMS.ContractID AND AddOnEMS.IsDeleted = 0 AND AddOnEMS.Active = 1 AND AddOnEMS.AddOnType = 5  
	LEFT OUTER JOIN ContractAddOn AS AddOnMU ON Contracts.ContractID = AddOnMU.ContractID AND AddOnMU.IsDeleted = 0 AND AddOnMU.Active = 1 AND AddOnMU.AddOnType = 8  

	LEFT JOIN Logins ON Contracts.LoginID = Logins.ID
	LEFT OUTER JOIN SalesRep ON Logins.SalesRepID = SalesRep.SalesRepID

	LEFT JOIN tbl_Address ON Logins.ContactID = tbl_Address.ContactsID
	LEFT JOIN Invoices ON Contracts.ContractID = Invoices.ContractID
	WHERE 
		Contracts.DateCreated BETWEEN @StartDate AND @EndDate
		AND Contracts.IsDeleted = 0
		AND ISNULL(Invoices.Paid, 0) = 1
		AND Invoices.IsDeleted = 0
	GROUP BY 
		Logins.ID
		, Contracts.EffectiveDate
		, tbl_Address.State
		, Logins.CompanyName
		, Promo.PromoAmount
		, ContractTerms.DepositAmount
		, ContractPriceLevels.PaymentAmount
		, AddOnFrame.ID
		, AddOnEMS.ID
		, AddOnMU.ID
		, SalesRep.SalesRepID
		, SalesRep.FirstName
END
GO
