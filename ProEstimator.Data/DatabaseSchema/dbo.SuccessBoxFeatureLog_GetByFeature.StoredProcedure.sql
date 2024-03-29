USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--SuccessBoxFeatureLog_GetByFeature-06.sql
CREATE PROCEDURE [dbo].[SuccessBoxFeatureLog_GetByFeature]
	@LoginID int = 0,
	@Feature varchar(50) = '',
	@StartDate varchar(50) = '',
	@EndDate varchar(50) = '',
	@IncludeImpersonate bit = 0
AS 
BEGIN 
	SET NOCOUNT ON; 
 
    SELECT SuccessBoxFeatureLog.*, IsImpersonated
	FROM SuccessBoxFeatureLog with(nolock)
		Left Join ActiveLogin with(nolock) on SuccessBoxFeatureLog.ActiveLoginID = ActiveLogin.ID
	WHERE 
		isnull(SuccessBoxFeatureLog.LoginID, 0) = @LoginID
		AND (isnull(@Feature, '') = '' OR dbo.HtmlEncode(Feature) = @Feature)
		AND (isnull(@StartDate, '') = '' OR TimeStamp >= @StartDate)
		AND (isnull(@EndDate, '') = '' OR TimeStamp < dateadd(dd, 1, convert(datetime, @EndDate)))
		AND (isnull(@IncludeImpersonate, 0) = 1 OR isnull(IsImpersonated, 0) = 0)
	ORDER BY SuccessBoxFeatureLog.ID desc
END 
GO
