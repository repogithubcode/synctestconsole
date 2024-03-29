USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--LoginFailures_Get-01.sql
CREATE PROCEDURE [dbo].[LoginFailures_Get]
	@Email varchar(50) = '',
	@Password varchar(50) = '',
	@StartDate varchar(50) = '',
	@EndDate varchar(50) = ''
AS 
BEGIN 
	SET NOCOUNT ON; 
 
    SELECT * 
	FROM LoginFailures 
	WHERE (isnull(@Email, '') = '' OR LoginName = @Email)
		AND (isnull(@Password, '') = '' OR [Password] = @Password)
		AND (isnull(@StartDate, '') = '' OR TimeDate > @StartDate)
		AND (isnull(@EndDate, '') = '' OR TimeDate < @EndDate)
	ORDER BY ID desc
END 
GO
