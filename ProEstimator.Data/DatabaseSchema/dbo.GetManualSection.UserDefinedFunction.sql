USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Ezra
-- Create date: 8/6/2018	
-- Description:	For manual vehicle estimates, get the mapped section by the PDR Panel ID
-- =============================================
CREATE FUNCTION [dbo].[GetManualSection] 
(
	@PanelID		int
)
RETURNS varchar(50)
AS
BEGIN
	RETURN 
	(
		SELECT Section
		FROM ManualSections 
		WHERE ID = @PanelID
	)
END


GO
