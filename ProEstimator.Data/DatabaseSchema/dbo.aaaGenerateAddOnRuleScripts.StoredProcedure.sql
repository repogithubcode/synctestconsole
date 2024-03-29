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
CREATE PROCEDURE [dbo].[aaaGenerateAddOnRuleScripts]
	@AddOnRuleID INT
AS
BEGIN
	
	SELECT 'INSERT INTO AddOnRule (Name, Deleted, Active) VALUES (''' + Name + ''', ' + CAST(Deleted AS VARCHAR) + ', ' + CAST(Active AS VARCHAR) + ');'
	FROM dbo.AddOnRule
	WHERE ID = @AddOnRuleID

	UNION 

	SELECT 'INSERT INTO AddOnRuleLine (AddOnRuleID, SortOrder, MatchType, MatchPiece, ChildRuleID, MatchText, Indented, Disabled) VALUES (' + CAST(AddOnRuleID AS VARCHAR) + ', ' + CAST(SortOrder AS VARCHAR) + ', ' + CAST(MatchType AS VARCHAR) + ', ' + CAST(MatchPiece AS VARCHAR) + ', ' + CAST(ChildRuleID AS VARCHAR) + ', ''' + MatchText + ''', ' + CAST(Indented AS VARCHAR) + ', ' + CAST(Disabled AS VARCHAR) + ');'
	FROM dbo.AddOnRuleLine
	WHERE AddOnRuleID = @AddOnRuleID

	UNION 

	SELECT 'INSERT INTO AddOnRulePresetLink (RuleID, PresetID, AddAction) VALUES (' + CAST(RuleID AS VARCHAR) + ', ' + CAST(PresetID AS VARCHAR) + ', ''' + AddAction + ''');'
	FROM dbo.AddOnRulePresetLink
	WHERE RuleID = @AddOnRuleID

END
GO
