USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
/*
7/31/2023 - EVB: Fixed issue with the handling expired loan apps that had no other activity

EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get]
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @LoginID = 56027
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @StartDate = '1/20/2023', @EndDate = '7/14/2023'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @SignupStatus = 'APPLICATION_APPROVED'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @SignupStatus = 'APPLICATION_SUBMITTED'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @SignupStatus = 'InProgress'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @MerchantName = 'EB'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @MerchantName = 'xx'
EXEC [dbo].[Wisetack_FinancingReportMerchantSignups_Get] @MinLoanAppCount = 1
*/
CREATE PROCEDURE [dbo].[Wisetack_FinancingReportMerchantSignups_Get]
	@StartDate DATETIME = null,
	@EndDate DATETIME = null,
	@MerchantName VARCHAR(200) = null,	
	@LoginID INT = null,
	@SignupStatus VARCHAR(200) = null,
	@MinLoanAppCount INT = 0
AS
BEGIN
SET NOCOUNT ON

	SELECT		LoginID,
				MerchantID,
				MerchantName,
				SignupLink,
				LastCallbackDate,
				SignupStatus,
				LoanAppCount
	FROM
	(
		SELECT		l.ID AS LoginID,
					l.WisetackMerchantID AS MerchantID,
					l.CompanyName AS MerchantName,
					l.WisetackSignupLink AS SignupLink,
					wcm.CreatedDate AS LastCallbackDate,
					wcm.Status AS SignupStatus,
					ISNULL((SELECT			COUNT(DISTINCT app.TransactionID) 
						FROM		WisetackCallbackLoanApplication app WITH (NOLOCK)
						JOIN		AdminInfo WITH (NOLOCK) ON app.TransactionID = AdminInfo.WisetackTransactionID
						WHERE		AdminInfo.CreatorID = l.ID  
									AND (SELECT Count(1) FROM WisetackCallbackLoanApplication la WHERE la.TransactionID = AdminInfo.WisetackTransactionID AND la.ChangedStatus <> 'EXPIRED') > 0
						), 0) AS LoanAppCount,
					ROW_NUMBER() OVER (PARTITION BY l.WisetackMerchantID ORDER BY CreatedDate DESC) as RowNumber
		FROM		logins l WITH (NOLOCK)
		LEFT JOIN	WisetackCallbackMerchantSignup wcm WITH (NOLOCK) ON l.WisetackMerchantID = wcm.MerchantID
		WHERE		ISNULL(l.WisetackSignupLink, '') <> '' 

	) AS a
	WHERE	RowNumber = 1 
			AND (ISNULL(@StartDate, '') = '' OR LastCallbackDate >= @StartDate)
			AND (ISNULL(@EndDate, '') = '' OR LastCallbackDate <= @EndDate)
			AND (ISNULL(@MerchantName, '') = '' OR MerchantName LIKE '%' + @MerchantName + '%')
			AND (ISNULL(@LoginID, 0) = 0 OR LoginID = @LoginID)
			AND (ISNULL(@SignupStatus, '') = '' OR SignupStatus = @SignupStatus OR 
				(@SignupStatus = 'InProgress' 
				AND ISNULL(SignupStatus, '') <> 'APPLICATION_APPROVED' 
				AND ISNULL(SignupStatus, '') <> 'APPLICATION_SUBMITTED' 
				AND ISNULL(SignupStatus, '') <> 'APPLICATION_DECLINED') OR
				(@SignupStatus = 'InProgressSubmitted' 
				AND ISNULL(SignupStatus, '') <> 'APPLICATION_APPROVED' 
				AND ISNULL(SignupStatus, '') <> 'APPLICATION_DECLINED'))
			AND (@MinLoanAppCount <= 0 OR LoanAppCount >= @MinLoanAppCount)
	ORDER BY LastCallbackDate DESC

END
GO
