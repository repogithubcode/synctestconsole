USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteVehicleInfoManualByAdminInfo]
	@admininfoid int
AS
DELETE FROM VehicleInfoManual
FROM VehicleInfoManual vim
INNER JOIN VehicleInfo vi
INNER JOIN EstimationData e
ON e.id = vi.estimationdataid
On vi.id = vim.vehicleinfoid
where e.admininfoid = @admininfoid


GO
