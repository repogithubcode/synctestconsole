USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure

-- =============================================
-- Author:		Ezra
-- Create date: 5/30/2023
-- Description:	Get over due invoices that have data in the failure log.
-- =============================================
CREATE PROCEDURE [dbo].[InvoiceFailureLog_GetSummary] 
	@DayWindow			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT 
		  Invoices.InvoiceID
		, Invoices.LoginID
		, Invoices.InvoiceAmount + Invoices.SalesTax AS InvoiceTotal
		, Invoices.DueDate
		, Invoices.Notes AS InvoiceNotes
		, Invoices.Summary AS InvoiceSummary
		, InvoiceFailureLog.TimeStamp AS LastFailStamp
		, InvoiceFailureLog.Note AS FailNote
		, CardLast4
		, CAST(DATEPART(mm,CardExpiration) AS VARCHAR(2)) + '/' + CAST(DATEPART(yyyy,CardExpiration) AS VARCHAR(4)) 'Expiration'
		, StripeCardID
	FROM InvoiceFailureLog with(nolock)
	LEFT OUTER JOIN Invoices with(nolock) ON InvoiceFailureLog.InvoiceID = Invoices.InvoiceID
	LEFT OUTER JOIN StripeInfo with(nolock) on InvoiceFailureLog.StripeInfoID = StripeInfo.ID
	WHERE 
		Invoices.Paid = 0
		AND (@DayWindow is null or Invoices.DueDate > DATEADD(dd, @DayWindow * -1, GETDATE()))
	ORDER BY InvoiceID, LastFailStamp DESC
END
GO
