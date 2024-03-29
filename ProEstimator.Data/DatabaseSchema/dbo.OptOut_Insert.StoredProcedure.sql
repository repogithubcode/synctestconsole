USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 6/12/23
-- Description:	Insert a new Opt Out record
-- =============================================
CREATE PROCEDURE [dbo].[OptOut_Insert]
	@LoginID				int
	, @TypeID				tinyint
	, @DetailID				int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    INSERT INTO OptOut
	(
		  LoginID
		, TypeID
		, DetailID
		, TimeStamp
	)
	VALUES
	(
		  @LoginID
		, @TypeID
		, @DetailID
		, GETDATE()
	)
END
GO
