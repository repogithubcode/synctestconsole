USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetContractTypes]
	@ContractTypeID int = NULL
AS

/*

EXECUTE GetContractTypes
	@ContractTypeID = 2

*/

SELECT		ContractTypeID,
			Type AS ContractType,
			ProductCode,
			ProductDescription
FROM		ContractType
WHERE		ContractTypeID = isNull(@ContractTypeID, ContractTypeID)

GO
