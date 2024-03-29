USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 4/14/2020 
-- Description:	Add or update a Contract AddOn Trial 
-- ============================================= 
CREATE PROCEDURE [dbo].[ContractAddOnTrial_Update] 
	  @ID					int 
	, @ContractID			int 
	, @AddOnType			tinyint 
	, @StartDate			date 
	, @EndDate				date 
	, @IsDeleted			bit 
 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    IF @ID > 0 
		BEGIN 
			UPDATE ContractAddOnTrial	 
			SET 
				  ContractID = @ContractID 
				, AddOnType = @AddOnType 
				, StartDate = @StartDate 
				, EndDate = @EndDate 
				, IsDeleted = @IsDeleted 
			WHERE ID = @ID 
 
			SELECT @ID 
		END 
	ELSE 
		BEGIN 
			INSERT INTO ContractAddOnTrial 
			( 
				  ContractID 
				, AddOnType 
				, StartDate 
				, EndDate 
				, IsDeleted 
			) 
			VALUES 
			( 
			      @ContractID 
				, @AddOnType 
				, @StartDate 
				, @EndDate 
				, @IsDeleted 
			) 
 
			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END 
END 
GO
