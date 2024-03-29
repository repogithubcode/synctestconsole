USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[sp_getVehicledetails] 
	@AdminInfoId int 
AS 
BEGIN 
  
	SET NOCOUNT ON; 
  
	SELECT  
	   VehicleInfo.id 
      ,[EstimationDataId] 
      ,VehicleInfo.[VehicleID] 
      ,[AdditionalOptions] 
      ,[Color] 
      ,[ColorCode] 
      ,[MilesIn] 
      ,[MilesOut] 
      ,[License] 
      ,[State] 
      ,[Condition] 
      ,[ExtColor]
	  ,[ExtColor2]
      ,[ExtColorCode] 
      ,[IntColor] 
      ,[IntColorCode] 
      ,[TrimLevel] 
      ,TRIM([Vin]) AS Vin 
      ,[VinDecode] 
      ,[InspectionDate] 
      ,[ExtColorCodeChar] 
      ,[IntColorCodeChar] 
      ,[ProductionDate] 
      ,[BodyType] 
      ,VehicleInfo.[Service_Barcode] 
      ,[DefaultPaintType] 
      ,[DriveType] 
      ,[VehicleValue] 
      ,[Year] 
      ,VehicleInfo.[MakeID] 
      ,VehicleInfo.[ModelID] 
      ,VehicleInfo.[SubModelID] 
      ,[EngineType] 
      ,[TransmissionType] 
      ,[paintcode] 
	  ,[StockNumber] 
	  ,[POILabel1] 
	  ,[POIOption1] 
	  ,[POICustom1] 
	  ,[POILabel2] 
	  ,[POIOption2] 
	  ,[POICustom2]
	  ,[PaintCode2]
	  , 1 AS 'Data' -- Case when VinYear between from_year and to_year then 1 Else 0 end as 'Data'  
	FROM EstimationData with(nolock) 
	LEFT OUTER JOIN VehicleInfo with(nolock) ON EstimationData.ID = VehicleInfo.EstimationDataID
	--LEFT OUTER JOIN vinn.dbo.vehicle_service_xref AS Xref ON VehicleInfo.VehicleID = Xref.VehicleID 
	--LEFT OUTER JOIN Mitchell3.dbo.Service ON Xref.Service_Barcode = Service.Service_Barcode 
	WHERE EstimationData.AdminInfoID = @AdminInfoId 

	--FROM VehicleInfo with(nolock) 
	--INNER JOIN EstimationData  with(nolock) ON EstimationData.id = VehicleInfo.EstimationDataId 
	--LEFT OUTER JOIN vinn.dbo.vehicle_service_xref AS Xref ON VehicleInfo.VehicleID = Xref.VehicleID 
	--LEFT OUTER JOIN Mitchell3.dbo.Service ON Xref.Service_Barcode = Service.Service_Barcode 
	--WHERE EstimationData.AdminInfoID = @AdminInfoId 
  
 
END 
GO
