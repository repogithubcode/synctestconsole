USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddOrUpdateEstimateLineLabor]   
	 @id					int 
	,@EstimationLineItemsID	int 
	,@LaborType				tinyint 
	,@LaborTime				real 
	,@LaborCost				money 
	,@BettermentFlag		bit 
	,@SubletFlag			bit 
	,@UniqueSequenceNumber	int 
	,@ModifiesID			int 
	,@AdjacentDeduction		tinyint 
	,@MajorPanel			bit 
	,@BettermentPercentage	real 
	,@Lock					bit 
	,@Include				bit 
AS 
BEGIN 
IF EXISTS (SELECT * FROM EstimationLineLabor with(nolock) WHERE id = @id) 
	BEGIN 
		UPDATE [FocusWrite].[dbo].[EstimationLineLabor] 
		SET   
			[EstimationLineItemsID] = @EstimationLineItemsID 
           ,[LaborType] = @LaborType 
           ,[LaborTime] = @LaborTime 
           ,[LaborCost] = @LaborCost 
           ,[BettermentFlag] = @BettermentFlag 
           ,[SubletFlag] = @SubletFlag 
           ,[UniqueSequenceNumber] = @UniqueSequenceNumber 
           ,[ModifiesID] = @ModifiesID 
           ,[AdjacentDeduction] = @AdjacentDeduction 
           ,[MajorPanel] = @MajorPanel 
           ,[BettermentPercentage] = @BettermentPercentage 
           ,[Lock] = @Lock 
           ,[include] = @Include 
		WHERE id = @id 
		 
		SELECT @id		 
	END 
ELSE 
	BEGIN 
		INSERT INTO [FocusWrite].[dbo].[EstimationLineLabor] 
        ( 
			[EstimationLineItemsID] 
           ,[LaborType] 
           ,[LaborTime] 
           ,[LaborCost] 
           ,[BettermentFlag] 
           ,[SubletFlag] 
           ,[UniqueSequenceNumber] 
           ,[ModifiesID] 
           ,[AdjacentDeduction] 
           ,[MajorPanel] 
           ,[BettermentPercentage] 
           ,[Lock] 
           ,[include] 
		) 
		VALUES 
		( 
			@EstimationLineItemsID 
			,@LaborType 
			,@LaborTime 
			,@LaborCost 
			,@BettermentFlag 
			,@SubletFlag 
			,@UniqueSequenceNumber 
			,@ModifiesID 
			,@AdjacentDeduction 
			,@MajorPanel 
			,@BettermentPercentage 
			,@Lock 
			,@Include 
		) 
            
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	END 
end 
GO
