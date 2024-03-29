USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[Estimator_Save] 
	@EstimatorID			INT, 
	@EstimateID				INT, 
	@AuthorFirstName				VARCHAR(50), 
	@AuthorLastName				VARCHAR(50), 
	@LoginID				INT, 
	@OrderNo				INT,
	@Email					NVARCHAR(150), 
	@Phone					NVARCHAR(100),
	@ActiveLoginID			INT,
	@SetAsDefaultEstimator	BIT
AS 
BEGIN 

	DECLARE @SiteUserID INT = 0
	SELECT	@SiteUserID = ISNULL(SiteUserID, 0) 
	FROM	ActiveLogin 
	WHERE	ID = ISNULL(@ActiveLoginID, -1)

	IF (@EstimatorID > 0 ) 
	BEGIN 
		UPDATE EstimatorsData 
		SET  
				AdminInfoID = @EstimateID 
			, AuthorFirstName = @AuthorFirstName 
			, AuthorLastName = @AuthorLastName 
			, LoginID = @LoginID 
			, OrderNo = @orderNo 
			, Email = @Email 
			, Phone = @Phone 
			WHERE EstimatorID = @EstimatorID 
 	END 
	ELSE 
	BEGIN
		INSERT INTO EstimatorsData 
		( 
				AdminInfoID 
			, AuthorFirstName 
			, AuthorLastName 
			, LoginID 
			, OrderNo 
			, Email
			, Phone
		) 
		VALUES 
		( 
				@EstimateID 
			, @AuthorFirstName 
			, @AuthorLastName 
			, @LoginID 
			, @OrderNo 
			, @Email
			, @Phone
		) 
 
		SET @EstimatorID = CAST(SCOPE_IDENTITY() AS INT) 
	END

	IF (@SiteUserID > 0)
	BEGIN
		DECLARE @CurrentDefaultEstimatorID INT = 0
		SELECT	@CurrentDefaultEstimatorID = ISNULL(DefaultEstimatorID, 0)
		FROM	SiteUsers
		WHERE	ID = @SiteUserID

		IF (@EstimatorID = @CurrentDefaultEstimatorID AND @SetAsDefaultEstimator = 0)
		BEGIN
			-- User deselected this Estimator as their default
			UPDATE	SiteUsers
			SET		DefaultEstimatorID = NULL
			WHERE	ID = @SiteUserID
		END
		ELSE IF (@EstimatorID <> @CurrentDefaultEstimatorID AND @SetAsDefaultEstimator = 1)
		BEGIN
			-- User selected this Estimator as their default
			UPDATE	SiteUsers
			SET		DefaultEstimatorID = @EstimatorID
			WHERE	ID = @SiteUserID
		END
	END

	SELECT @EstimatorID 

END 
GO
