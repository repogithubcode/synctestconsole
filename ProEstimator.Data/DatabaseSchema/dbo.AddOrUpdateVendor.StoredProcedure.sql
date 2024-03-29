USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddOrUpdateVendor] 
	@ID int, 
	@LoginsID int, 
    @CompanyName varchar(200), 
    @FirstName varchar(50), 
    @MiddleName varchar(50), 
    @LastName varchar(50), 
    @Email varchar(100), 
    @MobileNumber varchar(20), 
    @HomeNumber varchar(20), 
    @WorkNumber varchar(20), 
    @Address1 varchar(100), 
    @Address2 varchar(100), 
    @City varchar(50), 
    @State varchar(25), 
    @Zip varchar(15), 
    @TimeZone varchar(50), 
    @Universal bit, 
    @TypeID tinyint, 
    @FaxNumber varchar(50), 
    @FileName varchar(50), 
	@Deleted bit = false, 
	@Extension varchar(10), 
	@FederalTaxID varchar(50), 
	@LicenseNumber varchar(50), 
	@BarNumber varchar(50), 
	@RegistrationNumber varchar(50) 
AS 
BEGIN 
	IF EXISTS (SELECT * FROM Vendor WHERE ID = @id) 
	BEGIN 
		UPDATE Vendor 
		SET  
			 [LoginsID] = @LoginsID 
			,[CompanyName] = @CompanyName 
			,[FirstName] = @FirstName 
			,[MiddleName] = @MiddleName 
			,[LastName] = @LastName 
			,[Email] = @Email 
			,[MobileNumber] = @MobileNumber 
			,[HomeNumber] = @HomeNumber 
			,[WorkNumber] = @WorkNumber 
			,[Address1] = @Address1 
			,[Address2] = @Address2 
			,[City] = @City 
			,[State] = @State 
			,[Zip] = @Zip 
			,[TimeZone] = @TimeZone 
			,[Universal] = @Universal 
			,[TypeID] = @TypeID 
			,[FaxNumber] = @FaxNumber 
			,[FileName] = @FileName 
			,[Deleted] = @Deleted 
			,[Extension]=@Extension 
			,[FederalTaxID]=@FederalTaxID
			,[LicenseNumber]=@LicenseNumber
			,[BarNumber]=@BarNumber
			,[RegistrationNumber]=@RegistrationNumber
		WHERE ID = @ID 
 
		SELECT @ID		 
	END 
ELSE 
	BEGIN 
		INSERT INTO Vendor 
        ( 
			[LoginsID] 
           ,[CompanyName] 
           ,[FirstName] 
           ,[MiddleName] 
           ,[LastName] 
           ,[Email] 
           ,[MobileNumber] 
           ,[HomeNumber] 
           ,[WorkNumber] 
           ,[Address1] 
           ,[Address2] 
           ,[City] 
           ,[State] 
           ,[Zip] 
           ,[TimeZone] 
           ,[Universal] 
           ,[TypeID] 
           ,[FaxNumber] 
           ,[FileName] 
		   ,[Deleted] 
		   ,[Extension] 
		   ,[FederalTaxID]
		   ,[LicenseNumber]
		   ,[BarNumber]
		   ,[RegistrationNumber]
		) 
		VALUES 
		( 
			@LoginsID, 
			@CompanyName, 
			@FirstName, 
			@MiddleName, 
			@LastName, 
			@Email, 
			@MobileNumber, 
			@HomeNumber, 
			@WorkNumber, 
			@Address1, 
			@Address2, 
			@City, 
			@State, 
			@Zip, 
			@TimeZone, 
			@Universal, 
			@TypeID,
			@FaxNumber, 
			@FileName, 
			@Deleted, 
			@Extension,
			@FederalTaxID,
			@LicenseNumber,
			@BarNumber,
			@RegistrationNumber
		) 
            
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	END 
END 
 
GO
