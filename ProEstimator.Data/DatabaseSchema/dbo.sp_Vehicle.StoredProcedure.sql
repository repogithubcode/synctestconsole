USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE  PROCEDURE [dbo].[sp_Vehicle]  @vehicleID varchar(17)AS
 
select x.MakeID,x.ModelID,x.SubmodelID,x.EngineID as bodyid,x.BodyID as engineid
,x.TransmissionID,x.Service_Barcode,x.DriveID,x.VinYear
from vinn.dbo.Vehicle_Service_Xref as x with(nolock)
 
where x.VehicleID = @vehicleID

GO
