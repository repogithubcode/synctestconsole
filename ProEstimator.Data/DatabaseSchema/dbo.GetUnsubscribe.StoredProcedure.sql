USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--GetUnsubscribe-02.sql
CREATE PROCEDURE [dbo].[GetUnsubscribe]
	@Email varchar(50)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	SELECT *
	FROM UnsubscribeInfo
	WHERE Email = @Email
	Order By ID desc
END
GO
