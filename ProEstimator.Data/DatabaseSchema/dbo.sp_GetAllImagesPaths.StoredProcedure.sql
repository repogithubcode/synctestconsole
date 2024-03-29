USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[sp_GetAllImagesPaths]   
	@AdminInfoID int,
	@ContentDirectory VARCHAR(100)='',
	@ImageSizeParameter int=null,
	@IncludeImages bit = 1,
	@Supplement INT = -1,
	@OnlyChecked BIT = 0,
	@PDFs BIT = 0
as  
begin  

IF(@IncludeImages = 1)
	BEGIN

		DECLARE @ImageSize nvarchar(100)
		SELECT @ImageSize = CASE WHEN ISNULL(@ImageSizeParameter, 0) <> 0 THEN '_' + CAST(@ImageSizeParameter AS NVARCHAR(100)) ELSE '' END

		-- Make sure the path ends with a slash
		IF NOT @ContentDirectory LIKE '%\'
			BEGIN
				SET @ContentDirectory = @ContentDirectory + '\'
			END

		-- Get the Logins ID for the Estimate
		DECLARE @LoginsID INT = (SELECT CreatorID FROM AdminInfo with(nolock) WHERE id = @AdminInfoID)

		Create TABLE #Numbers  
		(
			Number INT
		)

		DECLARE @Counter INT = 1
		WHILE @Counter < 10
			BEGIN
				INSERT INTO #Numbers (Number) VALUES (@Counter)

				SET @Counter = @Counter + 1
			END

		IF @PDFs = 0
			BEGIN
				SELECT 
					  @ContentDirectory + CAST(@LoginsID AS VARCHAR) + '\' + CAST(@AdminInfoID AS VARCHAR) + '\images\' + CAST(EstimationImages.id AS VARCHAR) + CAST(@ImageSize AS NVARCHAR(100)) + '.' + FileType AS ImagePath
					, CASE WHEN ISNULL(EstimationImagesLookup.ID, 0) > 0 THEN EstimationImagesLookup.Tag ELSE ISNULL(EstimationImages.ImageTagCustom, '') END As ImageTag
				FROM EstimationImages with(nolock)
				LEFT OUTER JOIN EstimationImagesLookup with(nolock) ON EstimationImages.ImageTag = EstimationImagesLookup.ID
				WHERE 
					AdminInfoID = @AdminInfoID
					AND ISNULL(EstimationImages.Deleted, 0) = 0
					AND (@OnlyChecked = 0 OR ISNULL(EstimationImages.Include, 0) = 1)
					AND (@Supplement = -1 OR ISNULL(EstimationImages.SupplementVersion, 0) = @Supplement)
					AND (FileType = 'jpg' OR FileType = 'jpeg' OR FileType = 'png' OR FileType = 'jfif')
				Order By OrderNo asc, EstimationImages.id desc
			END
		ELSE
			BEGIN
				SELECT 
					  @ContentDirectory + CAST(@LoginsID AS VARCHAR) + '\' + CAST(@AdminInfoID AS VARCHAR) + '\images\' + CAST(EstimationImages.id AS VARCHAR) + '_' + Cast(Numbers.Number AS VARCHAR) + CAST(@ImageSize AS NVARCHAR(100)) + '.jpg' AS ImagePath
					, CASE WHEN Numbers.Number = 1 THEN CASE WHEN ISNULL(EstimationImagesLookup.ID, 0) > 0 THEN EstimationImagesLookup.Tag ELSE ISNULL(EstimationImages.ImageTagCustom, '') END ELSE '' END As ImageTag
				FROM EstimationImages with(nolock)
				LEFT OUTER JOIN EstimationImagesLookup with(nolock) ON EstimationImages.ImageTag = EstimationImagesLookup.ID
				LEFT OUTER JOIN #Numbers Numbers ON EstimationImages.PdfPageCount >= Numbers.Number
				WHERE 
					AdminInfoID = @AdminInfoID
					AND ISNULL(EstimationImages.Deleted, 0) = 0
					AND (@OnlyChecked = 0 OR ISNULL(EstimationImages.Include, 0) = 1)
					AND (@Supplement = -1 OR ISNULL(EstimationImages.SupplementVersion, 0) = @Supplement)
					AND FileType = 'pdf'
				Order By OrderNo asc, EstimationImages.id desc, Numbers.Number asc
			END
	END
END
GO
