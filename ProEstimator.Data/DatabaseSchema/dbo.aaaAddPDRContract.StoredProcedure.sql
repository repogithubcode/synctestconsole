USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[aaaAddPDRContract]
	@LoginID		INT
AS
BEGIN

	INSERT INTO Contract 
	(
		ContractTypeID, 
		ContractPriceLevelID,
		EffectiveDate,
		ExpirationDate,
		IsAutoPay,
		isSigned,
		Active,
		DateCreated
	)
	VALUES
	(
		7,
		481,
		GETDATE(),
		DATEADD(Year, 1, GETDATE()),
		0,
		1,
		1,
		GETDATE()
	)

	DECLARE @ContractID INT = @@IDENTITY

	INSERT INTO Invoice
	(
		LoginID,
		ContractID,
		PaymentNumber,
		InvoiceTypeID,
		InvoiceAmount,
		SalesTax,
		DueDate,
		Notes,
		Paid,
		DatePaid
	)
	VALUES
	(
		@LoginID,
		@ContractID,
		1,
		3,
		0,
		0,
		GETDATE(),
		'PDR From Script',
		1,
		GETDATE()
	)

	INSERT INTO ContractLogins
	(
		ContractID,
		LoginID,
		PrimaryLogin
	)
	VALUES 
	(
		@ContractID,
		@LoginID,
		1
	)
END
GO
