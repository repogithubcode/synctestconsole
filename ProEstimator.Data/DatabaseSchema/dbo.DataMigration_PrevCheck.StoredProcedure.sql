USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 12/24/2018
-- Description:	Check for previous import attempt
-- =============================================
CREATE PROCEDURE [dbo].[DataMigration_PrevCheck]
	@loginId int = null
AS
BEGIN

SELECT [Id]
      ,[LoginId]
      ,[EmailAdddress]
      ,[Content]
      ,[Status]
      ,[EnqueueDate]
      ,[DequeueDate]
      ,[CreateDate]
  FROM [dbo].[ImportQueueLog]
  WHERE loginId = @loginId

END
GO
