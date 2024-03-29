USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 7/18/2017 
-- Description:	Get all invoice records that are unpaid and due now 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetInvoices_AllDue]  
	@DueDate		DATETIME 
AS 
BEGIN 
 
	SELECT DISTINCT Invoices.*
	FROM Invoices 
 
	-- Only get invoices that are attached to a contract that has a payment 
	LEFT OUTER JOIN  
	( 
		SELECT DISTINCT ContractID AS ContractWithPaymentID
		FROM Invoices 
		WHERE Paid = 1 
	) AS HasPayment ON Invoices.ContractID = HasPayment.ContractWithPaymentID 

	-- Join in if the account is on auto renew
	LEFT OUTER JOIN 
	(
		SELECT LoginsAutoRenew.LoginID, LoginsAutoRenew.Enabled
		FROM
		(
			-- Get the most recent AutoRenew record for every login, then in the outer query join it back to the auto renew table to get the Enabled value for that record
			SELECT MAX(ID) ID, LoginID
			FROM LoginsAutoRenew
			GROUP BY LoginID
		) AS Base
		LEFT OUTER JOIN LoginsAutoRenew ON Base.ID = LoginsAutoRenew.ID
	) AS LoginsAutoRenew ON Invoices.LoginID = LoginsAutoRenew.LoginID
 
	-- Only get invoices for users who have stripe info and auto pay turned on 
	JOIN StripeInfo ON StripeInfo.LoginID = Invoices.LoginID AND ISNULL(StripeInfo.DeleteFlag, 0) = 0 AND ISNULL(StripeInfo.StripeCardID, '') <> '' 
 
	-- Ignore invoices who's contract or add on is deleted 
	LEFT OUTER JOIN Contracts ON Invoices.ContractID = Contracts.ContractID 
	LEFT OUTER JOIN ContractAddOn ON Invoices.AddOnID = ContractAddOn.ID 
	WHERE  
		Invoices.Paid = 0								-- Unpaid 
		AND Invoices.FailStamp IS NULL					-- Don't re-try invoices that have failed already 
		AND ISNULL(StripeInfo.StripeCardID, '') <> ''	-- Has a Stripe card ID 
		AND ISNULL(StripeInfo.AutoPay, 0) = 1			-- Auto pay is on 
		AND Contracts.ContractID IS NOT NULL			-- Has a contract 
		AND DueDate < DATEADD(dd, 0, GETDATE())			-- Due today or in the past 
		AND												-- Has no card error, or if there is a card error only if due within the last 3 months 
		( 
			ISNULL(StripeInfo.CardError, 0) = 0 
			OR (ISNULL(StripeInfo.CardError, 0) = 1 AND Invoices.DueDate > DATEADD(mm, -3, GETDATE())) 
		) 
		 
		AND (Invoices.IsDeleted = 0 AND ISNULL(Contracts.IsDeleted, 0) = 0 AND ISNULL(ContractAddOn.IsDeleted, 0) = 0)	-- Invoice, Contract, Add On not deleted 

		-- Get invoices that are a first payment for accounts with auto renew turned on
		AND (HasPayment.ContractWithPaymentID IS NOT NULL OR (LoginsAutoRenew.LoginID IS NOT NULL AND ISNULL(LoginsAutoRenew.Enabled, 0) = 1 AND PaymentNumber in (0, 1)))
	ORDER BY AddOnID		-- Pay base contracts first before add ons in case the card declines after the first payment
END 
GO
