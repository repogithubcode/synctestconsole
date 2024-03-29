USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Ezra
-- Create date: 12/17/2020
-- =============================================
CREATE PROCEDURE [dbo].[AddOnPreset_GetForProfileAndRule]
	  @ProfileID		int
	, @RuleID			int
	, @EstimateID		int
	, @Action			varchar(30)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	IF @Action = 'R&I'
		BEGIN
			SET @Action = 'RI'
		END

	SELECT DISTINCT AddOnPreset.*
	FROM AddOnRulePresetLink
	LEFT OUTER JOIN AddOnPresetShell ON AddOnPresetShell.Deleted = 0 AND AddOnRulePresetLink.PresetID = AddOnPresetShell.ID
	LEFT OUTER JOIN 
	(
		SELECT
			  ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalID ELSE ID END, 0) AS ID
			, @ProfileID AS ProfileID
			, Base.GlobalPresetShellID AS PresetShellID
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalLabor ELSE Labor END, 0) AS Labor
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalRefinish ELSE Refinish END, 0) AS Refinish
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalCharge ELSE Charge END, 0) AS Charge
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalOtherTypeOverride ELSE OtherTypeOverride END, '') AS OtherTypeOverride
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalOtherCharge ELSE OtherCharge END, 0) AS OtherCharge
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalActive ELSE Active END, 0) AS Active
			, ISNULL(CASE WHEN Base.IsEmpty = 1 THEN GlobalAutoSelect ELSE AutoSelect END, 0) AS AutoSelect
		FROM
		(
			SELECT --*
				AddOnPreset.*
				, CASE WHEN
					ISNULL(AddOnPreset.Labor, 0) = 0
					AND ISNULL(AddOnPreset.Refinish, 0) = 0
					AND ISNULL(AddOnPreset.Charge, 0) = 0
					AND ISNULL(AddOnPreset.OtherTypeOverride, '') = ''
					AND ISNULL(AddOnPreset.OtherCharge, 0) = 0
				 THEN 1 ELSE 0 END AS IsEmpty
				 , GlobalPreset.Labor AS GlobalLabor
				 , GlobalPreset.Refinish AS GlobalRefinish
				 , GlobalPreset.Charge AS GlobalCharge
				 , GlobalPreset.OtherTypeOverride AS GlobalOtherTypeOverride
				 , GlobalPreset.OtherCharge AS GlobalOtherCharge
				 , GlobalPreset.Active AS GlobalActive
				 , GlobalPreset.ID AS GlobalID
				 , GlobalPreset.AutoSelect AS GlobalAutoSelect

				 , GlobalPreset.PresetShellID AS GlobalPresetShellID
			FROM AddOnPreset AS GlobalPreset
			LEFT OUTER JOIN AddOnPreset ON AddOnPreset.ProfileID = @ProfileID AND AddOnPreset.PresetShellID = GlobalPreset.PresetShellID
			WHERE GlobalPreset.ProfileID = 1
		) AS Base
	) AS AddOnPreset ON AddOnPresetShell.ID = AddOnPreset.PresetShellID 
	WHERE 
		AddOnRulePresetLink.RuleID = @RuleID
		AND AddOnRulePresetLink.AddAction = @Action
		AND AddOnPreset.Active = 1

		-- If the preset is only allowed once per vehicle, don't select it if it's already on the estimate
		AND
		(AddOnPresetShell.OnePerVehicle = 0 OR
			AddOnPresetShell.ID NOT IN 
			(
				SELECT DISTINCT ISNULL(PresetShellID, 0)
				FROM EstimationData
				LEFT OUTER JOIN EstimationLineItems ON EstimationData.ID = EstimationLineItems.EstimationDataID
				WHERE AdminInfoID = @EstimateID AND ISNULL(PresetShellID, 0) > 0
			)
		)
  
END
GO
