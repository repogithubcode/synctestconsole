USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 10/23/2019 
-- Description:	Get a Contract Add On by ID 
-- ============================================= 
CREATE PROCEDURE [dbo].[ContractAddOn_Get] 
	@id				int 
AS 
BEGIN 
	SELECT ContractAddOn.*, InvoicesPaid.Paid AS HasPayment 
	FROM ContractAddOn 
	LEFT OUTER JOIN  
	( 
		SELECT AddOnID, MAX(CAST(ISNULL(Paid, 0) AS int)) AS Paid 
		FROM Invoices 
		WHERE AddOnID = @id AND ISNULL(IsDeleted, 0) = 0  
		GROUP BY AddOnID 
	) AS InvoicesPaid ON ContractAddOn.ID = InvoicesPaid.AddOnID 
	WHERE ContractAddOn.ID = @id  
END 
GO
