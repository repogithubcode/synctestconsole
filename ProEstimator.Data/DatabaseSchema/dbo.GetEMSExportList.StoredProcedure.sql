USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[GetEMSExportList]
	@AdminInfoID int
AS
BEGIN

	SELECT msgID id, msgAddDate CreationDateTime, msgStatusLong Status, msgValue3 ExportFormat
	FROM MessageQueue WITH (NOLOCK)
	WHERE msgType = 1
	AND 
		CAST(@AdminInfoID as varchar(20)) = msgValue1
	AND
		msgViewable = 1

END


GO
