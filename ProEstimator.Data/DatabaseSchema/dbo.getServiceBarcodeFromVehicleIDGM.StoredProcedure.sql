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

--exec getVehicleID 2010,9,108,180
CREATE PROCEDURE [dbo].[getServiceBarcodeFromVehicleIDGM]
	@VehicleID int
AS
BEGIN
	declare @count int 
	select @count = 0
	Create table #temp   (VehicleID int,VehicleName varchar(100), ServiceBarcode varchar(50), LastUpdated datetime, Year int, MakeID int)
	
	insert into #temp
	select distinct top 1 a.VehicleID,b.Make + ' ' + c.Model + ' ' + d.Submodel as 'VehicleName', a.Service_Barcode, a.LastUpdated, a.VinYear, a.MakeID
	from vinn.dbo.vehicle_service_xref a
	inner join vinn.dbo.makes b on b.makeid = a.makeid
	inner join vinn.dbo.models c on c.modelid = a.modelid
	inner join vinn.dbo.submodels d on d.submodelid = a.submodelid 
	where a.VehicleID = @VehicleID
	 
	select @count = COUNT(*)
	from #temp
	
	if @count = 0
	begin  print @count
		insert into #temp
		select distinct top 1 ms.VehicleID,ms.VehicleName, a.Service_Barcode , a.LastUpdated, A.VinYear, a.MakeID
		from vinn.dbo.vehicle_service_xref a
		inner join vinn.dbo.makes b on b.makeid = a.makeid
		inner join vinn.dbo.models c on c.modelid = a.modelid
		inner join vinn.dbo.submodels d on d.submodelid = a.submodelid 
		inner join Mitchell3.dbo.ServicesToVehicleID as ms on ms.BarCode = a.Service_Barcode
		where A.VehicleID = @VehicleID
		order by  ms.VehicleID,ms.VehicleName
	end

	Declare @LastUpdate datetime
	Declare @ServiceBarcode varchar(50)
	declare @MakeID int
	select @LastUpdate = LastUpdated, @ServiceBarcode = ServiceBarcode, @MakeID = MakeID from #temp

	--Update Last Updated date for gm service barcodes if older than 1 day (web code is immediately updating these)
	if @MakeID in (6,7,9,15,16,26,29,33,36,41,94) AND ((@LastUpdate is not null and @LastUpdate < DateAdd(day,-1,GETDATE())) OR @LastUpdate is null)
	Begin
		update vinn.dbo.Vehicle_Service_Xref
		set LastUpdated = getdate()
		where Service_Barcode = @ServiceBarcode
	End
	
	select * from #temp
 
END

GO
