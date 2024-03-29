USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 1/15/2017 
-- Description:	Add or update a PDR Estimate Data record 
-- ============================================= 
CREATE PROCEDURE [dbo].[AddOrUpdatePDREstimateData] 
	@ID					int, 
	@AdminInfoID		int, 
	@RateProfileID		int 
AS 
BEGIN 
 
	IF EXISTS (SELECT * FROM PDR_EstimateData with(nolock) WHERE ID = @ID) 
		BEGIN 
			UPDATE [dbo].[PDR_EstimateData] 
			SET  
				  [AdminInfoID] = @AdminInfoID 
			    , [RateProfileID] = @RateProfileID 
			WHERE ID = @ID  
 
			SELECT @ID	 
		END 
	ELSE 
		BEGIN 
			INSERT INTO [dbo].[PDR_EstimateData] 
			( 
				 [AdminInfoID] 
			    ,[RateProfileID] 
			) 
			VALUES 
			( 
				 @AdminInfoID 
				,@RateProfileID 
			) 
 
			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END 
END 
GO
