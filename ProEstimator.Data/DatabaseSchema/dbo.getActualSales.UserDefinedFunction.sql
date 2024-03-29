USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE FUNCTION [dbo].[getActualSales] 
(
	@SalesRepID int,
	@Month int,
	@year int
)
RETURNS int
AS
BEGIN

Declare @Count int
	 

select @Count=  sum(numbersold)
from (
select loginid,numberSold
from SalesBoard
where numberSold > 0 
and month(datesold) = @Month 
and year(datesold) = @year
and salesRepID = case when @SalesRepID > 0 then @SalesRepID else salesRepID end
union all
select loginid,numberSold
from  [WEB-EST-PROE\WEBESTARCHIVE].focuswrite.dbo.SalesBoard
where numberSold > 0 
and month(datesold) = @Month 
and year(datesold) = @year
and salesRepID = case when @SalesRepID > 0 then @SalesRepID else salesRepID end
) as a



	
	RETURN @Count

END
GO
