USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddOrUpdateContact]    
	@ContactID int = 0,  
	@AdmininfoID int = null,  
	@FirstName nvarchar(50) = null,  
	@MiddleName nvarchar(50) = null,  
	@LastName nvarchar(50) = null,  
	@Email nvarchar(75) = null,  
	@SecondaryEmail nvarchar(75) = null,  
	@Phone1 nvarchar(20) = null,  
	@Extension1 nvarchar(10) = null,  
	@Phone2 nvarchar(20) = null,  
	@Phone3 nvarchar(20)=null,  
	@PhoneNumberType1 nchar(10)=null,  
	@PhoneNumberType2 nchar(10)=null,  
	@PhoneNumberType3 nchar(10)=null,  
	@Extension2 nvarchar(10) = null,  
	@Extension3 nvarchar(10) = null,  
	@FaxNumber nvarchar(20) = null,  
	@BusinessName nvarchar(100) = null,  
	@Notes	nvarchar(1500) = '',  
	@Title NVARCHAR(50) = '',  
	@SaveCustomer bit = 0,  
	@ContactTypeID TINYINT = 0,  
	@ContactSubTypeID SMALLINT = 0  
AS  
BEGIN  
IF EXISTS (SELECT * FROM tbl_ContactPerson with(nolock) WHERE ContactID = @ContactID)  
	BEGIN  
		UPDATE [dbo].[tbl_ContactPerson]  
		SET  [AdmininfoID] = @AdmininfoID  
		  ,[FirstName] = @FirstName  
		  ,[MiddleName] = @MiddleName  
		  ,[LastName] = @LastName  
		  ,[Email] = @Email  
		  ,[SecondaryEmail] = @SecondaryEmail  
		  ,[Phone1] = @Phone1  
		  ,[Extension1] = @Extension1  
		  ,[Phone2] = @Phone2  
		  ,[Phone3] = @Phone3  
		  ,[PhoneNumberType1] = @PhoneNumberType1  
		  ,[PhoneNumberType2] = @PhoneNumberType2  
		  ,[PhoneNumberType3] = @PhoneNumberType3  
		  ,[Extension2] = @Extension2  
		  ,[Extension3] = @Extension3  
		  ,[FaxNumber] = @FaxNumber  
		  ,[BusinessName] = @BusinessName  
		  ,[Notes] = @Notes  
		  ,[Title] = @Title  
		  ,[SaveCustomer] = @SaveCustomer  
		  ,[ContactTypeID] = @ContactTypeID  
		  ,[ContactSubTypeID] = @ContactSubTypeID  
		WHERE ContactID = @ContactID  
		  
		SELECT @ContactID		  
	END  
ELSE  
	BEGIN  
		INSERT INTO [dbo].[tbl_ContactPerson]  
           (  
			     [AdmininfoID]  
			   , [FirstName]  
			   , [MiddleName]  
			   , [LastName]  
			   , [Email]  
			   , [SecondaryEmail]  
			   , [Phone1]  
			   , [Extension1]  
			   , [Phone2]  
			   , [Phone3]  
			   , [PhoneNumberType1]   
			   , [PhoneNumberType2]   
		       , [PhoneNumberType3]  
			   , [Extension2]  
			   , [Extension3]  
			   , [FaxNumber]  
			   , [BusinessName]  
			   , [Notes]  
			   , [Title]  
			   , [SaveCustomer]  
			   , [ContactTypeID]  
			   , [ContactSubTypeID]  
			   , [DateAdded])  
		 VALUES  
			   (  
			     @AdmininfoID   
			   , @FirstName  
			   , @MiddleName  
			   , @LastName  
			   , @Email  
			   , @SecondaryEmail  
			   , @Phone1  
			   , @Extension1  
			   , @Phone2  
			   , @Phone3  
			   , @PhoneNumberType1  
			   , @PhoneNumberType2  
			   , @PhoneNumberType3  
			   , @Extension2  
			   , @Extension3  
			   , @FaxNumber  
			   , @BusinessName  
			   , @Notes  
			   , @Title  
			   , @SaveCustomer  
			   , @ContactTypeID  
			   , @ContactSubTypeID  
			   , GETDATE()  
			)  
             
		SELECT CAST(SCOPE_IDENTITY() AS INT)  
	END  
end  
GO
