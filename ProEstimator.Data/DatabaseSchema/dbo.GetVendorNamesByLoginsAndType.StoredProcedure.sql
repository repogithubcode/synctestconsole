USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/15/2017
-- Description:	Gets the vendor names and ids linked to a Logins ID for a particular type
-- =============================================
CREATE PROCEDURE [dbo].[GetVendorNamesByLoginsAndType]
	@LoginsID	int,
	@Type		varchar(25)
AS
BEGIN

	SELECT CompanyIDCode AS id, CompanyName AS CompanyInfo
	FROM SelectedVendor
	LEFT OUTER JOIN Vendor ON SelectedVendor.VendorID = Vendor.ID
	WHERE 
		SelectedVendor.LoginID = @LoginsID
		AND CompanyIDCode IS NOT NULL
		AND Vendor.Type = @Type
	ORDER BY 
		CompanyName

END

GO
