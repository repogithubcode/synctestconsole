USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[sp_GetBodyTypes]
	 @Year int = null,
	 @MakeID int = null,
	 @ModelID int = null,
	 @TrimLevelID int = null,
	 
	 -- Old query 
	 @VehicleID Int = null,
	@AdminInfoID int = null,
	@BodyTypeRequired Bit = 0 OUTPUT
AS

Begin
	-- Ezra - 11/10/2017 - Changed this query to the top part, this is for the graphical page.
	-- After deploy found that this query is called by other stored procedures, so as a temp fix added both signatures.
	-- TODO - split into 2 queries
	if NOT @Year IS NULL 
		BEGIN
			SELECT DISTINCT Bodys.*
			FROM vinn.dbo.vehicle_service_xref AS Xref
			JOIN vinn.dbo.Bodys ON Xref.EngineID = Bodys.BodyID
			WHERE 
				Xref.VinYear = @Year 
				AND Xref.MakeID = @MakeID 
				AND XRef.ModelID = @ModelID
				AND (ISNULL(@TrimLevelID, 0) = 0 OR Xref.SubModelID = @TrimLevelID)
		END
	ELSE
		BEGIN
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
		END

end

GO
