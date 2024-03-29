USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE  PROCEDURE [dbo].[sp_getVehicleID]
	@Year int
	,@MakeID int
	,@ModelID int
	,@TrimLevelID int
AS
BEGIN
	declare @count int 
	select @count = 0
	Create table #Temp   (VehicleID int,Service_barcode int,VehicleName varchar(100))
	
	insert into #Temp
	select distinct top 1 a.VehicleID,a.Service_Barcode,b.Make + ' ' + c.Model + ' ' + d.Submodel as 'VehicleName'
	from vinn.dbo.vehicle_service_xref a
	inner join vinn.dbo.makes b
	on b.makeid = a.makeid
	inner join vinn.dbo.models c
	on c.modelid = a.modelid
	inner join vinn.dbo.submodels d
	on d.submodelid = a.submodelid and a.SubmodelID = @TrimLevelID
	 
	where VinYear = @Year
	and a.makeid = @MakeID
	and c.modelid = @ModelID
	
	 
	 
	 
 
	 
	
	select @count = COUNT(*)
	from #Temp
	
	 
	
	if @count = 0
	
	
	begin  print @count
		insert into #Temp
		select distinct top 1 ms.VehicleID,a.Service_Barcode,ms.VehicleName  
		from vinn.dbo.vehicle_service_xref a
		inner join vinn.dbo.makes b
		on b.makeid = a.makeid
		inner join vinn.dbo.models c
		on c.modelid = a.modelid
		inner join vinn.dbo.submodels d
		on d.submodelid = a.submodelid and a.SubmodelID = @TrimLevelID
		inner join Mitchell3.dbo.ServicesToVehicleID as ms
			on ms.BarCode = a.Service_Barcode
		where VinYear = @Year
		and a.makeid = @MakeID
		and c.modelid = @ModelID
		--and ms.VehicleName like '%' + d.Submodel + '%'
		and ms.VehicleName like '%' + CAST(@Year as varchar(4))+ '%' 
		order by  ms.VehicleID,ms.VehicleName
	end
	
	select * from #Temp
 
END


GO
