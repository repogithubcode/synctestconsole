USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[GetEstimatorsList] 
@LoginID int
as
begin
--GetEstimatorsList 55095
Select EstimatorID,AuthorFirstName+' '+ AuthorLastName+' '+ OrderNo as 'Estimators'  from EstimatorsData
where LoginID =@LoginID 
order by OrderNo
end

GO
