USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 9/27/2022
-- =============================================
CREATE PROCEDURE [dbo].[LinkRuleLine_Get]
	  @ID				int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *
	FROM LinkRuleLine
	WHERE ID = @ID

END
GO
