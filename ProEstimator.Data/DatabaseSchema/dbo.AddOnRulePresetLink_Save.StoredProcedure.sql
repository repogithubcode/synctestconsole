USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 11/18/2020
-- =============================================
CREATE PROCEDURE [dbo].[AddOnRulePresetLink_Save]
	  @ID				int
	, @RuleID			int
	, @PresetID			int
	, @AddAction		varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ID > 0)
		BEGIN
			UPDATE AddOnRulePresetLink
			SET
				  RuleID = @RuleID
				, PresetID = @PresetID
				, AddAction = @AddAction
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO AddOnRulePresetLink
			(
			      RuleID
				, PresetID
				, AddAction
			)
			VALUES 
			(
				  @RuleID
				, @PresetID 
				, @AddAction
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT)
		END
END
GO
