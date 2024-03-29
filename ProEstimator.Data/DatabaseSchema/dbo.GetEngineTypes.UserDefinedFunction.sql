USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[GetEngineTypes] (@VehicleID Int)
RETURNS VarChar(3000) AS  
BEGIN 	
	DECLARE @EngineTypes VarCHar(3000)
	SELECT @EngineTypes = ''
	
	SELECT @EngineTypes = @EngineTypes + 
		CASE 	WHEN 	EngineBase.Cylinders IS NOT NULL THEN 'V' + LTRIM(RTRIM(CONVERT(VarChar(3),EngineBase.Cylinders)))
			ELSE ''
		END +
		CASE	WHEN EngineBase.Cylinders IS NOT NULL AND (EngineBase.Liter IS NOT NULL OR Aspiration.AspirationName IN ('S','T')) THEN ', '
			ELSE ''
		END +
		CASE 	WHEN 	EngineBase.Liter IS NOT NULL THEN LTRIM(RTRIM(CONVERT(VarChar(10),EngineBase.Liter))) + 'L'
			ELSE ''
		END +
		CASE 	WHEN EngineBase.Liter IS NOT NULL  AND Aspiration.AspirationName IN ('S','T') THEN '; '
			ELSE ''
		END +
		CASE 	WHEN Aspiration.AspirationName = 'S' THEN 'Supercharged'
			WHEN Aspiration.AspirationName = 'T' THEN 'Turbocharged'
			ELSE ''
		END + '<BR>' 
	FROM Vehicles.dbo.SubModel VSubModel WITH (NOLOCK)
	INNER JOIN VCDB_Public.dbo.VehicleToEngineConfig VehicleToEngineConfig WITH (NOLOCK) ON
		(VehicleToEngineConfig.VehicleID = VSubModel.AAIAVehicleID)
	INNER JOIN VCDB_Public.dbo.EngineConfig EngineConfig WITH (NOLOCK) ON
		(EngineConfig.EngineConfigID = VehicleToEngineConfig.EngineConfigID)
	INNER JOIN VCDB_Public.dbo.EngineBase EngineBase WITH (NOLOCK) ON
		(EngineBase.EngineBaseID = EngineConfig.EngineBaseID)
	INNER JOIN VCDB_Public.dbo.Aspiration Aspiration WITH (NOLOCK) ON
		(Aspiration.AspirationID = EngineConfig.AspirationID)
	WHERE VSubModel.id = @VehicleId

	IF LEN(@EngineTypes) > 4
		SELECT @EngineTypes = LEFT(@EngineTypes,LEN(@EngineTypes)-4)
	RETURN @EngineTypes
END



GO
