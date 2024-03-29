USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- Author:		Ezra
-- Create date: 7/17/2017
-- Description:	Log an error to the Errors table
-- =============================================
CREATE PROCEDURE [dbo].[Error_Log]
	@LoginID		int,
	@AdminInfoID	int,
	@ErrorText		varchar(2000),
	@ErrorTag		varchar(255)
	
AS
BEGIN
	INSERT INTO Errors (LoginID, AdminInfoID, ErrorText, TimeOccurred, FixNote) 
	VALUES (@LoginID, @AdminInfoID, @ErrorText, GETDATE(), @ErrorTag) 
END

GO
