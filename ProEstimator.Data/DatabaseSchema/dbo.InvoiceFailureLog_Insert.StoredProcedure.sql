USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 5/19/2023
-- Description:	Insert an InvoiceFailureLog record
-- =============================================
CREATE PROCEDURE [dbo].[InvoiceFailureLog_Insert]
	  @InvoiceID		int
	, @Note				varchar(200)
	, @StripeInfoID		int
	, @AutoPay			bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO InvoiceFailureLog
	(
		  InvoiceID
		, TimeStamp
		, Note
		, StripeInfoID
		, AutoPay
	)
	VALUES
	(
		  @InvoiceID
		, GETDATE()
		, @Note
		, @StripeInfoID
		, @AutoPay
	)
END
GO
