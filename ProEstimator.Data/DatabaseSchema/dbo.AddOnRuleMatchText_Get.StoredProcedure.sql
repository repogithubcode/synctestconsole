USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 11/5/2020
-- =============================================
CREATE PROCEDURE [dbo].[AddOnRuleMatchText_Get]
	  @ID				int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *
	FROM AddOnRuleMatchText
	WHERE ID = @ID

END
GO
