USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



create procedure [dbo].[UpdateEstimatorsInfo] --2335262,2
@AdminInfoID int,
@EstimatorID int
as
begin

if (select count (AuthorFirstName)  from EstimatorsData where AdminInfoID =@AdminInfoID  ) > 0
begin
update EstimatorsData set AdminInfoID =0
where AdminInfoID =@AdminInfoId
end
update EstimatorsData set AdminInfoID=@AdminInfoID where EstimatorID=@EstimatorID 
end







GO
