USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Josh Blade
-- Create date: 4/8/16
-- Description:	Update service barcode in VehicleInfo table (used on Vehicle page after trim is selected so other drop downs can be populated).
-- =============================================
CREATE PROCEDURE [dbo].[UpdateServiceBarcode]
	@Year int
	,@MakeID int
	,@ModelID int
	,@TrimLevelID int
	,@AdminInfoID int
AS
BEGIN
	declare @VehicleID int 
	declare @ServiceBarcode int
	declare @EngineID int 
	
	select distinct top 1 @VehicleID = a.VehicleID, @ServiceBarcode = a.Service_Barcode,@EngineID=a.EngineID
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
	order by EngineID desc -- added by RB to force 4 doors to populate when more than one choice.

	update VehicleInfo
	set Service_Barcode = @ServiceBarcode
	from Focuswrite.dbo.VehicleInfo as v  with(nolock)
	inner join Focuswrite.dbo.EstimationData as e  with(nolock)
	on e.id = v.EstimationDataId  
	where AdminInfoID = @AdminInfoID
	 
	select @VehicleID as VehicleID, @ServiceBarcode as ServiceBarcode
END


GO
