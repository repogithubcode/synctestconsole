USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[AddressUpdate] 
	 @AddressID			int 
	,@AdmininfoID	int = null 
	,@ContactsID	int = null 
	,@Address1		nvarchar(150) 
	,@Address2		nvarchar(150) 
	,@City			nvarchar(50) 
	,@State			nvarchar(50) 
	,@Country		nvarchar(50) 
	,@zip			nvarchar(12) 
	,@TimeZone		varchar(10) 
	  
AS 
BEGIN 
  
	SET NOCOUNT ON; 
 
 
IF @AddressID > 0 
	begin 
		UPDATE [dbo].[tbl_Address] 
		   SET [AdminInfoID] = @AdmininfoID 
			  ,[ContactsID] = @ContactsID 
			  ,[Address1] = @Address1 
			  ,[Address2] = @Address2 
			  ,[City] = @City 
			  ,[State] = @State 
			  ,[Country] = @Country 
			  ,[zip] = @zip 
			  ,[TimeZone] = @TimeZone  
		 WHERE AddressID = @AddressID 
 
		 SELECT @AddressID 
	end  
else 
	begin 
		INSERT INTO [dbo].[tbl_Address] 
           ([AdminInfoID] 
           ,[ContactsID] 
           ,[Address1] 
           ,[Address2] 
           ,[City] 
           ,[State] 
           ,[Country] 
           ,[zip] 
           ,[TimeZone]) 
		VALUES 
           (@AdmininfoID 
           ,@ContactsID 
           ,@Address1 
           ,@Address2 
           ,@City 
           ,@State 
           ,@Country 
           ,@zip 
           ,@TimeZone) 
 
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	end  
End
GO
