USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--FrameMatrix_GetVehicleYears-01.sql
CREATE procedure [dbo].[FrameMatrix_GetVehicleYears] 
as 
Begin
	SET NOCOUNT ON;

	SELECT
		DISTINCT carYear
	FROM FrameMatrix.dbo.etl_VehicleMatrix with(nolock) 
	Order By carYear desc
End
GO
