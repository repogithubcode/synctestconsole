USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO





CREATE Procedure [dbo].[isNewSale]
	@LoginID Int
AS

/*
isNewSale is run after a contract is signed. It returns whether or not this was the first Estimatic/Frame/EMS sold. 
*/
select case when count(distinct(ContractIDEstimatic)) = 1 AND (Select EffectiveDate from Contract where Contract.ContractID = estimatic.ContractIDEstimatic) between dbo.GetDateOnly(getdate()) - day(10) AND  dbo.GetDateOnly(GetDate())THEN 1 ELSE 0 END as isNewEstimatic
,case when count(distinct(ContractIDFrame)) = 1 AND (Select EffectiveDate from Contract where Contract.ContractID = Frame.ContractIDFrame) = dbo.GetDateOnly(GetDate())THEN 1 ELSE 0 END as isNewFrame
,case when count(distinct(ContractIDEMS)) = 1 AND (Select EffectiveDate from Contract where Contract.ContractID = EMS.ContractIDEMS) = dbo.GetDateOnly(GetDate())THEN 1 ELSE 0 END as isNewEMS
,case when count(distinct(ContractIDMU)) = 1 AND (Select EffectiveDate from Contract where Contract.ContractID = MU.ContractIDMU) = dbo.GetDateOnly(GetDate())THEN 1 ELSE 0 END as isNewMU
, estimatic.SalesRepID
from(
select distinct(Contract.ContractID) as ContractIDEstimatic, Contract.contractTypeID as Estimatic, Logins.SalesRepID,Contract.EffectiveDate
from Contract
	join Invoice
		on Contract.ContractID = Invoice.ContractID
			join Logins
				on Invoice.LoginID = Logins.id
					join ContractPriceLevels
						on ContractPriceLevels.ContractPriceLevelID = Contract.ContractPriceLevelID
							join ContractTerms
								on ContractTerms.ContractTermID = ContractPriceLevels.ContractTermID						
where Logins.id = @LoginID
and ContractLength >= 365
and Contract.ContractTypeID = 1
) as estimatic
	left join
(
select distinct(Contract.ContractID) as ContractIDFrame, Contract.contractTypeID as Frame, Logins.SalesRepID, Contract.EffectiveDate
from Contract
	join Invoice
		on Contract.ContractID = Invoice.ContractID
			join Logins
				on Invoice.LoginID = Logins.id
					join ContractPriceLevels
						on ContractPriceLevels.ContractPriceLevelID = Contract.ContractPriceLevelID
							join ContractTerms
								on ContractTerms.ContractTermID = ContractPriceLevels.ContractTermID
						
where Logins.id = @LoginID
and ContractLength >= 365
and Contract.ContractTypeID = 2
) as Frame 
on 1=1
	left join
(
select distinct(Contract.ContractID) as ContractIDEMS, Contract.contractTypeID as EMS, Logins.SalesRepID, Contract.EffectiveDate
from Contract
	join Invoice
		on Contract.ContractID = Invoice.ContractID
			join Logins
				on Invoice.LoginID = Logins.id
					join ContractPriceLevels
						on ContractPriceLevels.ContractPriceLevelID = Contract.ContractPriceLevelID
							join ContractTerms
								on ContractTerms.ContractTermID = ContractPriceLevels.ContractTermID
						
where Logins.id = @LoginID
and ContractLength >= 365
and Contract.ContractTypeID = 5
) as EMS
on 1=1
	left join
(
select distinct(Contract.ContractID) as ContractIDMU, Contract.contractTypeID as EMS, Logins.SalesRepID, Contract.EffectiveDate
from Contract
	join Invoice
		on Contract.ContractID = Invoice.ContractID
			join Logins
				on Invoice.LoginID = Logins.id
					join ContractPriceLevels
						on ContractPriceLevels.ContractPriceLevelID = Contract.ContractPriceLevelID
							join ContractTerms
								on ContractTerms.ContractTermID = ContractPriceLevels.ContractTermID
						
where Logins.id = @LoginID
and ContractLength >= 365
and Contract.ContractTypeID = 8
) as MU
on 1=1
group by estimatic.ContractIDEstimatic, Frame.ContractIDFrame, EMS.ContractIDEMS, MU.ContractIDMU, estimatic.SalesRepID





GO
