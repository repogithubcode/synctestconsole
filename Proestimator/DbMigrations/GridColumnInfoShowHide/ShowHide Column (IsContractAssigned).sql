DECLARE @sortOrderIndex INT
DECLARE @gridControlID	VARCHAR(25)
DECLARE @columnName		VARCHAR(25)
DECLARE @headerText		VARCHAR(25)

SET @gridControlID	= 'renewal-report-grid'
SET @columnName		= 'IsContractSigned'
SET @headerText		= 'Contract Signed'

SELECT @sortOrderIndex = MAX(SortOrderIndex) FROM GridColumnInfo WHERE GridControlID = @gridControlID; 

IF NOT EXISTS (SELECT 1 FROM GridColumnInfo WHERE GridControlID = @gridControlID AND ColumnName = @columnName 
						AND HeaderText = @headerText)
BEGIN
	INSERT INTO GridColumnInfo(GridControlID, ColumnName, HeaderText, SortOrderIndex)
	VALUES(@gridControlID, @columnName, @headerText, @sortOrderIndex + 1)
END