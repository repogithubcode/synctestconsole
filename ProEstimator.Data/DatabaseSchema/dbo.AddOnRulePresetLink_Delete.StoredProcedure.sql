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
CREATE PROCEDURE [dbo].[AddOnRulePresetLink_Delete]
	@ID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	DELETE
	FROM AddOnRulePresetLink
	WHERE ID = @ID
	
END
GO
