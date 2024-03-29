USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 8/29/2019
-- Description:	Insert a record to the InvoicePaidTracking table
-- =============================================
CREATE PROCEDURE [dbo].[InvoicePaidTracking_Insert]
	  @InvoiceID			int
	, @Paid					bit
	, @SalesRepID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO InvoicePaidTracking
	(
		  InvoiceID
		, Paid
		, SalesRepID
		, DateTracked
	)
	VALUES
	(
		  @InvoiceID
		, @Paid
		, @SalesRepID
		, GETDATE()
	)
END
GO
