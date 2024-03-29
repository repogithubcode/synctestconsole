USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	2/20/2023 - EVB: This gets the Merchant ID and latest Wisetack callback
		info from the WisetackCallbackMerchantSignup table for the given
		LoginID. If there is no Callback info, returns the ID and link
		from the Logins table.

	EXEC [dbo].[Wisetack_MerchantInfo_Get]  @LoginID = 56027
*/
CREATE PROCEDURE [dbo].[Wisetack_MerchantInfo_Get] 
	@LoginID	int
AS
BEGIN
	SET NOCOUNT ON

	IF (NOT EXISTS(
		SELECT 1 FROM [WisetackCallbackMerchantSignup]
		JOIN	[Logins] ON [WisetackCallbackMerchantSignup].[MerchantID] = [Logins].[WisetackMerchantID]
		WHERE	[Logins].[ID] = @LoginID))
	BEGIN

		SELECT	NULL AS [CreatedDate]
				, NULL AS [CallbackDate]
				,[WisetackMerchantID] AS [MerchantID]
				,[WisetackSignupLink] AS [SignupLink]
				,NULL AS [Status]
				,NULL AS [EventType]
				,NULL AS [Reasons]
				,NULL AS [ExternalID]
				,NULL AS [OnboardingType]
				,NULL AS [TransactionsEnabled]
				,NULL AS [TransactionRange]
		FROM	[Logins] 
		WHERE	[Logins].[ID] = @LoginID

	END
	ELSE
	BEGIN

		SELECT	TOP 1
				[CreatedDate]
				,[CallbackDate]
				,[MerchantID]
				,[SignupLink]
				,[Status]
				,[EventType]
				,[Reasons]
				,[ExternalID]
				,[OnboardingType]
				,[TransactionsEnabled]
				,[TransactionRange]
		FROM	[WisetackCallbackMerchantSignup]
		JOIN	[Logins] ON [WisetackCallbackMerchantSignup].[MerchantID] = [Logins].[WisetackMerchantID]
		WHERE	[Logins].[ID] = @LoginID
		ORDER BY	[CallbackDate] DESC

	END

END
GO
