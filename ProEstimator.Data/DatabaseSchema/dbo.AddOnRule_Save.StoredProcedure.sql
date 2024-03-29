USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 11/5/2020
-- Description:	Save an Add On Rule record
-- =============================================
CREATE PROCEDURE [dbo].[AddOnRule_Save]
	  @ID				int
	, @Name				varchar(50)
	, @Deleted			bit
	, @Active			bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ID > 0)
		BEGIN
			UPDATE AddOnRule
			SET
				  Name = @Name
				, Deleted = @Deleted
				, Active = @Active
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO AddOnRule
			(
				  Name
				, Deleted
				, Active
			)
			VALUES
			(
				  @Name
				, @Deleted
				, @Active
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
