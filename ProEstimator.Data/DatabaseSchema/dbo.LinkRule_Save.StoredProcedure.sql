USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 9/27/2022
-- Description:	Save a LinkRule record
-- =============================================
CREATE PROCEDURE [dbo].[LinkRule_Save]
	  @ID				int
	, @RuleType			tinyint
	, @Deleted			bit
	, @Enabled			bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ID > 0)
		BEGIN
			UPDATE LinkRule
			SET
			      RuleType = @RuleType
				, Deleted = @Deleted
				, Enabled = @Enabled
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO LinkRule
			(
				  RuleType
				, Deleted
				, Enabled
			)
			VALUES
			(
				  @RuleType
				, @Deleted
				, @Enabled
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
