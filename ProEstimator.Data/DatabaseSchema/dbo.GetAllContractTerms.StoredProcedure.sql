USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 7/19/2017
-- Description:	Get all ContractTerms records.  The list is small so they are cached by the application.
-- =============================================
CREATE PROCEDURE [dbo].[GetAllContractTerms] 

AS
BEGIN
	SELECT *
	FROM ContractTerms
END

GO
