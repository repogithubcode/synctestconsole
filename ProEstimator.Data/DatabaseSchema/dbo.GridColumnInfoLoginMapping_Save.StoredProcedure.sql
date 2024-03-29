USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 10/21/2020
-- Description:	Save a GridInfoLoginMapping record
-- =============================================
CREATE PROCEDURE [dbo].[GridColumnInfoLoginMapping_Save]
	  @ID					int
	, @GridColumnInfoID		int
	, @LoginID				int
	, @Visible				bit
	, @SortOrderIndex		int	
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    IF @ID > 0
		BEGIN
			UPDATE GridColumnInfoLoginMapping
			SET
				  GridColumnInfoID = @GridColumnInfoID
				, LoginID = @LoginID
				, Visible = @Visible
				, SortOrderIndex = @SortOrderIndex
			WHERE ID = @ID

			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO GridColumnInfoLoginMapping
			(
				  GridColumnInfoID
				, LoginID
				, Visible
				, SortOrderIndex
			)
			VALUES
			(
				  @GridColumnInfoID
				, @LoginID
				, @Visible
				, @SortOrderIndex
			)

			SELECT CAST(SCOPE_IDENTITY() AS INT) 
		END
END
GO
