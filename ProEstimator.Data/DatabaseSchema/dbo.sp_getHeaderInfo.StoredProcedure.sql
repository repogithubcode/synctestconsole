USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 
CREATE PROCEDURE [dbo].[sp_getHeaderInfo]
	@AdminInfoID int
AS
BEGIN
select 
	AdminInfo.id
	, AdminInfo.EstimateNumber
	, tbl_ContactPerson.FirstName + ' ' + tbl_ContactPerson.LastName AS Name
	,isnull(AdminInfo.GrandTotal,0) as Total
	,CASE WHEN ISNULL(VehicleInfoManual.ManualSelection, 0) = 1 THEN
			ISNULL(VehicleInfoManual.ModelYear,'') + ' ' + 
			ISNULL(VehicleInfoManual.Make,'') + ' ' +
			ISNULL(VehicleInfoManual.Model,'') + ' ' + 
			ISNULL(VehicleInfoManual.SubModel,'')
		ELSE
			FocusWrite.dbo.GetVehicleName(VehicleInfo.VehicleID)
	END 'Vehicle'
	, VehicleInfo.Vin
	, VehicleInfo.VehicleValue
	, StatusHistory.JobStatusID
	, JobStatus.Description AS JobStatusName
	, ISNULL(AdminInfo.GrandTotal, '') AS GrandTotal
	, ISNULL(AdminInfo.WorkOrderNumber, '') AS WorkOrderNumber
	, ISNULL(CustomerProfilesMisc.TotalLossPerc, 0) AS TotalLossPercent
FROM AdminInfo	
	INNER JOIN EstimationData ON AdminInfo.id = EstimationData.AdminInfoID
	LEFT OUTER JOIN Customer ON AdminInfo.CustomerID = Customer.ID
	LEFT OUTER JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID
	LEFT OUTER JOIN VehicleInfo ON EstimationData.ID = VehicleInfo.EstimationDataID
	LEFT OUTER JOIN VehicleInfoManual ON VehicleInfo.ID = VehicleInfoManual.VehicleInfoID
	LEFT OUTER JOIN
	(
		SELECT TOP 1 *
		FROM JobStatusHistory
		WHERE AdminInfoID = @AdminInfoID
		ORDER BY ID DESC
	) AS StatusHistory ON 1 = 1
	LEFT OUTER JOIN JobStatus ON StatusHistory.JobStatusID = JobStatus.id

	LEFT OUTER JOIN CustomerProfilesMisc ON AdminInfo.CustomerProfilesID = CustomerProfilesMisc.CustomerProfilesID
WHERE AdminInfo.id = @AdminInfoID
END

GO
