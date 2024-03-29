USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 11/21/2019
-- Description:	Log when the end date for a contract, trial, or add on gets changed
-- =============================================
CREATE PROCEDURE [dbo].[ContractEndDateChangeLog_Insert]
	  @ContractID		int
	, @TrialID			int
	, @OldDate			date
	, @NewDate			date
	, @SalesRepID		int
	, @TimeStamp		datetime
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO ContractEndDateChangeLog
	(
		  ContractID
		, TrialID
		, OldDate
		, NewDate
		, SalesRepID
		, TimeStamp
	)
	VALUES
	(
		@ContractID
		, @TrialID
		, @OldDate
		, @NewDate
		, @SalesRepID
		, @TimeStamp
	)
END
GO
