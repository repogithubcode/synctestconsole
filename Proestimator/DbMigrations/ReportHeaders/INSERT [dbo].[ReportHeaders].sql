IF NOT EXISTS (SELECT 1 FROM ReportHeaders WHERE Header = 'Final Report')
BEGIN
	DECLARE @maxSortOrder INT

	SELECT @maxSortOrder = MAX(SortOrder) FROM ReportHeaders

	INSERT INTO ReportHeaders(Header, SortOrder, ReportType)
	VALUES('Final Report', @maxSortOrder + 1, 'Estimate')
END