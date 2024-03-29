USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/1/2019 
-- Description:	Get labor rates for an estimate.  NOTE: this is for the original add parts page and should not be needed when replaced 
-- ============================================= 
CREATE PROCEDURE [dbo].[CustomerProfile_GetRates]  
	@EstimateID			int 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
     SELECT RateTypes.id, RateTypes.RateName, CustomerProfileRates.Rate       
	 FROM AdminInfo WITH(NOLOCK)       
	 INNER JOIN CustomerProfileRates WITH (NOLOCK) ON       
	  (CustomerProfileRates.CustomerProfilesID = AdminInfo.CustomerProfilesID)       
	 INNER JOIN RateTypes WITH (NOLOCK) ON       
	  (RateTypes.ID = CustomerProfileRates.RateType)       
	 WHERE AdminInfo.ID = @EstimateID AND       
	  RateTypes.ID IN (1, 2, 3, 4, 5, 12, 14, 15 ,16 ,17, 20, 21)       
	 ORDER BY CustomerProfileRates.RateType       
END 
GO
