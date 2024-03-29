USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 10/22/2019
-- Description:	Delete a non-committed contract and its invoices.  This happens when the user decides to pick a different contract before paying for the selected one.
-- =============================================
CREATE PROCEDURE [dbo].[Contract_DeleteUncommitted]
	@ContractID					int
	, @DeleteAddOns				bit = 0
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    DELETE FROM Contracts
	WHERE ContractID = @ContractID

	DELETE FROM Invoices
	WHERE ContractID = @ContractID AND (@DeleteAddOns = 1 OR AddOnID = 0)

	IF @DeleteAddOns = 1
		BEGIN
			DELETE FROM ContractAddOn
			WHERE ContractID = @ContractID
		END

END
GO
