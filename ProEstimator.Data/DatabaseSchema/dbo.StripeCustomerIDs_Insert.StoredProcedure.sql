USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 4/12/2023
-- Description:	Insert a StripeCustomerIDs record
-- =============================================
CREATE PROCEDURE [dbo].[StripeCustomerIDs_Insert]
	  @LoginID			int
	, @StripeCustomerID	varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO StripeCustomerIDs
	(
		  LoginID
		, StripeCustomerID
		, TimeStamp
	)
	VALUES
	(
		  @LoginID
		, @StripeCustomerID
		, GETDATE()
	)
END
GO
