USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	2/9/2023 - EVB: This inserts rows into WisetackCallbackLoanApplication, triggered from Wisetack callbacks
		during the loan application process.
*/
CREATE PROCEDURE [dbo].[WisetackCallbackLoanApplication_Insert] 
	@CreatedDate datetime,
	@TransactionID nvarchar(200),
	@CallbackDate datetime,
	@MessageID nvarchar(200) = NULL,
	@ChangedStatus nvarchar(100),
	@ActionsRequired nvarchar(2000) = NULL,
	@RequestedLoanAmount nvarchar(100),
	@ApprovedLoanAmount nvarchar(100) = NULL,
	@SettledLoanAmount nvarchar(100) = NULL,
	@ProcessingFee nvarchar(100) = NULL,
	@MaximumAmount nvarchar(100) = NULL,
	@TransactionPurpose nvarchar(200) = NULL,
	@ServiceCompletedOn datetime,
	@TilaAcceptedOn datetime = NULL,
	@LoanCreatedAt datetime = NULL,
	@EventType nvarchar(100) = NULL,
	@ExpirationDate datetime = NULL,
	@PrequalID nvarchar(200) = NULL,
	@CustomerID nvarchar(200) = NULL
AS
BEGIN
	SET NOCOUNT ON

	INSERT INTO WisetackCallbackLoanApplication 
		(
			[CreatedDate],
			[TransactionID],
			[CallbackDate],
			[MessageID],
			[ChangedStatus],
			[ActionsRequired],
			[RequestedLoanAmount],
			[ApprovedLoanAmount],
			[SettledLoanAmount],
			[ProcessingFee],
			[MaximumAmount],
			[TransactionPurpose],
			[ServiceCompletedOn],
			[TilaAcceptedOn],
			[LoanCreatedAt],
			[EventType],
			[ExpirationDate],
			[PrequalID],
			[CustomerID]
		) VALUES (
			@CreatedDate,
			@TransactionID,
			@CallbackDate,
			@MessageID,
			@ChangedStatus,
			@ActionsRequired,
			@RequestedLoanAmount,
			@ApprovedLoanAmount,
			@SettledLoanAmount,
			@ProcessingFee,
			@MaximumAmount,
			@TransactionPurpose,
			@ServiceCompletedOn,
			@TilaAcceptedOn,
			@LoanCreatedAt,
			@EventType,
			@ExpirationDate,
			@PrequalID,
			@CustomerID
		)

	SELECT scope_identity() AS ID

END
GO
