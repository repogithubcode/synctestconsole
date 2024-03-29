USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


create PROCEDURE [dbo].[GetBonusGrowth]
	@SalesRepID int = null,
	@month varchar(2) = NULL,
	@year varchar(4) = NULL,
	@renewPercent int = null,
	@salesGoal int = null,
	@salesForcast int = null
AS


/*
GetBonusGrowth 0,1,2012,70, 15, 15
getrenewals 25, '3/1/12', '3/31/12'
*/

SET NOCOUNT ON

--month/year/salesrep/renewpercent
declare @monthstart datetime
declare @monthend datetime
declare @s varchar(10)
declare @totalSales int
declare @totalForcastSales int
declare @renewalGoal int
declare @totalDue int
declare @renewalcount int
select @s = @month  + '/1/' + @year
select @monthstart = ThisMonthStart,@monthend =ThisMonthEnd
from fnDateswDate(@s)-- use month/year from app here
select @totalSales = SUM(numberSold)
					From SalesBoard where dateSold between @monthstart and @monthend
select @totalForcastSales = SUM(salesforcast) from GrowthBonus where month = @month and year = @year  and GrowthBonus.SalesRepID <> 0
select @salesGoal = Case when @SalesRepID <> 0 THEN @salesGoal ELSE (Select SUM(salesGoal) from GrowthBonus where SalesRepID <> 0 AND month = @month And year = @year) END 


Create Table #temp (LoginID varchar(50), ContatractID varchar(50), CompanyName varchar(50), FrameData varchar(50),  EMS varchar(50), contact varchar(50), salesRepID varchar(50), pricelevel varchar(50), renewaldate datetime, notes varchar(1000), renewalamount money,phone varchar(50), willrenew bit, hasRenewed bit)
insert into #temp
exec getRenewals @SalesRepID,@monthstart, @monthend

select @renewalGoal = (Select Cast(Sum(Round(cRen*renewalGoalPercent/100.00,0)) as Decimal(18,0))
						from(
						select Count(LoginID) as cRen,  GrowthBonus.SalesRepID
						from #temp
							Left join SalesRep
								on SalesRep.FirstName + ' ' + SalesRep.LastName = #temp.salesRepID
									left join GrowthBonus
										on GrowthBonus.SalesRepID = SalesRep.SalesRepID
						where GrowthBonus.month = @month and GrowthBonus.year = @year
						Group by GrowthBonus.SalesRepID) as a
							join GrowthBonus b
								on b.SalesRepID = a.SalesRepID
								
						where b.month = @month 
						and b.year = @year )
select @totalDue = (select COUNT(loginid) from #temp)
select @renewPercent = case when @SalesRepID <> 0 THEN @renewPercent ELSE Cast(Round(100.00*@renewalGoal/@totalDue,2) as Decimal(18,0)) END


select @renewalGoal = ISNULL(@renewalGoal,0),@totalDue = ISNULL(@totalDue,0), @renewPercent = ISNULL(@renewPercent,100)

select @renewalcount = ISNULL(COUNT(*),0) from #temp 

if(@renewalcount = 0 and @SalesRepID <> 0)
Begin
print'a' 
select @totalDue as Due
		,0 as RenewPercentGoal
		,0 as RenewPercentActual
		,0 as RenewPercentForcast 
		,0 as RenewForcast
		,0 as RenewActual
		,0 as RenewGoal
		, @salesGoal as SalesGoal
		 , case when @salesrepid <> 0 THEN @salesForcast ELSE @totalForcastSales END as SalesForcast
		, Case when @SalesRepID <> 0 THEN 
			 SUM(numberSold)  
		  ELSE 
			@totalSales
		  END as SalesActual
		 , Cast(Round(@salesGoal *100.00,2) as Decimal(18,2)) as GrowthPercentGoal
		 , Cast(Round((case when @salesrepid <>0 THEN  @salesForcast   ELSE @totalForcastSales END) *100.00,2) AS  Decimal(18,2))  AS GrowthPercentForcast
		 , Cast(Round((case when @salesrepid <>0 THEN  SUM(numberSold)   ELSE @totalSales END) *100.00,2) AS  Decimal(18,2))  AS GrowthPercentActual
		 ,100 as RenewPercent
		 ,case when @salesGoal > 0 then cast(round((CAST(@salesForcast as Decimal(18,2)) / CAST(@salesGoal as Decimal(18,2))*100),2) AS  Decimal(18,2))else 0 end  PercentOfGoalForcast
		 ,case when @salesGoal > 0 then  cast(round((CAST(SUM(numberSold) as Decimal(18,2)) / CAST(@salesGoal  as Decimal(18,2))*100),2) AS  Decimal(18,2)) else 0 end PercentOfGoalActual
		 	  
From SalesBoard
where dateSold between @monthstart and @monthend
AND salesRepID = @SalesRepID
end
else 
begin
print 'b'
--This is where the table actually starts
Select COUNT(LoginID) as Due
	 , @renewPercent as RenewPercentGoal
	 , Cast(Round(SUM(Case When hasRenewed = 1 THEN 1 ELSE 0 END)*100.00/COUNT(LoginID),0) as Decimal(18,0)) as RenewPercentActual
	 , Cast(Round(SUM(Case When willrenew = 1 THEN 1 ELSE 0 END)*100.00/COUNT(LoginID),0) as Decimal(18,0)) as RenewPercentForcast
	 , SUM(Case When willrenew = 1 THEN 1 ELSE 0 END) as RenewForcast
	 , SUM(Case When hasRenewed = 1 THEN 1 ELSE 0 END) RenewActual
	 , Case when @salesRepID <> 0 THEN Cast(Round(COUNT(LoginID)*@renewPercent/100.00,0) as Decimal(18,0)) ELSE @renewalGoal END as RenewGoal
	 , @salesGoal as SalesGoal
	 , case when @salesrepid <> 0 THEN @salesForcast ELSE @totalForcastSales END as SalesForcast
	 , Case when @SalesRepID <> 0 THEN 
			a.ActualSales 
	   ELSE 
			@totalSales
	   END as SalesActual
	 , Case When COUNT(LoginID) >0 THEN Cast(Round((case when @salesrepid <>0 THEN a.ActualSales ELSE @totalSales END + 1.0*SUM(Case When hasRenewed = 1 THEN 1 ELSE 0 END) - COUNT(LoginID))/COUNT(LoginID)*100.00,2) as Decimal(18,2)) ELSE 0 END AS GrowthPercentActual
	 , Case When COUNT(LoginID) >0 THEN Cast(Round((case when @salesrepid <> 0 THEN @salesForcast ELSE @totalForcastSales END + 1.0*SUM(Case When willrenew = 1 THEN 1 ELSE 0 END) - COUNT(LoginID))/COUNT(LoginID)*100.00,2) as Decimal(18,2)) ELSE 0 END as GrowthPercentForcast
	
	 , Case When COUNT(LoginID) >0 THEN Cast(Round((@salesGoal + 1.0*Round(COUNT(LoginID)*@renewPercent/100.00,0) - COUNT(LoginID))/COUNT(LoginID)*100.00,2) as Decimal(18,2)) ELSE 0 END as GrowthPercentGoal
    
     , Case When COUNT(LoginID) >0 THEN Cast(Round(SUM(Case When hasRenewed = 1 THEN 1 ELSE 0 END)/COUNT(LoginID), 0) as Decimal(18,2)) ELSE 0 END as RenewPercent
	
	 , Case When COUNT(LoginID) >0 THEN 
			CASE WHEN Case When COUNT(LoginID) >0 THEN Cast(Round((@salesGoal + 1.0*Round(COUNT(LoginID)*@renewPercent/100.00,0) - COUNT(LoginID))/COUNT(LoginID)*100.00,2) as Decimal(18,2)) ELSE 0 END <> 0 THEN
				Cast(Round(((case when @salesrepid <>0 THEN a.ActualSales ELSE @totalSales END + 1.0*SUM(Case When hasRenewed = 1 THEN 1 ELSE 0 END) - COUNT(LoginID))/COUNT(LoginID)*100.00)/((@salesGoal + 1.0*Round(COUNT(LoginID)*@renewPercent/100.00,0) - COUNT(LoginID))/COUNT(LoginID)*100.00)*100,2) as Decimal(18,0)) 
			ELSE 0
			END
	   ELSE Cast(Round(1.0*case when @salesrepid <>0 THEN a.ActualSales ELSE @totalSales END/@salesGoal,0) AS Decimal(18,0))
	   END as PercentOfGoalActual
	 , Case When COUNT(LoginID) >0 THEN 
			CASE WHEN Case When COUNT(LoginID) >0 THEN Cast(Round((@salesGoal + 1.0*Round(COUNT(LoginID)*@renewPercent/100.00,0) - COUNT(LoginID))/COUNT(LoginID)*100.00,2) as Decimal(18,2)) ELSE 0 END <> 0 THEN
			Cast(Round(((case when @salesrepid <> 0 THEN @salesForcast ELSE @totalForcastSales END + 1.0*SUM(Case When willrenew = 1 THEN 1 ELSE 0 END) - COUNT(LoginID))/COUNT(LoginID)*100.00)/((@salesGoal + 1.0*Round(COUNT(LoginID)*@renewPercent/100.00,0) - COUNT(LoginID))/COUNT(LoginID)*100.00)*100,2) as Decimal(18,0)) 
			Else 0
			END
	   ELSE Cast(Round(1.0*case when @salesrepid <> 0 THEN @salesForcast ELSE @totalForcastSales END/@salesGoal,0) AS Decimal(18,0))
	   END as PercentOfGoalForcast
from 
#temp
Left join
(
Select SUM(numberSold) as ActualSales
From SalesBoard
where dateSold between @monthstart and @monthend
AND salesRepID = @SalesRepID
) as a


	on 1=1
group by a.ActualSales
end

drop table #temp


GO
