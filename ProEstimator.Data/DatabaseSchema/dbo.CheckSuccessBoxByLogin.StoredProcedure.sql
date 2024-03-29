USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[CheckSuccessBoxByLogin] 
	@LoginID				int 
AS 
BEGIN 
	IF EXISTS(SELECT ID FROM SuccessBoxManualSync WHERE LoginID = @LoginID)
	BEGIN
		SELECT 1 AS 'SuccessBoxLoginExists'
	END
	ELSE
	BEGIN
		SELECT 0 AS 'SuccessBoxLoginExists'
	END
END 

GO
