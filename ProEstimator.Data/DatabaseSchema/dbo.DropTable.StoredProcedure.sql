USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE PROCEDURE [dbo].[DropTable]
	@TableName VarChar(100)
 AS
IF exists (SELECT * FROM sysobjects WHERE id = object_id(@TableName) and OBJECTPROPERTY(id, N'IsUserTable') = 1)
	EXECUTE ('DROP TABLE ' + @TableName)


GO
