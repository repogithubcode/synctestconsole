USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 7/18/2017
-- Description: Get all ContractTerms records.
-- =============================================
CREATE PROCEDURE [dbo].[GetAllContractTypes]

AS
BEGIN

	SELECT *
	FROM ContractTerms

END

GO
