USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[GetDriveTypes] (@VehicleID Int)
RETURNS VarChar(3000) AS  
BEGIN 	
	DECLARE @DriveTypes VarCHar(3000)
	SELECT @DriveTypes = ''
	
	SELECT @DriveTypes = @DriveTypes + 
			RTRIM(LTRIM(DriveType.DriveTypeName)) + '/' 
	FROM Vehicles.dbo.SubModel VSubModel WITH (NOLOCK)
	INNER JOIN VCDB_Public.dbo.VehicleToDriveType VehicleToDriveType WITH (NOLOCK) ON
		(VehicleToDriveType.VehicleID = VSubModel.AAIAVehicleID)
	INNER JOIN VCDB_Public.dbo.DriveType DriveType WITH (NOLOCK) ON
		(DriveType.DriveTypeID = VehicleToDriveType.DriveTypeID)
	WHERE VSubModel.id = @VehicleId
	
	IF LEN(@DriveTypes) > 1
		SELECT @DriveTypes = LEFT(@DriveTypes,LEN(@DriveTypes)-1)
	RETURN @DriveTypes
END



GO
