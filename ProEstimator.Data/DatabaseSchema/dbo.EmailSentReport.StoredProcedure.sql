USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[EmailSentReport]
		@LoginID		INT  = NULL, 
		@DateStart		DATETIME  = NULL, 
		@DateEnd		DATETIME  = NULL, 
		@EmailAddress	VARCHAR(1000) = NULL,
		@EmailSubject	VARCHAR(500) = NULL,
		@EmailBody		VARCHAR(MAX) = NULL,
		@HasError		BIT = NULL,
		@ErrorMessage	VARCHAR(MAX) = NULL
AS
BEGIN

	SELECT Emails.ID, LoginID, ToAddresses, CCAddresses, SMSNumbers, ReplyTo, Subject, Body, AttachmentPaths, CreateStamp 
	, SendGridInfo.TimeStamp As SendStamp
	, case when SendGridInfo.Event is null then Emails.HasError else case when SendGridInfo.Event = 'delivered' then 0 else 1 end end As HasError
	, case when SendGridInfo.Event is null then Emails.ErrorMessage else case when SendGridInfo.Event = 'delivered' then '' else SendGridInfo.Event + ' ' + isnull(SendGridInfo.Reason, '') end end As ErrorMessage
	, isnull(SendGridInfo.Email, case when isnull(Emails.HasError, 0) = 0 then 'Pending' else 'N/A' end) As Recipient
	, TemplateID
	, '' As CompanyName
	FROM dbo.[Emails] with(nolock)
	LEFT OUTER JOIN SendGridInfo with(nolock) on Emails.ID = SendGridInfo.EmailID and [Event] <> 'processed'
	LEFT OUTER JOIN SendGridInfo b with(nolock) on SendGridInfo.[EmailID] = b.EmailID and SendGridInfo.Email = b.Email and SendGridInfo.ID <> b.ID and b.[Event] = 'delivered'
	WHERE
		b.ID is null
		AND Emails.LoginID = ISNULL(@LoginID, Emails.LoginID)
		AND (CreateStamp >= ISNULL(@DateStart, CreateStamp) AND CreateStamp <= ISNULL(@DateEnd, CreateStamp))
		AND (ToAddresses LIKE '%' + (CASE WHEN @EmailAddress IS NULL THEN ToAddresses ELSE @EmailAddress END) + '%'
			 OR CCAddresses LIKE '%' + (CASE WHEN @EmailAddress IS NULL THEN CCAddresses ELSE @EmailAddress END) + '%'
			 OR SMSNumbers LIKE '%' + (CASE WHEN @EmailAddress IS NULL THEN SMSNumbers ELSE @EmailAddress END) + '%')
		AND ([Subject] LIKE '%' + (CASE WHEN @EmailSubject IS NULL THEN [Subject] ELSE @EmailSubject END) + '%')	
		AND (CAST(Body AS VARCHAR(MAX)) LIKE '%' + (CASE WHEN @EmailBody IS NULL THEN CAST(Body AS VARCHAR(MAX)) ELSE @EmailBody END) + '%')	
		AND (@HasError is null or case when SendGridInfo.Event is null then Emails.HasError else case when SendGridInfo.Event = 'delivered' then 0 else 1 end end = @HasError)
		AND (@ErrorMessage is null or CAST(ErrorMessage AS VARCHAR(MAX)) LIKE '%' + @ErrorMessage + '%' or SendGridInfo.Event like '%' + @ErrorMessage + '%' or SendGridInfo.Reason like '%' + @ErrorMessage + '%')
END
GO
