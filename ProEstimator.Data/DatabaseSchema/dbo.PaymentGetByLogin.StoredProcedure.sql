USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/17/2017 
-- Description:	Get all payments for a Login 
-- ============================================= 
CREATE PROCEDURE [dbo].[PaymentGetByLogin] 
	@loginID			int 
AS 
BEGIN 
	SELECT * 
	FROM Payment 
	WHERE LoginID = @loginID 
	ORDER BY TimeStamp 
END 
GO
