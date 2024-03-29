USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		<Author,,Name> 
-- Create date: <Create Date,,> 
-- Description:	<Description,,> 
-- ============================================= 
CREATE PROCEDURE [dbo].[aaaProcessUnsummarisedEstimates] 
	 
AS 
BEGIN 
	 
		-- User a cursor to loop through all of the line item IDs   
	DECLARE @cursor CURSOR	   
	SET @cursor = CURSOR FOR   
		SELECT ID 
		FROM AdminInfo 
		WHERE ID NOT IN (SELECT AdminInfoID FROM EstimateSummary) 
		ORDER BY LastView DESC
   
	DECLARE @AdminInfoID INT   
   
	OPEN @cursor   
	FETCH NEXT FROM @cursor   
	INTO @AdminInfoID   
   
	WHILE @@FETCH_STATUS = 0   
	BEGIN   
		Declare @StartTime DateTime = GetDate()
	
		EXEC Estimate_UpdateSummary @AdminInfoID = @AdminInfoID 
   
		Print 'Processed estimate ' + CAST(@AdminInfoID AS VARCHAR(20)) + ' in ' + cast(DateDiff(millisecond, @StartTime, GetDate()) as varchar) + 'ms'
   
		FETCH NEXT FROM @cursor   
		INTO @AdminInfoID   
	END    
 
END 
GO
