USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
create PROCEDURE [dbo].[getTransmissionTypeByAdminInfoID]
	@AdminInfoID int
	 
AS
BEGIN
	 
	
	 
	select v.TransmissionType
	from VehicleInfo as v
		inner join EstimationData as e
			on e.id = v.EstimationDataId
	where e.AdminInfoID = @AdminInfoID
 
END


GO
