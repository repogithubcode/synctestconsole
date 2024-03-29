USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE  PROCEDURE [dbo].[sp_GetVinn]  @Vin varchar(17)AS

declare @Year int
declare @MakeID int
declare @Make varchar(50)
declare @ModelId int
declare @Model varchar(50)
declare @VehicleTypeID int
declare @VehicleType varchar(10)
declare @SubmodelID int
declare @Submodel varchar(50)
declare @BodyID int
declare @Body varchar(50)
declare @EngineID int
declare @Engine varchar(50)
declare @TransmissionID int
declare @Transmission varchar(50)
declare @DriveId int
declare @Drive varchar(50)
declare @Service_Barcode int
declare @VehicleID int
 select Cast(x.VinYear as varchar(10)) + ' ' + m.make + ' ' + mo.Model + ' ' + s.Submodel + ' ' +  b.Body + ' ' + d.Drive + ' '  as Display,x.Service_Barcode,x.VehicleID 

--select x.VinYear as 'Year',m.make,m.MakeID,mo.Model,vt.VehicleType,@Vin,s.Submodel,s.SubmodelID
--,b.Body,b.BodyID,e.Engine, e.EngineID,tr.Transmission,tr.TransmissionsID,d.Drive,d.DriveID
--,x.Service_Barcode,x.VehicleID,'' as paintcode
 
from vinn.dbo.Vehicle_Service_Xref as x with(nolock)
	inner join vinn.dbo.VinToYear  as y with(nolock)
		on x.VinYear = y.VinYear
		and y.VinTenthCharacter = substring(@Vin, 10, 1)
		AND ( @Vin like y.VinExpression + '%' )
	inner join (vinn.dbo.VinToMake as vm with(nolock) 
					inner join  vinn.dbo.Makes as m with(nolock)
						on vm.MakeID = m.MakeID) 
		on vm.VinSecondCharacter =  substring(@Vin, 2, 1)
		and ( @Vin like vm.VinExpression + '%' )
		and vm.MakeID = x.MakeID
	inner join (vinn.dbo.VinToModel as vmo with(nolock)
					inner join vinn.dbo.Models as mo with(nolock) 
						on vmo.ModelID = mo.ModelID )
		on x.ModelID = vmo.ModelID
		and x.MakeID = vmo.MakeID
		and ( @Vin like vmo.VinExpression + '%' 
            OR vmo.VinExpression IS NULL)
    left join (vinn.dbo.VinToVehicleType as vvt with(nolock)
				inner join vinn.dbo.VehicleTypes as vt with(nolock)
					on vvt.VehicleTypeID = vt.VehicleTypeID)
		on x.MakeID = vvt.MakeID 
		and x.ModelID = vvt.ModelID
	inner join (vinn.dbo.VinToSubmodel vs with(nolock) 
					inner join vinn.dbo.submodels s with(nolock) 
						on vs.submodelid = s.submodelid)
		on x.SubmodelID = vs.SubmodelID 
		and x.MakeID = vs.MakeID 
		and x.ModelID = vs.ModelID
		and ( @Vin like vs.VinExpression + '%' 
            OR vs.VinExpression IS NULL)
	inner join ( vinn.dbo.VinToBody as vb with(nolock)
					inner join vinn.dbo.Bodys  as b with(nolock)
						on vb.BodyID = b.BodyID
						and (@Vin like vb.VinExpression + '%' 
								OR vb.VinExpression IS NULL))
		on x.EngineID = vb.BodyID  -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
		and x.MakeID = vb.MakeID
		and x.ModelID = vb.ModelID
	 inner join  (vinn.dbo.VinToEngine as ve  with(nolock)
					inner join vinn.dbo.Engines as e with(nolock)
						on ve.EngineID = e.EngineID
						and ( @Vin like ve.VinExpression + '%' 
								 OR ve.VinExpression IS NULL))
		on x.BodyID = ve.EngineID  -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
		and x.MakeID = ve.MakeID
		and x.ModelID = ve.ModelID
	inner join (vinn.dbo.VinToTransmission as vtr with(nolock)
				  inner join vinn.dbo.Transmissions as tr with(nolock)
						on vtr.TransmissionID = tr.TransmissionsID
						and ( @Vin like vtr.VinExpression + '%' 
							OR vtr.VinExpression IS NULL))
		on x.TransmissionID = vtr.TransmissionID 
		and x.MakeID = vtr.MakeID 
		and x.ModelID = vtr.ModelID
	 inner join (vinn.dbo.VinToDrive as vd with(nolock)
				  inner join vinn.dbo.Drives as d with(nolock)
						on vd.DriveID = d.DriveID
						and ( @Vin like vd.VinExpression + '%' 
							OR vd.VinExpression IS NULL))
		on x.DriveID = vd.DriveID 
		and x.MakeID = vd.MakeID 
		and x.ModelID = vd.ModelID

GO
