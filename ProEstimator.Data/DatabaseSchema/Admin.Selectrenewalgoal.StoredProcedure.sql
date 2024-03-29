USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- Author:    T. Gibson 
-- Create date: 6/3/2019 
-- Description:  Select Renewal Goal 
-- ============================================= 
CREATE PROCEDURE [Admin].[Selectrenewalgoal] @SalesRepId INT = NULL, 
                                             @BonusMonth INT = NULL, 
                                             @BonusYear  INT = NULL 
AS 
  BEGIN 
      SELECT [id], 
             [salesrepid], 
             [bonusmonth], 
             [bonusyear], 
             [renewalgoal1yr], 
             [renewalgoal2yr], 
             [salesgoal], 
             [salesbonus100], 
             [salesbonus110], 
             [salesbonus120], 
             [salesbonus130], 
             [renewalbonus1yr100], 
             [renewalbonus1yr110], 
             [renewalbonus120], 
             [renewalbonus130], 
             [createdate], 
             [modifieddate],
			 [SalesForcast]
      FROM   [dbo].[renewalgoal] 
      WHERE  [salesrepid] = @SalesRepId 
             AND [bonusmonth] = @BonusMonth 
             AND [bonusyear] = @BonusYear 
  END 



SET ANSI_NULLS ON
GO
