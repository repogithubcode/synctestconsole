USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================  
-- Author:		Ezra  
-- Create date: 8/7/2020  
-- Description:	Renewal Goal, get for user / month / year  
-- =============================================  
CREATE PROCEDURE [dbo].[RenewalGoal_Get]   
	  @SalesRepId INT   
    , @BonusMonth INT  
    , @BonusYear  INT   
AS  
BEGIN  
	-- SET NOCOUNT ON added to prevent extra result sets from  
	-- interfering with SELECT statements.  
	SET NOCOUNT ON;  
  
	IF (@SalesRepId > -1)  
			BEGIN  
				SELECT *, [dbo].[getActualSales] (@SalesRepId, @BonusMonth, @BonusYear) AS ActualSales 
				FROM RenewalGoal  
				WHERE   
					SalesRepId = @SalesRepId  
					AND BonusMonth = @BonusMonth   
					AND BonusYear = @BonusYear   
			END  
		ELSE  
			BEGIN  
				SELECT   
					0 AS ID  
					, -1 AS SalesRepId  
					, @BonusMonth AS BonusMonth  
					, @BonusYear AS BonusYear  
					, ISNULL(Round(AVG(RenewalGoal1Yr),2), 0) AS RenewalGoal1Yr  
					, ISNULL(Round(AVG(RenewalGoal2Yr),2), 0) AS RenewalGoal2Yr  
					, ISNULL(SUM(SalesGoal), 0) AS SalesGoal  
					, ISNULL(AVG(SalesBonus100), 0) AS SalesBonus100  
					, ISNULL(AVG(SalesBonus110), 0) AS SalesBonus110  
					, ISNULL(AVG(SalesBonus120), 0) AS SalesBonus120  
					, ISNULL(AVG(SalesBonus130), 0) AS SalesBonus130  
					, ISNULL(AVG(RenewalBonus1Yr100), 0) AS RenewalBonus1Yr100  
					, ISNULL(AVG(RenewalBonus1Yr110), 0) AS RenewalBonus1Yr110  
					, ISNULL(AVG(RenewalBonus120), 0) AS RenewalBonus120  
					, ISNULL(AVG(RenewalBonus130), 0) AS RenewalBonus130  
					, ISNULL(MAX(CreateDate), GETDATE()) AS CreateDate  
					, ISNULL(MAX(ModifiedDate), GETDATE()) AS ModifiedDate  
					, ISNULL(SUM(SalesForcast), 0) AS SalesForcast  
					, [dbo].[getActualSales] (@SalesRepId, @BonusMonth, @BonusYear) AS ActualSales 
				FROM RenewalGoal  
				WHERE   
					BonusMonth = @BonusMonth   
					AND BonusYear = @BonusYear 
					AND RenewalGoal1Yr > 1		-- Ignore defaults that have no data yet  
  
			END  
END  
GO
