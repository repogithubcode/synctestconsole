USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 6/14/2021
-- =============================================
CREATE PROCEDURE [dbo].[Estimate_GetChildLineIDs]
	@ParentLineID			int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT ID
	FROM EstimationLineItems
	WHERE ParentLineID = @ParentLineID

END
GO
