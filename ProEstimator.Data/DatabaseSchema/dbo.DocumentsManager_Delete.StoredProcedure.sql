USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[DocumentsManager_Delete]
	 @DocumentID    INT
AS
BEGIN
		UPDATE	[dbo].[Document] SET [IsDeleted] = 1
		WHERE	DocumentID = @DocumentID

		SELECT @DocumentID AS DocumentID
END


GO
