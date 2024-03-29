USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[aaaGetActiveAddOnCounts]
AS
BEGIN
	
	SELECT ContractType.Type, Total
	FROM
	(
		SELECT AddOnType, COUNT(*) AS Total
		FROM
		(
			SELECT DISTINCT ContractAddOn.*
			FROM Contracts
			JOIN ContractAddOn ON Contracts.ContractID = ContractAddOn.ContractID
			JOIN Invoices ON Contracts.ContractID = Invoices.ContractID AND ContractAddOn.ID = Invoices.AddOnID
			WHERE
				Contracts.IsDeleted = 0
				AND ContractAddOn.IsDeleted = 0
				AND Contracts.Active = 1
				AND ContractAddOn.Active = 1
				AND GETDATE() BETWEEN Contracts.EffectiveDate AND Contracts.ExpirationDate
				AND Invoices.Paid = 1
		) AS Base
		GROUP BY AddOnType
	) AS Final
	LEFT OUTER JOIN ContractType ON Final.AddOnType = ContractType.ContractTypeID

END
GO
