USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 12/1/2017 
-- Description: Delete accessory links for an estimate. 
-- ============================================= 
CREATE PROCEDURE [dbo].[Accessories_InsertLink] 
	@AdminInfoID		int, 
	@AccessoryID		int 
AS 
BEGIN 
 
	DECLARE @VehicleInfoID INT = 0 
 
	SELECT @VehicleInfoID = VehicleInfo.ID 
	FROM EstimationData with(nolock) 
	JOIN VehicleInfo with(nolock) ON VehicleInfo.EstimationDataID = EstimationData.ID 
	WHERE EstimationData.AdminInfoID = @AdminInfoID 
 
	INSERT INTO VehicleSelectedOptions (VehicleInfoID, VehicleOptionsID) VALUES (@VehicleInfoID, @AccessoryID) 
 
END 
GO
