USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[GetWorkOrderNumber]
	@LoginsID Int,
	@WorkOrderNumber Int = NULL OUTPUT	--A NULL is returned if an Organization has not been set up.
AS
	SET TRANSACTION ISOLATION LEVEL REPEATABLE READ	--Don't allow the record to change while getting the report number
	BEGIN TRANSACTION

	IF EXISTS (	SELECT OrganizationInfo.LastWorkOrderNumber
			FROM Logins WITH (NOLOCK)
			INNER JOIN OrganizationInfo WITH (NOLOCK) ON
				(OrganizationInfo.id = Logins.OrganizationID)
			WHERE Logins.id = @LoginsID	)
	BEGIN
		SELECT @WorkOrderNumber = OrganizationInfo.LastWorkOrderNumber
		FROM Logins WITH (NOLOCK)
		INNER JOIN OrganizationInfo WITH (NOLOCK) ON
			(OrganizationInfo.id = Logins.OrganizationID)
		WHERE Logins.id = @LoginsID
	
		SELECT @WorkOrderNumber = ISNULL(@WorkOrderNumber,0) + 1
	
		UPDATE OrganizationInfo WITH (ROWLOCK)
		SET LastWorkOrderNumber = @WorkOrderNumber
		FROM Logins WITH (NOLOCK)
		INNER JOIN OrganizationInfo WITH (ROWLOCK) ON
			(OrganizationInfo.id = Logins.OrganizationID)
		WHERE Logins.id = @LoginsID
	END

	COMMIT TRANSACTION



GO
