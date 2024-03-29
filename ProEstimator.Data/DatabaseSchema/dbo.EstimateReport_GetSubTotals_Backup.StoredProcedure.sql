USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================   
-- Author:		Ezra   
-- Create date: 4/24/2018   
-- Description:	Get sub totals for an estimate.   
--				Replaces GetEstimateSubTotalsNew   
-- =============================================   
CREATE PROCEDURE [dbo].[EstimateReport_GetSubTotals_Backup]    
	@AdminInfoID Int,   
	@SupplementVersion TinyInt,   
	@EOR BIT = 1,   
	@ImageDirectory VARCHAR(200) = ''   
AS   
BEGIN   
   
	 -- This strange piece of code is necessary for the Telerek designer to work with a sp that uses temp tables.    
	 -- http://www.telerik.com/support/kb/reporting/details/how-to-configure-stored-procedure-with-temporary-tables-for-use-with-sqldatasource-component    
	 IF 1=0 BEGIN    
       SET FMTONLY OFF    
     END    
   
	IF @EOR = 1   
		BEGIN  
			SELECT *   
			FROM dbo.GetEstimateSubTotalsFunction(@AdminInfoID, @SupplementVersion, @ImageDirectory)  
			WHERE FinalLineItemTotal > 0 OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')  
		END  
	ELSE  
		BEGIN  
  
			DECLARE @ThisVersion TABLE    
			(   
				RateName				varchar(50),    
				FinalLineItemTotal		float,    
				Taxable					bit,    
				Rate					float,    
				Hours					float,    
				SortOrder				varchar(50),  
				Notes					varchar(300),  
				CapType					int,  
				Cap						real  
			)   
  
			INSERT INTO @ThisVersion  
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion, '') AS ThisVersion  
			WHERE FinalLineItemTotal > 0 OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')  
  
			DECLARE @LastVersion TABLE    
			(   
				RateName				varchar(50),    
				FinalLineItemTotal		float,    
				Taxable					bit,    
				Rate					float,    
				Hours					float,    
				SortOrder				varchar(50),  
				Notes					varchar(300),  
				CapType					int,  
				Cap						real  
			)   
  
			INSERT INTO @LastVersion  
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion - 1, '') AS ThisVersion  
			WHERE FinalLineItemTotal > 0 OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')  
  
			SELECT *  
			FROM  
			(  
				SELECT  
					  ISNULL(Last.RateName, This.RateName) AS RateName  
					,   
						CASE WHEN This.FinalLineItemTotal = Last.FinalLineItemTotal THEN 0  
						ELSE  
							ISNULL(This.FinalLineItemTotal, 0) - ISNULL(Last.FinalLineItemTotal, 0)   
						END AS FinalLineItemTotal  
					, ISNULL(This.Taxable, Last.Taxable) AS Taxable  
					, ISNULL(This.Rate, Last.Rate) AS Rate  
					, ISNULL(This.Hours, 0) - ISNULL(Last.Hours, 0) AS Hours  
					, ISNULL(This.SortOrder, Last.SortOrder) AS SortOrder  
					, ISNULL(ISNULL(This.Notes, Last.Notes), '') AS Notes  
					, ISNULL(This.CapType, Last.CapType) AS CapType  
					, ISNULL(This.Cap, Last.Cap) AS Cap  
				FROM @ThisVersion This  
				FULL OUTER JOIN @LastVersion Last ON Last.RateName = This.RateName  
			) AS Final  
			WHERE FinalLineItemTotal <> 0 OR Hours <> 0  
    OPTION(RECOMPILE)
		END  
   
END   
GO
