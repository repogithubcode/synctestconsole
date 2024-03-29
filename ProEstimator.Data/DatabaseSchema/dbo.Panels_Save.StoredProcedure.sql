USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 9/27/2022
-- Description:	Save a Panel record
-- =============================================
CREATE PROCEDURE [dbo].[Panels_Save] 
	  @ID				int
	, @PanelName		varchar(100)
	, @SortOrder		int
	, @Symmetry			bit
	, @SectionLinkRuleID		int
	, @PrimarySectionLinkRuleID	int
	, @PrimaryPanelLinkRuleID	int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ID > 0)
		BEGIN
			UPDATE Panels
			SET
				  PanelName = @PanelName
				, SortOrder = @SortOrder
				, Symmetry = @Symmetry
				, SectionLinkRuleID = @SectionLinkRuleID
				, PrimarySectionLinkRuleID = @PrimarySectionLinkRuleID
				, PrimaryPanelLinkRuleID = @PrimaryPanelLinkRuleID
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO Panels
			(
				  PanelName
				, SortOrder
				, Symmetry
				, SectionLinkRuleID
				, PrimarySectionLinkRuleID
				, PrimaryPanelLinkRuleID
			)
			VALUES
			(
				  @PanelName
				, @SortOrder
				, @Symmetry
				, @SectionLinkRuleID
				, @PrimarySectionLinkRuleID
				, @PrimaryPanelLinkRuleID
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
