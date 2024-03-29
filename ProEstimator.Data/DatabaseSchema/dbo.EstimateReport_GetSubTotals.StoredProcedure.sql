USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EstimateReport_GetSubTotals]     
	@AdminInfoID Int,    
	@SupplementVersion TinyInt,    
	@EOR BIT = 1,    
	@PdrOnly			INT = 0    
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
			FROM dbo.GetEstimateSubTotalsFunction(@AdminInfoID, @SupplementVersion, @PdrOnly, 0)   
			WHERE FinalLineItemTotal > 0 OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')   
		END   
	ELSE   
		BEGIN   
   
			Create TABLE #ThisVersion      
			(    
				RateName				varchar(50),     
				FinalLineItemTotal		float,     
				Taxable					bit,     
				Rate					float,     
				Hours					float,     
				SortOrder				varchar(50),   
				Notes					varchar(300),   
				CapType					int,   
				Cap						real,
				DirectLineItemTotal				real,
				DiscountMarkupLineItemTotal		real   
			)    
			-- insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'GESTF') 
			INSERT INTO #ThisVersion   
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion, @PdrOnly, 0) AS ThisVersion   
			WHERE FinalLineItemTotal > 0 OR (FinalLineItemTotal < 0 AND SortOrder LIKE 'C%')-- AND SortOrder NOT LIKE '%Parts')   
   
			Create TABLE #LastVersion      
			(    
				RateName				varchar(50),     
				FinalLineItemTotal		float,     
				Taxable					bit,     
				Rate					float,     
				Hours					float,     
				SortOrder				varchar(50),   
				Notes					varchar(300),   
				CapType					int,   
				Cap						real,
				DirectLineItemTotal				real,
				DiscountMarkupLineItemTotal		real   
			)    
			--	insert into Ryan(AdmininfoID,Soure) values(@AdminInfoID,'GESTF') 
			INSERT INTO #LastVersion   
			SELECT * FROM [dbo].[GetEstimateSubTotalsFunction] (@AdminInfoID, @SupplementVersion - 1, @PdrOnly, 0) AS ThisVersion   
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
					, ISNULL(This.DirectLineItemTotal, Last.DirectLineItemTotal) AS DirectLineItemTotal
					, ISNULL(This.DiscountMarkupLineItemTotal, Last.DiscountMarkupLineItemTotal) AS DiscountMarkupLineItemTotal
				FROM #ThisVersion This   
				FULL OUTER JOIN #LastVersion Last ON Last.RateName = This.RateName   
			) AS Final   
			WHERE FinalLineItemTotal <> 0 OR Hours <> 0   
    OPTION(RECOMPILE) 
		END   
    
END    
GO
