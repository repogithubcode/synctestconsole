USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsuranceCompany_GetAll]
	  @LoginID			int
	, @ShowDeleted		bit = 0
AS 
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT *
	FROM InsuranceCompany
	WHERE LoginID = @LoginID AND (IsDeleted = ISNULL(@ShowDeleted,IsDeleted))
	ORDER BY Name

END
GO
