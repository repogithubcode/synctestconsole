USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra	
-- Create date: 11/19/2020
-- =============================================
CREATE PROCEDURE [dbo].[AddOnPreset_Save]
	  @ID				int
	, @ProfileID		int
	, @PresetShellID	int
	, @Labor			real
	, @Refinish			real
	, @Charge			money
	, @OtherTypeOverride	varchar(20)	
	, @OtherCharge		money
	, @Active			bit
	, @AutoSelect		bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- There was a bug somewhere where we ended up with multiple presets for the same shell ID.  To prevent this, check if there are multiple records for the same preset and delete the others
	IF 
	(
		SELECT COUNT(*)
		FROM AddOnPreset
		WHERE 
			ProfileID = @ProfileID 
			AND PresetShellID = @PresetShellID
			AND ID <> @ID
	) > 0
	BEGIN
		DELETE FROM AddOnPreset
		WHERE 
			ProfileID = @ProfileID 
			AND PresetShellID = @PresetShellID
			AND ID <> @ID
	END


    IF (@ID > 0)
		BEGIN
			UPDATE AddOnPreset
			SET
				  ProfileID = @ProfileID
				, PresetShellID = @PresetShellID
				, Labor = @Labor
				, Refinish = @Refinish
				, Charge = @Charge
				, OtherTypeOverride = @OtherTypeOverride
				, OtherCharge = @OtherCharge
				, Active = @Active
				, AutoSelect = @AutoSelect
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO AddOnPreset
			(
				  ProfileID
				, PresetShellID
				, Labor
				, Refinish
				, Charge
				, OtherTypeOverride
				, OtherCharge
				, Active
				, AutoSelect
			)
			VALUES
			(
				  @ProfileID
				, @PresetShellID
				, @Labor
				, @Refinish
				, @Charge
				, @OtherTypeOverride
				, @OtherCharge
				, @Active
				, @AutoSelect
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT)
		END
END
GO
