USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/9/2020 
-- Description:	Add or update a User Message 
-- ============================================= 
CREATE PROCEDURE [dbo].[UserMessages_Update] 
	@ID				int, 
	@Title			varchar(100), 
	@Message		varchar(5000), 
	@StartDate		date, 
	@EndDate		date, 
	@CreatedDate	datetime, 
	@IsPermanent	bit, 
	@IsActive		bit, 
	@IsDeleted		bit 
	 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    IF @ID > 0 
		BEGIN 
			UPDATE UserMessages 
			SET 
				Title = @Title 
				, Message = @Message 
				, StartDate = @StartDate 
				, EndDate = @EndDate 
				, CreatedDate = @CreatedDate 
				, IsPermanent = @IsPermanent 
				, IsActive = @IsActive 
				, IsDeleted = @IsDeleted 
			WHERE ID = @ID 
 
			SELECT @ID 
		END 
	ELSE 
		BEGIN 
			INSERT INTO UserMessages 
			( 
				  Title 
				, Message 
				, StartDate 
				, EndDate 
				, CreatedDate 
				, IsPermanent 
				, IsActive 
				, IsDeleted 
			) 
			VALUES 
			( 
				  @Title 
				, @Message 
				, @StartDate 
				, @EndDate 
				, @CreatedDate 
				, @IsPermanent 
				, @IsActive 
				, @IsDeleted 
			) 
 
			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END 
END 
GO
