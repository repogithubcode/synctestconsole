USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_GetBodyTypes_Old]
	 @VehicleID varchar(30),
	 @AdminInfoID int,
	 @BodyTypeRequired Bit = 0 OUTPUT
AS

Begin
	select '0' as 'BodyStyleConfigID'
		,'' as 'BodyType'  
		,@BodyTypeRequired as 'BodyTypeRequired' 
		union all
		select distinct  b.BodyID as 'BodyStyleConfigID'
		, b.Body as 'BodyType'  
		,@BodyTypeRequired 'BodyTypeRequired' 
		--,(select top 1 v.BodyType from FocusWrite.dbo.VehicleInfo as v inner join FocusWrite.dbo.EstimationData as e on e.id = v.EstimationDataId where e.AdminInfoID = @AdminInforID) as selected
		from vinn.dbo.Bodys as b
		inner join vinn.dbo.VinToBody as a
			on a.bodyid = b.bodyid
		inner join  vinn.dbo.Vehicle_Service_Xref c
					on a.MakeID        = c.MakeID 
					and a.ModelID       = c.ModelID 
	where  c.VehicleID = @VehicleID

end

GO
