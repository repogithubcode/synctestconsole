USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 11/7/2018
-- Description:	Update record to track import status
-- =============================================
CREATE PROCEDURE [Admin].[UpdateImport]
	@ID int = null
	,@Content nvarchar(max) = null
	,@Status int = null
	,@DequeueDate datetime = null
AS
BEGIN

UPDATE [dbo].[ImportQueueLog]
   SET [Content] = @Content
      ,[Status] = @Status
      ,[DequeueDate] = @DequeueDate
 WHERE ID = @ID

END

/****** Object:  StoredProcedure [dbo].[GET_IMPORT_MESSAGE_BY_STATUS]    Script Date: 12/12/2018 2:22:28 PM ******/
SET ANSI_NULLS ON
GO
