USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetEstimateReportTotalsOnly]
	@AdminInfoID Int,
	@SupplementVersion Bit = NULL
AS
SET NOCOUNT ON

DECLARE @Total Money
DECLARE @TotalBetterment Money

CREATE TABLE #Totals (
		LineDesc VarChar(100),
		OtherInfo VarChar(100),
		Total Money,
		AdminInfoID Int,
		Taxable Bit)
		
--Get Total
	--INSERT INTO #Totals
		EXECUTE GetEstimateReportTotals
			@AdminInfoID = @AdminInfoID,
			@Betterment = 0,
			@SupplementVersion = @SupplementVersion,
			@TotalsOnly = 1

	SELECT @Total = Total
	FROM #Totals
	WHERE LineDesc = 'Total'

TRUNCATE TABLE #Totals

--Get Betterment Total
	--INSERT INTO #Totals
		EXECUTE GetEstimateReportTotals
			@AdminInfoID = @AdminInfoID,
			@Betterment = 1,
			@SupplementVersion = @SupplementVersion,
			@TotalsOnly = 1

	SELECT @TotalBetterment = Total
	FROM #Totals
	WHERE LineDesc = 'Total'

SELECT 	dbo.FormatMoney(ISNULL(Round(@Total,2),0) + ISNULL(Round(@TotalBetterment,2),0)) 'Total',
	dbo.FormatMoney(ISNULL(@TotalBetterment,0)) 'BettermentTotal'

DROP TABLE #Totals


GO
