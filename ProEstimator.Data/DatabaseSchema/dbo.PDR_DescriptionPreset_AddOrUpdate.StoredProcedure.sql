USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 3/5/2018 
-- Description:	Add or update a PDR Description Preset 
-- ============================================= 
CREATE PROCEDURE [dbo].[PDR_DescriptionPreset_AddOrUpdate] 
	@ID					int, 
	@LoginID			int, 
	@Text				varchar(500) 
AS 
BEGIN 
 
	IF EXISTS (SELECT * FROM PDR_DescriptionPreset WHERE ID = @ID) 
		BEGIN 
			UPDATE [dbo].PDR_DescriptionPreset 
			SET  
			       [LoginID] = @LoginID 
				 , [Text] = @Text 
			WHERE ID = @ID  
 
			SELECT @ID	 
		END 
	ELSE 
		BEGIN 
			INSERT INTO [dbo].PDR_DescriptionPreset 
			( 
			     [LoginID] 
				,[Text] 
			) 
			VALUES 
			( 
				 @LoginID 
				,@Text 
			) 
 
			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END 
END 
GO
