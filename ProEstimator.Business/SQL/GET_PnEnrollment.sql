SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 8/13/2018
-- Description:	Retrieve parts now enrollment
-- =============================================
CREATE PROCEDURE GET_PnEnrollment
	@LoginId int = null
AS
BEGIN

SELECT [Id]
      ,[LoginId]
      ,[ShopId]
      ,[ShopUri]
      ,[RequestId]
      ,[CreateDate]
  FROM [dbo].[PnEnrollment]
  WHERE LoginId = @LoginId
END
