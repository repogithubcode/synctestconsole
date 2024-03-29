USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
 
  
CREATE PROCEDURE [dbo].[sp_getVehicleProductionDate] 
@AdminInfoId int 
AS 
BEGIN 
  
	SET NOCOUNT ON;  
	SELECT   v.[ProductionDate] 
  FROM [FocusWrite].[dbo].[VehicleInfo] as v with(nolock) 
	inner join EstimationData as e with(nolock) 
		on e.id = v.EstimationDataId 
  where e.AdminInfoID = @AdminInfoId 
END 
GO
