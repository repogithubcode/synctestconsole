USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



create procedure [dbo].[GetEstimatorname]
@Admininfo int
as
begin
select AuthorFirstName+' ' +AuthorLastName  from estimatorsdata
where AdminInfoID=@Admininfo 
end




GO
