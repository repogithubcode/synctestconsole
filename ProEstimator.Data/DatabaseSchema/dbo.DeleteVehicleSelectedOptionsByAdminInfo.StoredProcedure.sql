USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteVehicleSelectedOptionsByAdminInfo]
	@admininfoid int
AS
DELETE FROM VehicleSelectedOptions


GO
