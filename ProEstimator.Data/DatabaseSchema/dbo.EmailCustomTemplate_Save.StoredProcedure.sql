USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EmailCustomTemplate_Save]
	  @ID			int
	, @LoginID		int
	, @Name			varchar(100)
	, @Description	varchar(1000)
	, @Template		text
	, @IsDeleted	bit
	, @IsDefault	bit
AS
BEGIN
	SET NOCOUNT ON;

	-- Remove the IsDefault flag from all other templates for this user, if this one is set to default
	IF (@IsDefault = 1)
	BEGIN
		UPDATE	[dbo].[EmailCustomTemplate]
		SET		IsDefault = 0
		WHERE	LoginID = @LoginID
				AND IsDefault = 1
				AND ID <> @ID
	END

	IF (@ID > 0)
		BEGIN
			UPDATE [dbo].[EmailCustomTemplate]
			SET
				  LoginID = @LoginID
				, Name = @Name
				, Description = @Description
				, Template = @Template
				, IsDeleted = @IsDeleted
				, IsDefault = @IsDefault
				, ModifiedDate    = GETDATE()
			WHERE ID = @ID
			
			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO [dbo].[EmailCustomTemplate]
			(
				  LoginID
				, Name
				, Description
				, Template
				, IsDeleted
				, IsDefault
				, CreatedDate
				, ModifiedDate
			)
			VALUES
			(
				  @LoginID
				, @Name
				, @Description
				, @Template
				, @IsDeleted
				, @IsDefault
				, GETDATE()
				, GETDATE()
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
