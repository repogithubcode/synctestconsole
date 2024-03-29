USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/2/2017 
-- Description:	Add or update an SentSMS_Status record 
-- ============================================= 
CREATE PROCEDURE [dbo].[AddOrUpdateSendSMSStatus] 
	@ID			int, 
	@ReportId				int,
	@Status					varchar(500),
	@SMSResponseResource	varchar(5000),
	@ErrorMessage			varchar(500),
	@SMSId					varchar(500)
	
 
AS 
BEGIN 
 
IF EXISTS (SELECT * FROM SentSMS_Status with(nolock)  WHERE ID = @ID) 
	BEGIN 
		UPDATE [FocusWrite].[dbo].[SentSMS_Status] 
		SET   
			ReportId = @ReportId 
			, [Status] = @Status 
			, SMSResponseResource = @SMSResponseResource 
			, SMSId = @SMSId 
			, ErrorMessage = @ErrorMessage 
		WHERE ID = @ID 
		 
		SELECT @ID		 
	END 
ELSE 
	BEGIN 
		INSERT INTO [FocusWrite].[dbo].[SentSMS_Status] 
        ( 
			ReportId  
			, [Status] 
			, SMSResponseResource 
			, SMSId  
			, ErrorMessage 
		 ) 
		 VALUES 
		 ( 
			@ReportId 
			,@Status 
			,@SMSResponseResource 
			,@SMSId 
			,@ErrorMessage 
		) 
            
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	END 
 
END 
GO
