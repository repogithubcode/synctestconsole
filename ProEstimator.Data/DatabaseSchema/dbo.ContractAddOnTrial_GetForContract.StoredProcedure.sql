USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 4/14/2020 
-- Description:	Get all Contract Add On Trialss by contract ID 
-- ============================================= 
CREATE PROCEDURE [dbo].[ContractAddOnTrial_GetForContract] 
	  @contractID		int 
	, @Deleted			bit = 0 
AS 
BEGIN 
	SELECT * 
	FROM ContractAddOnTrial 
	WHERE  
		ContractID = @contractID  
		AND (IsDeleted = 0 OR @Deleted = 1) 
END 
GO
