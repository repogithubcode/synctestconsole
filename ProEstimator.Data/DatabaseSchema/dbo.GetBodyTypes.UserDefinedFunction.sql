USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[GetBodyTypes] (@VehicleID Int)
RETURNS VarChar(3000) AS  
BEGIN 	
	
	DECLARE @BodyTypes VarCHar(3000)
	SELECT @BodyTypes = ''
	
	
	SELECT @BodyTypes = @BodyTypes + 
			RTRIM(LTRIM(BodyNumDoors.BodyNumDoors)) + ' Door ' + 
			RTRIM(LTRIM(BodyType.BodyTypeName)) + '<BR>' 
	FROM Vehicles.dbo.SubModel VSubModel WITH (NOLOCK)
	INNER JOIN VCDB_Public.dbo.VehicleToBodyStyleConfig VehicleToBodyStyleConfig WITH (NOLOCK) ON
		(VehicleToBodyStyleConfig.VehicleID = VSubModel.AAIAVehicleID)
	INNER JOIN VCDB_Public.dbo.BodyStyleConfig BodyStyleConfig WITH (NOLOCK) ON
		(BodyStyleConfig.BodyStyleConfigID = VehicleToBodyStyleConfig.BodyStyleConfigID)
	INNER JOIN VCDB_Public.dbo.BodyType BodyType WITH (NOLOCK) ON
		(BodyType.BodyTypeID = BodyStyleConfig.BodyTypeID)
	LEFT JOIN VCDB_Public.dbo.BodyNumDoors BodyNumDoors WITH (NOLOCK) ON
		(BodyNumDoors.BodyNumDoorsID = BodyStyleConfig.BodyNumDoorsID)
	WHERE VSubModel.id = @VehicleId
	
	IF LEN(@BodyTypes) > 4
		SELECT @BodyTypes = LEFT(@BodyTypes,LEN(@BodyTypes)-4)
	RETURN @BodyTypes
	
END



GO
