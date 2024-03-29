USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/16/2021
-- =============================================
CREATE PROCEDURE [dbo].[SiteUsers_Update]
	  @ID					int
	, @LoginID				int
	, @EmailAddress			varchar(50)
	, @Name					varchar(50)
	, @Password				varchar(300)
	, @IsDeleted			bit
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF @ID > 0
		BEGIN
			UPDATE SiteUsers
			SET
				  LoginID = @LoginID
				, EmailAddress = @EmailAddress
				, Name = @Name
				, Password = @Password
				, IsDeleted = @IsDeleted
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO SiteUsers
			(
				  LoginID
				, EmailAddress
				, Name
				, Password
				, IsDeleted
			)
			VALUES
			(
			      @LoginID
				, @EmailAddress
				, @Name
				, @Password
				, @IsDeleted
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
