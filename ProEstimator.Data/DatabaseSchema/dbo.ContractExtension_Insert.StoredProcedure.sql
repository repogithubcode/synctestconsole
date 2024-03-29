USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[ContractExtension_Insert] 
	@LoginID			int
	, @SalesRep			int
	, @ExtendFrom		datetime
	, @Days				int
	, @ExtendTo			datetime
	, @TrialID			int = NULL
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO [dbo].[ContractExtension]
	(
			[LoginID]
		,[SalesRep]
		,[extendFrom]
		,[Days]
		,[ExtendTo]
		,[ExtendedDate]
		,[TrialID]
	)
	VALUES
	(
			@LoginID
		,@SalesRep
		,@extendFrom
		,@Days
		,@ExtendTo
		,GETDATE()
		,@TrialID
	)

	SELECT CAST(SCOPE_IDENTITY() AS INT)
END
GO
