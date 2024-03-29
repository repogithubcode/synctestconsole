USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE FUNCTION [dbo].[fnDates]()

/*

SELECT * FROM dbo.fnDates()

*/

RETURNS @Dates table (
	Today datetime,
	ThisWeekStart datetime,
	ThisMonthStart datetime,
	ThisQuarterStart datetime,
	ThisYearStart datetime,
	TodayEnd datetime,
	ThisWeekEnd datetime,
	ThisMonthEnd datetime,
	ThisMonthLastWeekday datetime,
	ThisQuarterEnd datetime,
	ThisYearEnd datetime,
	Tomorrow datetime,
	NextWeekStart datetime,
	NextMonthStart datetime,
	NextQuarterStart datetime,
	NextYearStart datetime,
	TomorrowEnd datetime,
	NextWeekEnd datetime,
	NextMonthEnd datetime,
	NextQuarterEnd datetime,
	NextYearEnd datetime,
	YesterdayStart datetime,
	LastWeekStart datetime,
	LastMonthStart datetime,
	LastQuarterStart datetime,
	LastYearStart datetime,
	YesterdayEnd datetime,
	LastWeekEnd datetime,
	LastMonthEnd datetime,
	LastQuarterEnd datetime,
	LastYearEnd datetime
)
AS
BEGIN

-- Set current date
DECLARE @Today datetime
SELECT @Today = Convert(datetime, Convert(varchar(10), GetDate, 101)) FROM vwGetDate


INSERT INTO @Dates
	SELECT	dateAdd(dd, dateDiff(dd, 0, @Today), 0) AS Today,
			dateAdd(wk, dateDiff(wk, 0, @Today), 0) AS ThisWeekStart,
			dateAdd(mm, dateDiff(mm, 0, @Today), 0) AS ThisMonthStart,
			dateAdd(qq, dateDiff(qq, 0, @Today), 0) AS ThisQuarterStart,
			dateAdd(yy, dateDiff(yy, 0, @Today), 0) AS ThisYearStart,
			dateAdd(ms, -3, dateAdd(dd, dateDiff(dd, 0, @Today) + 1, 0)) AS TodayEnd,
			dateAdd(ms, -3, dateAdd(wk, dateDiff(wk, 0, @Today) + 1, 0)) AS ThisWeekEnd,
			dateAdd(ms, -3, dateAdd(mm, dateDiff(mm, 0, @Today) + 1, 0)) AS ThisMonthEnd,
			NULL AS ThisMonthLastWeekday,
			dateAdd(ms, -3, dateAdd(qq, dateDiff(qq, 0, @Today) + 1, 0)) AS ThisQuarterEnd,
			dateAdd(ms, -3, dateAdd(yy, dateDiff(yy, 0, @Today) + 1, 0)) AS ThisYearEnd,
			dateAdd(dd, dateDiff(dd, 0, @Today) + 1, 0) AS Tomorrow,
			dateAdd(wk, dateDiff(wk, 0, @Today) + 1, 0) AS NextWeekStart,
			dateAdd(mm, dateDiff(mm, 0, @Today) + 1, 0) AS NextMonthStart,
			dateAdd(qq, dateDiff(qq, 0, @Today) + 1, 0) AS NextQuarterStart,
			dateAdd(yy, dateDiff(yy, 0, @Today) + 1, 0) AS NextYearStart,
			dateAdd(ms, -3, dateAdd(dd, dateDiff(dd, 0, @Today) + 2, 0)) AS TomorrowEnd,
			dateAdd(ms, -3, dateAdd(wk, dateDiff(wk, 0, @Today) + 2, 0)) AS NextWeekEnd,
			dateAdd(ms, -3, dateAdd(mm, dateDiff(mm, 0, @Today) + 2, 0)) AS NextMonthEnd,
			dateAdd(ms, -3, dateAdd(qq, dateDiff(qq, 0, @Today) + 2, 0)) AS NextQuarterEnd,
			dateAdd(ms, -3, dateAdd(yy, dateDiff(yy, 0, @Today) + 2, 0)) AS NextYearEnd,
			dateAdd(dd, -1, dateAdd(dd, dateDiff(dd, 0, @Today), 0)) AS YesterdayStart,
			dateAdd(wk, -1,dateAdd(wk, dateDiff(wk, 0, @Today), 0)) AS LastWeekStart,
			dateAdd(mm, -1,dateAdd(mm, dateDiff(mm, 0, @Today), 0)) AS LastMonthStart,
			dateAdd(qq, -1,dateAdd(qq, dateDiff(qq, 0, @Today), 0)) AS LastQuarterStart,
			dateAdd(yy, -1,dateAdd(yy, dateDiff(yy, 0, @Today), 0)) AS LastYearStart,
			dateAdd(ms, -3, dateAdd(dd, dateDiff(dd, 0, @Today), 0)) AS YesterdayEnd,
			dateAdd(ms, -3,dateAdd(wk, dateDiff(wk, 0, @Today), 0)) AS LastWeekEnd,
			dateAdd(ms, -3,dateAdd(mm, dateDiff(mm, 0, @Today), 0)) AS LastMonthEnd,
			dateAdd(ms, -3,dateAdd(qq, dateDiff(qq, 0, @Today), 0)) AS LastQuarterEnd,
			dateAdd(ms, -3,dateAdd(yy, dateDiff(yy, 0, @Today), 0)) AS LastYearEnd


-- Set last weekday of month
UPDATE		@Dates
SET			ThisMonthLastWeekday =
				Convert(varchar(10), ( CASE
					WHEN datepart(dw, ThisMonthEnd) = 7 THEN dateadd(d, -1, ThisMonthEnd)
					WHEN datepart(dw, ThisMonthEnd) = 1 THEN dateadd(d, -2, ThisMonthEnd)
					ELSE ThisMonthEnd
				END ), 101)

RETURN 

END

GO
