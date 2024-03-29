USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 5/6/2019 
-- Description:	Update an overlap line 
-- ============================================= 
CREATE PROCEDURE [dbo].[Overlaps_UpdateDetails] 
	  @id					int 
	, @OverlapAdjacentFlag		char(1) 
	, @Amount					real 
	, @Minimum					real 
	, @UserAccepted				bit 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
	UPDATE EstimationOverlap 
	SET  
		OverlapAdjacentFlag = @OverlapAdjacentFlag 
		, Amount = @Amount 
		, Minimum = @Minimum 
		, UserAccepted = @UserAccepted 
	WHERE id = @id 
END 
GO
