USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 10/7/2019
-- Description:	Get just the vehicle options as a comma seperated list
-- =============================================
CREATE PROCEDURE [dbo].[EstimateReport_GetVehicleOptions]
	@AdminInfoID			INT
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

     DECLARE @VehicleOptions VARCHAR(2000) = ''

	SELECT @VehicleOptions = @VehicleOptions + Accessory + ', '
	FROM EstimationData
	LEFT JOIN VehicleInfo ON VehicleInfo.EstimationDataId = EstimationData.id
	LEFT JOIN VehicleSelectedOptions ON VehicleSelectedOptions.VehicleInfoID = VehicleInfo.id
	LEFT JOIN Accessories ON Accessories.id = VehicleSelectedOptions.VehicleOptionsID
	WHERE EstimationData.AdminInfoID = @AdminInfoID
  
	IF @VehicleOptions <> '' 
		SET @VehicleOptions = SUBSTRING(@VehicleOptions, 0, LEN(@VehicleOptions)) 

	SELECT @VehicleOptions AS VehicleOptions
END
GO
