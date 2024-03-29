USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[getSalesBoardAll]
	-- Add the parameters for the stored procedure here
	 @date datetime = null
AS
BEGIN
	 
	SET NOCOUNT ON;

	if @date is null
		begin
		 select @date = GETDATE()
		end

	select 
		  FirstName + ' ' + LastName as Name
		, l.Organization  as Company
		, 0 AS ProE
		, sb.salesBoardID
		, sb.salesRepID
		, sb.loginID
		, sb.numberSold
		, sb.frame
		, sb.ems
		, sb.dateSold
		, sb.AddUser 
		, 0 as HasQBExporter
		, 0 as ProAdvisor
		, 0 as bundle
		, 0 as imageEditor
		, 0 as enterpriseReporting
	from [WEB-EST-PROE\WEBESTARCHIVE].focuswrite.[dbo].salesboard as sb
	inner join [WEB-EST-PROE\WEBESTARCHIVE].focuswrite.[dbo].SalesRep as s on sb.salesRepID = s.SalesRepID
	inner join [WEB-EST-PROE\WEBESTARCHIVE].focuswrite.[dbo].Logins as l on l.id = sb.loginid
	where 
		MONTH(datesold) = MONTH(@date)
		and YEAR(datesold) = YEAR(@date)

	UNION

	SELECT 
		  (FirstName + ' ' + LastName) as Name 
		, Logins.Organization as Company
		, '1' as ProE
		, SalesBoard.* 
	FROM SalesBoard
	JOIN SalesRep ON SalesBoard.salesRepID = SalesRep.SalesRepID 
	LEFT OUTER JOIN Logins on Logins.id = SalesBoard.loginID 
	WHERE MONTH(datesold) = MONTH(@date) 
		and YEAR(datesold) = YEAR(@date) 
	order by ProE, Name
END
GO
