BEGIN TRY

	BEGIN TRAN

	DECLARE @contractSignedSortOrderIndex INT
	DECLARE @notesSortOrderIndex INT

	DECLARE @contractSignedGridColumnInfoID	 INT
	DECLARE @notesGridColumnInfoID	 INT

	SELECT @contractSignedGridColumnInfoID = ID, @contractSignedSortOrderIndex = SortOrderIndex
	FROM GridColumnInfo 
	WHERE GridControlID = 'renewal-report-grid' AND ColumnName = 'IsContractSigned' AND HeaderText = 'Contract Signed'; 

	SELECT @notesGridColumnInfoID = ID, @notesSortOrderIndex = SortOrderIndex
	FROM GridColumnInfo 
	WHERE GridControlID = 'renewal-report-grid' AND ColumnName = 'Notes' AND HeaderText = 'Notes'; 

	-- GridColumnInfo
	UPDATE GridColumnInfo SET SortOrderIndex = @contractSignedSortOrderIndex WHERE ID = @notesGridColumnInfoID
	UPDATE GridColumnInfo SET SortOrderIndex = @notesSortOrderIndex WHERE ID = @contractSignedGridColumnInfoID

	-- GridColumnInfoLoginMapping
	UPDATE GridColumnInfoLoginMapping SET SortOrderIndex = @contractSignedSortOrderIndex WHERE GridColumnInfoID = @notesGridColumnInfoID
	UPDATE GridColumnInfoLoginMapping SET SortOrderIndex = @notesSortOrderIndex WHERE GridColumnInfoID = @contractSignedGridColumnInfoID

	-- GridColumnInfoUserMapping
	UPDATE GridColumnInfoUserMapping SET SortOrderIndex = @contractSignedSortOrderIndex WHERE GridColumnInfoID = @notesGridColumnInfoID
	UPDATE GridColumnInfoUserMapping SET SortOrderIndex = @notesSortOrderIndex WHERE GridColumnInfoID = @contractSignedGridColumnInfoID

	COMMIT TRAN
END TRY
BEGIN CATCH
	ROLLBACK TRAN
END CATCH

