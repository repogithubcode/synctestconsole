USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
 
CREATE PROCEDURE [dbo].[GetPartsListByPartNumber] 
	@PartNumber VarChar(25), 
	@LoginsID Int, 
	@SourcePartNumber VarChar(25) = '', 
	@PartSourceVendorID Int = 0, 
	@SelectedIndex Int = NULL OUTPUT 
AS 
 
SELECT @PartNumber = LTRIM(RTRIM(REPLACE(@PartNumber,'GM PART',''))) 
 
SELECT	Convert(VarChar(30),0) 'id', CONVERT(VarChar(500),'Select a part, or enter info below') 'PartInfo', 
	CONVERT(VarChar(25),'') 'SourcePartNumber', 
	CONVERT(Int,0) 'PartSourceVendorID', 
	IDENTITY(Int, 0, 1) 'Id2' 
INTO #Temp 
 
 
SELECT @SelectedIndex = NULL 
SELECT @SelectedIndex = Id2 
FROM #Temp 
WHERE SourcePartNumber = @SourcePartNumber AND 
	PartSourceVendorID = @PartSourceVendorID 
SELECT @SelectedIndex = ISNULL(@SelectedIndex,-1) 
 
SELECT id, Partinfo 
FROM #Temp 
ORDER BY ID DESC 
 
 
GO
