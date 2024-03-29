USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 7/16/2019
-- Description:	Mark renewal as unactive
-- =============================================
CREATE PROCEDURE  [Admin].[DeleteRenewal] @id      
	INT = NULL
AS
BEGIN
	UPDATE	[dbo].[RenewalReport]
	SET		[Active] = 0
	WHERE	[ID] = @id
END
GO
