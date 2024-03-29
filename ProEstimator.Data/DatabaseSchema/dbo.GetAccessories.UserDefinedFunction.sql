USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE FUNCTION [dbo].[GetAccessories] (@AdminInfoID Int)
RETURNS VarChar(5000) AS  
BEGIN 	
	DECLARE @Accessories VarChar(5000)
	SELECT @Accessories = ''
	
	SELECT	@Accessories = @Accessories + Accessories.Accessory + ',' 
		
	FROM AdminInfo WITH (NOLOCK)
	INNER JOIN EstimationData WITH (NOLOCK) ON
		(EstimationData.AdminInfoID = AdminInfo.ID)
	INNER JOIN VehicleInfo WITH (NOLOCK) ON
		(VehicleInfo.EstimationDataID = EstimationData.ID)
	INNER JOIN VehicleAccessories WITH (NOLOCK)  ON
		(VehicleAccessories.VehicleInfoID = VehicleInfo.ID)
	INNER JOIN Accessories WITH (NOLOCK) ON
		(Accessories.ID = VehicleAccessories.AccessoriesID)
	WHERE AdminInfo.ID = @AdminInfoID
	SELECT @Accessories = RTRIM(@Accessories)
	IF LEN(@Accessories)>2 
		SELECT @Accessories = LEFT(@Accessories ,LEN(@Accessories)-1)
	ELSE
		SELECT @Accessories = ''
	
	RETURN @Accessories
	
END





GO
