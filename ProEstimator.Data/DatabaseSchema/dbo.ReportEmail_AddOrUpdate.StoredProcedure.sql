USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 1/15/2019
-- Description:	Add or update Email
-- =============================================
--ReportEmail_AddOrUpdate-01.sql
CREATE PROCEDURE [dbo].[ReportEmail_AddOrUpdate]
	  @ID				int
	, @SentStamp		datetime
	, @DeleteStamp		datetime = null
	, @EstimateID		int
	, @Subject			varchar(500)
	, @Body				text
	, @ToAddresses		varchar(500)
	, @CCAddresses		varchar(500)
	, @PhoneNumbers		varchar(500)
	, @Errors			varchar(2000)
	, @EmailID			int		
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF (@ID > 0)
		BEGIN
			UPDATE ReportEmail
			SET
				  SentStamp = @SentStamp
				, DeleteStamp = @DeleteStamp
				, EstimateID = @EstimateID
				, Subject = @Subject
				, Body = @Body
				, ToAddresses = @ToAddresses
				, CCAddresses = @CCAddresses
				, PhoneNumbers = @PhoneNumbers
				, Errors = @Errors
				, EmailID = @EmailID
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO ReportEmail
			(
				  SentStamp
				, DeleteStamp
				, EstimateID
				, Subject
				, Body
				, ToAddresses
				, CCAddresses
				, PhoneNumbers
				, Errors
				, EmailID
			)
			VALUES
			(
			      @SentStamp
				, @DeleteStamp
				, @EstimateID
				, @Subject
				, @Body
				, @ToAddresses
				, @CCAddresses
				, @PhoneNumbers
				, @Errors
				, @EmailID
			)
			
			SELECT CAST(SCOPE_IDENTITY() AS INT)
		END
END
GO
