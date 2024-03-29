USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
-- Stored procedure 
 
CREATE PROCEDURE [dbo].[Estimate_CreateNew] 
	  @LoginID		    int 
	, @EstimateNumber	int 
	, @ActiveLoginID	int
AS 
BEGIN 
	 
	INSERT INTO AdminInfo WITH (ROWLOCK)  
	( 
		  CreatorID 
		, Description 
		, CustomerProfilesID 
		, EstimateNumber 
	) 
	VALUES  
	( 
		  @LoginID 
		, '' 
		, 0 
		, @EstimateNumber 
	) 
	DECLARE @AdminInfoID INT = @@IDENTITY 
 	
	-- If there is only 1 estimator available, then set it to that
	-- Else if there is a Default Estimator, then use that for the new Estimate
	-- Else, set it to 0, telling the app to force the user to select 1
	DECLARE @EstimatorID INT = 0 
	IF ((SELECT Count(*) FROM EstimatorsData WHERE LoginID = @LoginID) = 1)
	BEGIN
		SET @EstimatorID = ISNULL((SELECT TOP 1 EstimatorID FROM EstimatorsData WHERE LoginID = @LoginID), 0)
	END
	ELSE IF (EXISTS(SELECT	DefaultEstimatorID 
				FROM	SiteUsers
				JOIN	ActiveLogin ON SiteUsers.ID = ActiveLogin.SiteUserID
				WHERE	ActiveLogin.ID = @ActiveLoginID AND ISNULL(DefaultEstimatorID, 0) > 0))
	BEGIN
		SELECT	@EstimatorID = DefaultEstimatorID 
		FROM	SiteUsers
		JOIN	ActiveLogin ON SiteUsers.ID = ActiveLogin.SiteUserID
		WHERE	ActiveLogin.ID = @ActiveLoginID		
	END

	INSERT INTO EstimationData WITH (ROWLOCK)  
	( 
		  AdminInfoID 
		, EstimatorID 
		, ClaimantSameAsOwner 
		, InsuredSameAsOwner 
	)  
	VALUES  
	( 
		  @AdminInfoID 
		, @EstimatorID 
		, 1 
		, 1 
	) 
	DECLARE @EstimationDataID INT = @@Identity 
	 
	INSERT INTO VehicleInfo WITH (ROWLOCK)  
	( 
		  EstimationDataID 
		, VehicleID 
		, BodyType 
		, DefaultPaintType 
	)  
	VALUES  
	( 
		  @EstimationDataID 
		, NULL 
		, NULL 
		, 19 
	) 
 
	SELECT @AdminInfoID 
END 
GO
