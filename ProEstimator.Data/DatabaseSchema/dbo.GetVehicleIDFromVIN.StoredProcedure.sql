USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



--select vehicleid from (exec getvinn '2GCGC29F1P1035637')
Create  PROCEDURE [dbo].[GetVehicleIDFromVIN]  @Vin varchar(17)AS

--select *
--from vinn.dbo.Vehicle_Service_Xref
--where MakeID = 9
--and ModelID = 341
--and SubmodelID = 1
--and vinyear = 1993

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

 
SELECT @Year = VinYear 
   FROM vinn.dbo.VinToYear 
   WHERE
        VinTenthCharacter = substring(@Vin, 10, 1)
      AND ( @Vin like VinExpression + '%' )


SELECT DISTINCT @MakeID = a. MakeID, @Make= b.Make  
   FROM vinn.dbo.VinToMake a, 
            vinn.dbo.Makes b 
WHERE
         VinSecondCharacter = substring(@Vin, 2, 1)
   AND ( @Vin like a.VinExpression + '%' )
   AND b.MakeID = a.MakeID
   
  

 SELECT distinct  @ModelId=  a.ModelID,@Model = b.Model
FROM vinn.dbo.VinToModel a 
		inner join vinn.dbo.Models b 
			on a.ModelID = b.ModelID 
		inner join vinn.dbo.Vehicle_Service_Xref as x
			on x.MakeID = a.MakeID
			and x.VinYear  = @Year
			and a.ModelID = x.modelid
WHERE a.MakeID = @MakeID
      and ( @Vin like VinExpression + '%' 
            OR VinExpression IS NULL)
 


SELECT DISTINCT @VehicleTypeID = a.VehicleTypeID,
                @VehicleType = b.VehicleType
FROM vinn.dbo.VinToVehicleType a, 
         vinn.dbo.VehicleTypes b 
WHERE a.MakeID        = @MakeID
  and a.ModelID       = @ModelId
  and a.VehicleTypeID = b.VehicleTypeID 



SELECT DISTINCT  @SubmodelID = a.SubmodelID,@Submodel = b.Submodel 
FROM    vinn.dbo.VinToSubmodel a
			inner join vinn.dbo.submodels b
				on b.submodelid = a.submodelid 
			inner join  vinn.dbo.Vehicle_Service_Xref c
				on a.MakeID        = c.MakeID 
			    and a.ModelID       = c.ModelID 
			    and a.SubmodelID    = c.SubmodelID 
WHERE a.MakeID      = @MakeID
      and a.ModelID = @ModelID
      and ( @Vin like VinExpression + '%' 
            OR VinExpression IS NULL)
      and c.VinYear          = @Year
      and c.VehicleTypeID = @VehicleTypeID
      and a.MakeID        = c.MakeID 
      and a.ModelID       = c.ModelID 
      and a.SubmodelID    = c.SubmodelID 
      and a.SubmodelID    = b.SubmodelID

   

SELECT DISTINCT @BodyID = b.BodyID,@Body = b.Body
FROM       vinn.dbo. VinToBody a, 
           vinn.dbo.Bodys b,
		   vinn.dbo.Vehicle_Service_Xref c
WHERE a.MakeID      = @MakeID
      and a.ModelID = @ModelID
       and (@Vin like VinExpression + '%' 
             OR VinExpression IS NULL)
      and c.VinYear       = @Year
      and c.VehicleTypeID = @VehicleTypeID
      and c.SubmodelID    = @SubmodelID
      and a.MakeID        = c.MakeID 
      and a.ModelID       = c.ModelID 
      and a.BodyID        = c.EngineID -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and a.BodyID        = b.BodyID        

	 


SELECT DISTINCT @EngineID = a.EngineID, @Engine = b.Engine 
FROM      vinn.dbo.VinToEngine a, 
          vinn.dbo.Engines b,
		  vinn.dbo.Vehicle_Service_Xref c
WHERE a.MakeID      = @MakeID
      and a.ModelID = @ModelID
      and ( @Vin like VinExpression + '%' 
            OR VinExpression IS NULL)
      and c.VinYear       = @Year
      and c.VehicleTypeID = @VehicleTypeID
      and c.SubmodelID    = @SubmodelID       
      and c.engineid        = @BodyID -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and a.MakeID        = c.MakeID 
      and a.ModelID       = c.ModelID 
      and a.EngineID      = c.BodyID -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and a.EngineID      = b.EngineID 



 
      
 SELECT DISTINCT @TransmissionID = a.TransmissionID, @Transmission = b.Transmission 
FROM vinn.dbo.VinToTransmission a, 
     vinn.dbo.Transmissions b,
	 vinn.dbo.Vehicle_Service_Xref c
WHERE a.MakeID      = @MakeID
      and a.ModelID = @ModelID
      and ( @Vin like VinExpression + '%' 
            OR VinExpression IS NULL)
      and c.VinYear        = @Year
      and c.VehicleTypeID  = @VehicleTypeID
      and c.SubmodelID     = @SubmodelID       
      and c.EngineID       = @BodyID   -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and c.BodyID         = @EngineID  -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and a.MakeID         = c.MakeID 
      and a.ModelID        = c.ModelID 
      and a.TransmissionID = c.TransmissionID 
      and a.TransmissionID = b.TransmissionsID 
 
      
      
     
  SELECT DISTINCT @DriveID = a.DriveID, @Drive = b.Drive 
FROM  vinn.dbo.VinToDrive a, 
      vinn.dbo.Drives b,
	  vinn.dbo.Vehicle_Service_Xref c
WHERE a.MakeID      = @MakeID
      and a.ModelID = @ModelID
      and ( @Vin like VinExpression + '%' 
           OR VinExpression IS NULL)
      and c.VinYear        = @Year
      and c.VehicleTypeID  = @VehicleTypeID
      and c.SubmodelID     = @SubmodelID       
      and c.EngineID       = @BodyID   -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and c.BodyID         = @EngineID  -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
      and c.TransmissionID = @TransmissionID 
      and a.MakeID         = c.MakeID 
      and a.ModelID        = c.ModelID 
      and a.DriveID       = c. DriveID
      and a.DriveID        = b.DriveID 
    
      
-- SELECT top 1 @Service_Barcode = Service_Barcode 
--		,@VehicleID = VehicleID
--from    vinn.dbo.Vehicle_Service_Xref 
--WHERE  VinYear        = @Year 
--  AND  MakeID         = @MakeID 
--  AND  ModelID        = @ModelID
--  AND  SubmodelID     = @SubmodelID
--   AND  EngineID       = @BodyID  -- engineID and BodyID are backwards in Vehicle_Service_Xref
--  -- and  bodyid		  = @EngineID  -- engineID and BodyID are backwards in Vehicle_Service_Xref
--  and TransmissionID  = @TransmissionID
--  and DriveID		  = @DriveId
  
 --select @VehicleTypeID,@year,@makeid,@modelid,@submodelid,@bodyid as 'b',@Engineid as 'e',@TransmissionID as 't',@DriveId as 'd'
 
  
SELECT @Service_Barcode = Service_Barcode 
	   ,@VehicleID = vehicleid
FROM vinn.dbo.Vehicle_Service_Xref
WHERE VinYear        = @Year 
  AND MakeID         = @MakeID 
  AND ModelID        = @ModelID
  AND SubmodelID     = @SubmodelID
  and EngineID       = case when  @BodyID is null then EngineID else @BodyID end     -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
  and BodyID         = case when  @EngineID is null then BodyID else @EngineID end -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
  AND TransmissionID = case when  @TransmissionID is null then TransmissionID else @TransmissionID end 
  AND DriveID        = case when  @DriveID is null then DriveID else @DriveID end 
  AND VehicleTypeID  = @VehicleTypeID

  
     
select @VehicleID AS 'VehicleID'
 

 
 




GO
