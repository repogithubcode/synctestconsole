USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[GetVehicleSelectedOptionsByAdminInfo]
@AdminInfoId int
AS
SELECT 	[VehicleInfoID]
	,[VehicleOptionsID]
FROM VehicleSelectedOptions vso with(nolock)
INNER JOIN VehicleInfo vi with(nolock)
INNER JOIN EstimationData e with(nolock)
INNER JOIN AdminInfo a with(nolock)
ON e.AdminInfoId = a.id
on vi.estimationdataid = e.id
on vi.id = vso.vehicleinfoid
where a.id = @AdminInfoId


GO
