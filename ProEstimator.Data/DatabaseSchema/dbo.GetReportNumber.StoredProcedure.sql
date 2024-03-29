USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[GetReportNumber]
	@AdminInfoID Int,
	@ReportNumber Int = NULL OUTPUT		--A NULL is returned if an Organization has not been set up.
AS
	SET TRANSACTION ISOLATION LEVEL REPEATABLE READ	--Don't allow the record to change while getting the report number
	BEGIN TRANSACTION

/*	IF (	SELECT EstimationData.AlternateIdentitiesID
		FROM AdminInfo
		LEFT JOIN EstimationData ON
			(EstimationData.AdminInfoID = AdminInfo.id)
		LEFT JOIN AlternateIdentities ON
			(AlternateIdentities.id = EstimationData.AlternateIdentitiesID)
		WHERE AdminInfo.id = @AdminInfoID	) IS NULL
	BEGIN */
		-- Use Organization Info to get numbers
	IF EXISTS (	SELECT OrganizationInfo.LastReportNumber
			FROM AdminInfo with(nolock)
			INNER JOIN Logins WITH (NOLOCK) ON
				(Logins.id = AdminInfo.CreatorID)
			INNER JOIN OrganizationInfo WITH (NOLOCK) ON
				(OrganizationInfo.id = Logins.OrganizationID)
			WHERE AdminInfo.id = @AdminInfoID	)
	BEGIN		
		SELECT @ReportNumber = NULL
		SELECT @ReportNumber = OrganizationInfo.LastReportNumber
		FROM AdminInfo WITH (NOLOCK)
		INNER JOIN Logins WITH (NOLOCK) ON
			(Logins.ID = AdminInfo.CreatorID)
		INNER JOIN OrganizationInfo WITH (NOLOCK) ON
			(OrganizationInfo.id = Logins.OrganizationID)
		WHERE AdminInfo.id = @AdminInfoID
	
		IF @ReportNumber IS NOT NULL 
		BEGIN
			SELECT @ReportNumber = ISNULL(@ReportNumber,0) + 1
	
			UPDATE OrganizationInfo WITH (ROWLOCK)
			SET LastReportNumber = @ReportNumber
			FROM AdminInfo WITH (NOLOCK)
			INNER JOIN Logins WITH (NOLOCK) ON
				(Logins.ID = AdminInfo.CreatorID)
			INNER JOIN OrganizationInfo WITH (ROWLOCK) ON
				(OrganizationInfo.id = Logins.OrganizationID)
			WHERE AdminInfo.id = @AdminInfoID
		END
	END
/*	END
	ELSE
	BEGIN
		--Use alternate identity to get numbers
		SELECT @ReportNumber = AlternateIdentities.LastReportNumber
		FROM AdminInfo
		INNER JOIN EstimationData ON
			(EstimationData.AdminInfoID = AdminInfo.id)
		INNER JOIN AlternateIdentities ON
			(AlternateIdentities.id = EstimationData.AlternateIdentitiesID)
		WHERE AdminInfo.id = @AdminInfoID	
	
		IF @ReportNumber IS NOT NULL 
		BEGIN
			SELECT @ReportNumber = @ReportNumber + 1
	
			UPDATE AlternateIdentities
			SET LastReportNumber = @ReportNumber
			FROM AdminInfo
			INNER JOIN EstimationData ON
				(EstimationData.AdminInfoID = AdminInfo.id)
			INNER JOIN AlternateIdentities ON
				(AlternateIdentities.id = EstimationData.AlternateIdentitiesID)
			WHERE AdminInfo.id = @AdminInfoID
		END
	END*/

	COMMIT TRANSACTION



GO
