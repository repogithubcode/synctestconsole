USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [Admin].[GET_SmsHistory]
AS
BEGIN
	SELECT h.*, s.UserName
	FROM SmsHistory h with(nolock)
	INNER JOIN SalesRep s with(nolock)
	ON s.SalesRepID = h.SalesRepId

END
GO
