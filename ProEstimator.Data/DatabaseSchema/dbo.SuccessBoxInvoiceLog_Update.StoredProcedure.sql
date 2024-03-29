USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/4/2020
-- Description:	Add or update a Success Box Invoice Synce Log record
-- =============================================
CREATE PROCEDURE [dbo].[SuccessBoxInvoiceLog_Update]
	  @InvoiceID				int
	, @IsSynced					bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF EXISTS (SELECT * FROM SuccessBoxInvoiceLog WHERE InvoiceID = @InvoiceID)
		BEGIN
			UPDATE SuccessBoxInvoiceLog
			SET IsSynced = @IsSynced
			WHERE InvoiceID = @InvoiceID
		END
	ELSE
		BEGIN
			INSERT INTO SuccessBoxInvoiceLog
			(InvoiceID, IsSynced)
			VALUES (@InvoiceID, @IsSynced)
		END

END
GO
