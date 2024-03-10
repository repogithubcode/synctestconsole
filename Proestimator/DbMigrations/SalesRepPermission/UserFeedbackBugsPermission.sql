IF NOT EXISTS(SELECT 1 FROM [dbo].[SalesRepPermission] WHERE Tag = 'UserFeedbackBugs')
BEGIN
	INSERT INTO SalesRepPermission(PermissionGroup,Tag,[Name],[Description])
	VALUES(1,'UserFeedbackBugs','User Feedback Bugs','Gives access to the User Feedback/Bugs page.')
END