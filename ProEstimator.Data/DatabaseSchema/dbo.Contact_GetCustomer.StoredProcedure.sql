USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/22/2019
-- Description:	Get a customer contact record for an estimate ID.
-- =============================================
CREATE PROCEDURE [dbo].[Contact_GetCustomer]
	@EstimateID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT TOP 1 *
	FROM tbl_ContactPerson
	WHERE
		AdminInfoID = @EstimateID
		AND ContactTypeID = 1
		AND ContactSubTypeID = 4
END
GO
