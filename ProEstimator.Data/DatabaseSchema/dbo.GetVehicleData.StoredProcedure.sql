USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO




CREATE Procedure [dbo].[GetVehicleData]
	@VehicleID Int,
	@GroupNumber Int,
	@PartNumber VarChar(50),
	@AdminInfoID Int
AS

/*IF @VehicleID = 35826
BEGIN
	SELECT 	VehicleData.Step 'Section',
		VehicleData.PartNumber, 
		ISNULL(VehicleData.QuantityRequired,0) 'QuantityRequired',  
		ISNULL(VehicleData.LaborType,0) 'LaborType',  
		ISNULL(VehicleData.LaborTime,0) 'Labor',  
		ISNULL(VehicleData.PaintType,0) 'PaintType',  
		ISNULL(VehicleData.PaintTime,0) 'Paint',
		ISNULL(VehicleData.RepairTime,0) 'Repair',
		ISNULL(VehicleData.ReplaceTime,0) 'Replace',
		ISNULL(VehicleData.RITime,0) 'RI',
		ISNULL(VehicleData.RRTime,0) 'RR',
		VehicleData.Description,
		RateTypes.RateName,
		VehicleData.Paintable,
		Parts2.Price,
		VehicleData.Step 'StepID',
		'' 'VehiclePosition',
		CustomerProfilesPaint.*
	FROM AdminInfo WITH (NOLOCK)
	LEFT JOIN VehicleData WITH (NOLOCK) ON
		(VehicleID = @VehicleID AND
		 VehicleData.Step = @GroupNumber AND
		 VehicleData.PartNumber = @PartNumber)
	LEFT JOIN RateTypes WITH (NOLOCK) ON 
		(RateTypes.id = VehicleData.LaborType)
	LEFT JOIN CustomerProfilesPaint WITH (NOLOCK) ON
		(CustomerProfilesPaint.CustomerProfilesID = AdminInfo.CustomerProfilesID)
	LEFT JOIN Honda.Dbo.Parts2 Parts2 WITH (NOLOCK) ON
		(Parts2.PartNumber = @PartNumber)
	WHERE 	
		AdminInfo.id = @AdmininfoID
END
ELSE 
IF EXISTS (	SELECT VehicleIDtoMitchell2VehicleID.FWVehicleID
			FROM Mitchell2.Dbo.VehicleIDtoMitchell2VehicleID VehicleIDtoMitchell2VehicleID WITH (NOLOCK)
			INNER JOIN EstimationData WITH (NOLOCK) ON
				(EstimationData.AdminInfoID = @AdminInfoID)
			INNER JOIN VehicleInfo WITH (NOLOCK) ON
				(VehicleInfo.EstimationDataId = EstimationData.Id)
			WHERE VehicleIDtoMitchell2VehicleID.FWVehicleID = VehicleInfo.VehicleID AND
				(VehicleIDtoMitchell2VehicleID.BodyType = -1 OR VehicleIDtoMitchell2VehicleID.BodyType = VehicleInfo.BodyType) )
BEGIN
	SELECT  Mitchell2Subsections.ID 'Section',
		Mitchell2Parts.PartNumber, 
		1 'QuantityRequired',  

		CASE	WHEN ISNULL(Mitchell2Parts.LaborCategoryID,'') in ('',' ') THEN 1
			WHEN Mitchell2Parts.LaborCategoryID = 'm' THEN 4
			WHEN Mitchell2Parts.LaborCategoryID = 's' THEN 5
		END 'LaborType',  
		ISNULL(Mitchell2Parts.LaborTime,0) 'Labor',  
		ISNULL(9,0) 'PaintType',  
		CONVERT(Real,ISNULL(Mitchell2LaborItems.LaborTime,0)) 'Paint',
		CONVERT(Real,ISNULL(0,0)) 'Repair',
		CONVERT(Real,ISNULL(Mitchell2Parts.LaborTime,0)) 'Replace',
		CONVERT(Real,ISNULL(Mitchell2Parts.LaborTime,0)) 'RI',
		CONVERT(Real,ISNULL(Mitchell2Parts.LaborTime,0)) 'RR',
		ISNULL(Mitchell2Parts.Text, 'Unk. Desc.') + 
		CASE 	WHEN Mitchell2Parts.Side = 'R' THEN 'Right '
			WHEN Mitchell2Parts.Side  = 'L' THEN 'Left '
			WHEN ISNULL(Mitchell2Parts.Side ,'') <> '' THEN ISNULL(Mitchell2Parts.Side ,'') + ' ' 
			ELSE ''
		END + '(' +
		ISNULL(Mitchell2WheelDrives.WheelDriveText+'/','') +
		ISNULL(Mitchell2TransmissionTypes.TypeText+'/','') +
		ISNULL(Mitchell2Engines.EngineText+'/','') +
		ISNULL(Mitchell2EnginSizes.SizeText+'/','')  'Description',
		RateTypes.RateName,
		CASE WHEN CONVERT(Real,ISNULL(Mitchell2LaborItems.LaborTime,0))>0 THEN 1 ELSE 0 END 'Paintable',
		ISNULL(Mitchell2Parts.Price,0) 'Price',
		-1 'StepID',
		Mitchell2Parts.Side 'VehiclePosition',
		CustomerProfilesPaint.*

	FROM  Mitchell2.Dbo.VehicleIDtoMitchell2VEhicleID VehicleIDtoMitchell2VEhicleID WITH (NOLOCK)
	INNER JOIN Mitchell2.Dbo.Vehicles Mitchell2Vehicles WITH (NOLOCK) ON
		(Mitchell2Vehicles.ID = VehicleIDtoMitchell2VEhicleID.Mitchell2VehicleID )
	INNER JOIN Mitchell2.Dbo.Sections Mitchell2Sections WITH (NOLOCK) ON
		(Mitchell2Sections.VehicleID = Mitchell2Vehicles.ID)
	INNER JOIN Mitchell2.Dbo.Subsections Mitchell2Subsections WITH (NOLOCK) ON
		(Mitchell2Subsections.SectionID = Mitchell2Sections.ID)
	INNER JOIN Mitchell2.Dbo.DetailGroups Mitchell2DetailGroups WITH (NOLOCK) ON
		(Mitchell2DetailGroups.VehicleID = Mitchell2Vehicles.ID)
	INNER JOIN Mitchell2.Dbo.Parts Mitchell2Parts WITH (NOLOCK) ON
		(Mitchell2Parts.DetailGroupID = Mitchell2DetailGroups.ID)
	--More Info--
	LEFT JOIN Mitchell2.Dbo.PartNotes Mitchell2PartNotes WITH (NOLOCK) ON
		(Mitchell2PartNotes.PartID = Mitchell2Parts.ID)
	LEFT JOIN Mitchell2.Dbo.WheelDrives Mitchell2WheelDrives WITH (NOLOCK) ON
		(Mitchell2WheelDrives.ID = Mitchell2Parts.WheelDriveID)
	LEFT JOIN Mitchell2.Dbo.TransmissionTypes Mitchell2TransmissionTypes WITH (NOLOCK) ON
		(Mitchell2TransmissionTypes.ID = Mitchell2Parts.TransmissionTypeID)
	LEFT JOIN Mitchell2.Dbo.Engines Mitchell2Engines WITH (NOLOCK) ON
		(Mitchell2Engines.ID = Mitchell2Parts.EngineID)
	LEFT JOIN Mitchell2.Dbo.EngineSizes Mitchell2EnginSizes WITH (NOLOCK) ON
		(Mitchell2EnginSizes.ID = Mitchell2Parts.EngineSizeID)

	LEFT JOIN Mitchell2.dbo.LaborItems Mitchell2LaborItems WITH (NOLOCK) ON
		(Mitchell2LaborItems.GroupID = Mitchell2DetailGroups.ID AND
		 Mitchell2LaborItems.LaborText LIKE 'Refinish%' AND
		 Mitchell2LaborItems.LaborText LIKE '%' + Mitchell2Parts.Text + '%' AND
		 ISNULL(Mitchell2LaborItems.EngineID,-1) = Mitchell2Parts.EngineID AND
		 ISNULL(Mitchell2LaborItems.EngineSizeID,-1) = Mitchell2Parts.EngineSizeID AND
		 ISNULL(Mitchell2LaborItems.TransmissionTypeID,-1) = Mitchell2Parts.TransmissionTypeID AND
		 ISNULL(Mitchell2LaborItems.WheelDriveID,-1) = Mitchell2Parts.WheelDriveID	)

	INNER JOIN AdminInfo WITH (NOLOCK) ON
		(AdminInfo.id = @AdminInfoID)
	INNER JOIN EstimationData WITH (NOLOCK) ON
		(EstimationData.AdminInfoID = AdminInfo.id)	
	INNER JOIN VehicleInfo WITH (NOLOCK) ON
		(VehicleInfo.EstimationDataId = EstimationData.Id )
	LEFT JOIN RateTypes WITH (NOLOCK) ON 
		(RateTypes.id = CASE	WHEN ISNULL(Mitchell2Parts.LaborCategoryID,'') in ('',' ') THEN 1
					WHEN Mitchell2Parts.LaborCategoryID = 'm' THEN 4
					WHEN Mitchell2Parts.LaborCategoryID = 's' THEN 5
				END)

	LEFT JOIN CustomerProfilesPaint WITH (NOLOCK) ON
		(CustomerProfilesPaint.CustomerProfilesID = AdminInfo.CustomerProfilesID)
	WHERE
		VehicleIDtoMitchell2VEhicleID.FWVehicleID = @VehicleID AND
		Mitchell2Parts.PartNumber = @PartNumber AND
		Mitchell2Subsections.ID = @GroupNumber
END
ELSE
BEGIN*/
	SELECT  subsectionhead.SubSectionID 'Section',
		DetailLine.partnumber, 
		1 'QuantityRequired',  

		CASE	WHEN ISNULL(DetailLine.laborcategorycode,'') in ('',' ') THEN 1
			WHEN DetailLine.laborcategorycode = 'm' THEN 4
			WHEN DetailLine.laborcategorycode = 's' THEN 5
		END 'LaborType',  
		ISNULL(DetailLine.labortime,0) 'Labor',  
		ISNULL(9,0) 'PaintType',  
		CONVERT(Real,ISNULL(RefinishTimes.labortime,0)) 'Paint',
		CONVERT(Real,ISNULL(0,0)) 'Repair',
		CONVERT(Real,ISNULL(DetailLine.labortime,0)) 'Replace',
		CONVERT(Real,ISNULL(DetailLine.labortime,0)) 'RI',
		CONVERT(Real,ISNULL(DetailLine.labortime,0)) 'RR',
		ISNULL(DescriptorDetailLine.TextValue, 'Unk. Desc.') + 
		CASE 	WHEN DetailLine.qualifier = 'R' AND CHARINDEX('Right',DescriptorDetailLine.TextValue)=0 THEN 'Right '
			WHEN DetailLine.qualifier = 'R' AND CHARINDEX('Right',DescriptorDetailLine.TextValue)>0 THEN ''
			WHEN DetailLine.qualifier  = 'L' AND CHARINDEX('Left',DescriptorDetailLine.TextValue)=0 THEN 'Left '
			WHEN DetailLine.qualifier  = 'L' AND CHARINDEX('Left',DescriptorDetailLine.TextValue)>0 THEN ''
			WHEN ISNULL(DetailLine.qualifier ,'') <> '' THEN ISNULL(DetailLine.qualifier ,'') + ' ' 
			ELSE ''
		END 'Description',
		RateTypes.RateName,
		CASE WHEN CONVERT(Real,ISNULL(DetailLine.labortime,0))>0 THEN 1 ELSE 0 END 'Paintable',
		ISNULL(DetailLine.price,0) 'Price',
		-1 'StepID',
		DetailLine.qualifier 'VehiclePosition',
		CustomerProfilesPaint.*

	FROM Mitchell.Dbo.ServicesToVehicleID ServicesToVehicleID  WITH (NOLOCK)
	INNER JOIN Mitchell.Dbo.service Service WITH (NOLOCK) ON
		(Service.barcode = ServicesToVehicleID.BarCode)
	INNER JOIN Mitchell.Dbo.CegDoc CegDoc WITH (NOLOCK) ON
		(CegDoc.CegDocID = Service.CegDocID)

	INNER JOIN Mitchell.dbo.section Section WITH (NOLOCK) ON
		(Section.CegDocID = CegDoc.CegDocID)
	INNER JOIN Mitchell.dbo.subsection SubSection WITH (NOLOCK) ON
		(SubSection.SectionID = Section.SectionID)
	LEFT JOIN Mitchell.dbo.subsectionhead subsectionhead WITH (NOLOCK) ON
		(subsectionhead.SubSectionID = SubSection.SectionID)
	INNER JOIN Mitchell.Dbo.detailgroup DetailGroup WITH (NOLOCK) ON
		(DetailGroup.SubSectionID = SubSection.SubSectionID)
	INNER JOIN Mitchell.Dbo.DetailLine DetailLine WITH (NOLOCK) ON
		(DetailLine.DetailGroupID = DetailGroup.DetailGroupID)
	INNER JOIN Mitchell.Dbo.DescriptorDetailLine DescriptorDetailLine WITH (NOLOCK) ON
		(DescriptorDetailLine.DetailLineID = DetailLine.DetailLineID)

	LEFT JOIN Mitchell.dbo.RefinishTimes RefinishTimes WITH (NOLOCK) ON
		(RefinishTimes.DetailGroupID = DetailGroup.DetailGroupID AND
		 RefinishTimes.partnumber = DetailLine.partnumber)

	INNER JOIN AdminInfo WITH (NOLOCK) ON
		(AdminInfo.id = @AdminInfoID)
	INNER JOIN EstimationData WITH (NOLOCK) ON
		(EstimationData.AdminInfoID = AdminInfo.id)	
	INNER JOIN VehicleInfo WITH (NOLOCK) ON
		(VehicleInfo.EstimationDataId = EstimationData.Id )
	LEFT JOIN RateTypes WITH (NOLOCK) ON 
		(RateTypes.id = CASE	WHEN ISNULL(DetailLine.laborcategorycode,'') in ('',' ') THEN 1
					WHEN DetailLine.laborcategorycode = 'm' THEN 4
					WHEN DetailLine.laborcategorycode = 's' THEN 5
				END)

	LEFT JOIN CustomerProfilesPaint WITH (NOLOCK) ON
		(CustomerProfilesPaint.CustomerProfilesID = AdminInfo.CustomerProfilesID)
	WHERE
		ServicesToVehicleID.VehicleID = @VehicleID AND
		DetailLine.PartNumber = @PartNumber AND
		SubSection.SubsectionID = @GroupNumber
--END




GO
