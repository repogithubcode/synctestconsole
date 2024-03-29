USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
	2/20/2023 - EVB: This updates the Merchant ID on the Logins table for a
		given LoginID
*/
CREATE PROCEDURE [dbo].[Wisetack_LoginsMerchantID_Update] 
	@LoginID	int,
	@MerchantID	nvarchar(200),
	@SignupLink	nvarchar(200)
AS
BEGIN
	SET NOCOUNT ON

	UPDATE	[Logins]
	SET		[WisetackMerchantID] = @MerchantID,
			[WisetackSignupLink] = @SignupLink
	WHERE	[ID] = @LoginID

END
GO
