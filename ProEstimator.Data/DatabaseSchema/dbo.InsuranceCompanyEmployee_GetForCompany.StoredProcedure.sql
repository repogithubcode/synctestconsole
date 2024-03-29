USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[InsuranceCompanyEmployee_GetForCompany]
	    @CompanyID			int 
	  , @EmployeeType		tinyint 
	  , @IsDeleted			BIT	= 0 
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

    SELECT * 
	FROM InsuranceCompanyEmployee
	WHERE InsuranceCompanyID = @CompanyID AND EmployeeType = @EmployeeType AND 
	IsDeleted = ISNULL(@IsDeleted,IsDeleted)
	ORDER BY FirstName
		
END	
GO
