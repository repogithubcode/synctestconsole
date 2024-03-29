USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[spBuildSQLInsert]
	@TableName varchar(50)
AS

/*

This procedure will create the SQL for an insert statement for the provided table
Note: Your query editor should be switched to "Results to Text" in order to view the output correctly

Example:
spBuildSQLInsert 'tblIndAccount'

*/


SET NOCOUNT ON

DECLARE @OrdinalPosition int
SELECT @OrdinalPosition = (	SELECT Max(ORDINAL_POSITION) FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = @TableName )

-- Build top part of sql statement
SELECT		-1 AS Position, 'CREATE PROCEDURE spInsert_' + @TableName  AS SQLStatement
INTO		#TempDeclare
UNION ALL
SELECT		ORDINAL_POSITION AS Position,
			CHAR(9) + '@' + Column_Name + ' ' + DATA_TYPE +  
			( CASE WHEN DATA_TYPE IN ('char', 'nchar', 'varchar', 'nvarchar') THEN '(' + Convert(VARCHAR,CHARACTER_OCTET_LENGTH) + ')' ELSE '' END ) + 
			( CASE WHEN DATA_TYPE = 'decimal' THEN '(' + Convert(varchar, NUMERIC_PRECISION) + ',' + Convert(varchar, NUMERIC_SCALE) + ')' ELSE '' END ) + 
			( CASE WHEN ORDINAL_POSITION < @OrdinalPosition THEN ',' ELSE '' END )
FROM		INFORMATION_SCHEMA.COLUMNS
WHERE		TABLE_NAME = @TableName
AND			ORDINAL_POSITION > 1
ORDER BY	Position


-- Build bottom part of sql statement
SELECT		-1 AS Position, 'AS' + CHAR(13) + CHAR(13) + 'INSERT INTO' + CHAR(9) + @TableName + ' (' AS SQLStatement
INTO		#TempSQL1
UNION ALL
SELECT		ORDINAL_POSITION AS Position,
			CHAR(9) + 
			Column_Name +
			( CASE WHEN ORDINAL_POSITION < @OrdinalPosition THEN ',' ELSE '' END )
FROM		INFORMATION_SCHEMA.COLUMNS
WHERE		TABLE_NAME = @TableName
AND			ORDINAL_POSITION > 1
ORDER BY	Position


SELECT		-1 AS Position, 'VALUES (' AS SQLStatement
INTO		#TempSQL2
UNION ALL
SELECT		ORDINAL_POSITION AS Position,
			CHAR(9) + 
			'@' + Column_Name + 
			( CASE WHEN ORDINAL_POSITION < @OrdinalPosition THEN ',' ELSE '' END )
FROM		INFORMATION_SCHEMA.COLUMNS
WHERE		TABLE_NAME = @TableName
AND			ORDINAL_POSITION > 1
ORDER BY	Position


-- Put sql statement together
SELECT SQLStatement FROM #TempDeclare
UNION ALL
SELECT SQLStatement FROM #TempSQL1
UNION ALL
SELECT ')' AS SQLStatement
UNION ALL
SELECT SQLStatement FROM #TempSQL2
UNION ALL
SELECT ')' AS SQLStatement


GO
