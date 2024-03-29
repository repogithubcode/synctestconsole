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
CREATE PROCEDURE [dbo].[UpdateVehicleInfo]
	-- Add the parameters for the stored procedure here
	@AdminInfoID int,
	@BodyId int = null,
	@DriveID int = null,
	@EngineID int = null,
	@TransmissionID int = null 
	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;
declare @VehicleID int,
		@MakeID int,
		@ModelID int,
		@SubModelID int,
		@Year int,
		@Service_Barcode int,
		@oldBodyID int,
		@oldDriveID int,
		@oldEngineID int,
		@oldTransID int
		
IF @BodyId > 0
Begin  print 'b'
	select @Year = x.VinYear,
		   @MakeID = x.MakeID,
		   @ModelID = x.ModelID,
		   @SubModelID = x.SubmodelID,
		   @oldBodyID = v.BodyType
	from Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
	where e.AdminInfoID = @AdminInfoID
if @BodyId <> @oldBodyID
begin print 'bb'
  goto getVeh
end 
else
begin
  goto Finish
end
END
IF @DriveID > 0
Begin print 'd'
	select @Year = x.VinYear,
		   @MakeID = x.MakeID,
		   @ModelID = x.ModelID,
		   @SubModelID = x.SubmodelID,
		   @BodyId = x.EngineID , -- Mitchell has BodyID and EngineID reversed
		   @oldDriveID = v.DriveType
	from Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
	where e.AdminInfoID = @AdminInfoID
if @DriveID <> @oldDriveID
begin
  goto getVeh
end 
else
begin
  goto Finish
end
END
IF @EngineID > 0
Begin print 'e'
	select @Year = x.VinYear,
		   @MakeID = x.MakeID,
		   @ModelID = x.ModelID,
		   @SubModelID = x.SubmodelID,
		   @BodyId = x.EngineID,  -- Mitchell has BodyID and EngineID reversed
		   @DriveID = x.DriveID,
		   @oldEngineID = v.EngineType
	from Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
	where e.AdminInfoID = @AdminInfoID
	
	 print @EngineID
	 print @oldEngineID
if @EngineID <> isnull(@oldEngineID,0)
begin
	print 'as'
  goto getVeh
end 
else
begin
  goto Finish
end
END
IF @TransmissionID > 0
Begin print 't'
	select @Year = x.VinYear,
		   @MakeID = x.MakeID,
		   @ModelID = x.ModelID,
		   @SubModelID = x.SubmodelID,
		   @BodyId = x.EngineID,  -- Mitchell has BodyID and EngineID reversed
		   @DriveID = x.DriveID,
		   @EngineID = x.BodyID,
		   @oldTransID = v.TransmissionType
	from Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
	where e.AdminInfoID = @AdminInfoID
if @TransmissionID <> isnull(@oldTransID,0)
begin
  goto getVeh
end 
else
begin
  goto Finish
end
END
    -- Insert statements for procedure here
 
getVeh: 
 
SELECT top 1 @VehicleID = x.vehicleid,
		@Service_Barcode = x.Service_Barcode
from vinn.dbo.Vehicle_Service_Xref x with(nolock)
	inner join vinn.dbo.VinToBody as vb with(nolock)
			on x.MakeID = vb.MakeID 
			and x.ModelID = vb.ModelID 
	inner join vinn.dbo.VinToDrive as vd with(nolock)
		on x.MakeID = vd.MakeID 
			and x.ModelID = vd.ModelID 
	inner join vinn.dbo.VinToEngine as ve with(nolock)
		on x.MakeID = ve.MakeID 
			and x.ModelID = ve.ModelID 
	inner join vinn.dbo.VinToTransmission as vt with(nolock)
		on x.MakeID = vt.MakeID 
			and x.ModelID = vt.ModelID  		
WHERE x.VinYear        = @Year 
  AND x.MakeID       = @MakeID 
  AND x.ModelID      = @ModelID
  AND x.SubmodelID     = @SubmodelID
  and x.EngineID        = case when  @BodyID is null then vb.BodyID else @BodyID end  
  and x.BodyID      = case when  @EngineID is null then ve.EngineID else @EngineID end     -- Vehicle_Service_Xref has these backwards  engineid,bodyid 
  
  AND vt.TransmissionID = case when  @TransmissionID is null then vt.TransmissionID else @TransmissionID end 
  AND vd.DriveID        = case when  @DriveID is null then vd.DriveID else @DriveID end 
  
--select @VehicleID,@Service_Barcode,@MakeID,@ModelID,@SubModelID,@BodyID,@DriveID,@EngineID,@TransmissionID
goto updateveh

updateveh:
print 'yes'
	if @VehicleID is not null
	Begin
		print @BodyId
	   update v
	   set v.VehicleID = @VehicleID,
		   v.Service_Barcode = @Service_Barcode,
		   v.MakeID = @MakeID,
		   v.ModelID = @ModelID,
		   v.SubModelID = @SubModelID,
		   v.BodyType = @BodyId,
		   v.DriveType = @DriveID ,
		   v.EngineType = @EngineID,
		   v.TransmissionType = @TransmissionID 	   		 
	   from Focuswrite.dbo.EstimationData as e with(nolock)
			inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
				on e.id = v.EstimationDataId  
	   where e.AdminInfoID = @AdminInfoID
	   
	  goto Finish
   END
Finish:

END

GO
