USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[sp_GetGridColumnInfo] 
AS
BEGIN
	SELECT ID
		,GridControlID
		,ColumnName
		,HeaderText
	FROM GridColumnInfo
END
GO
