SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 8/13/2018
-- Description:	Insert parts now enrollment
-- =============================================
ALTER PROCEDURE [dbo].[INSERT_PnEnrollment]
	@LoginId int = null,
	@ShopId int = null,
	@ShopUri nvarchar(max) = null,
	@RequestId nvarchar(50) = null
AS
BEGIN

INSERT INTO [dbo].[PnEnrollment]
           ([LoginId]
           ,[ShopId]
           ,[ShopUri]
           ,[RequestId]
           ,[CreateDate])
     VALUES
           (@LoginId
           ,@ShopId
           ,@ShopUri
           ,@RequestId
           ,GETDATE())

END
