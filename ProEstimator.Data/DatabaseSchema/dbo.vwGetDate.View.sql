USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE VIEW [dbo].[vwGetDate]
AS

/*
-- Returns value of getDate()
-- Used in functions, as they do not allow you to call an actual function

SELECT GetDate FROM vwGetDate

*/

SELECT getDate() AS GetDate

GO
