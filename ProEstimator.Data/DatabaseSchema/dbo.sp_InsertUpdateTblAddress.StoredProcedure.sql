USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

 
CREATE PROCEDURE [dbo].[sp_InsertUpdateTblAddress]
	 @AdmininfoID int = null
	,@ContactsID int = null
	,@Address1 nvarchar(150)
	,@Address2 nvarchar(150)
	,@City nvarchar(50)
	,@State nvarchar(50)
	,@Country nvarchar(50)
	,@zip nvarchar(12)
	,@TimeZone varchar(10)
	 
AS
BEGIN
 
	SET NOCOUNT ON;


IF EXISTS (SELECT * FROM tbl_Address WHERE ContactsID = @ContactsID)
	begin
		UPDATE [FocusWrite].[dbo].[tbl_Address]
		   SET [AdminInfoID] = @AdmininfoID
			  ,[Address1] = @Address1
			  ,[Address2] = @Address2
			  ,[City] = @City
			  ,[State] = @State
			  ,[Country] = @Country
			  ,[zip] = @zip
			  ,[TimeZone] = @TimeZone 
		 WHERE ContactsID = @ContactsID
	end 
else
	begin
		INSERT INTO [FocusWrite].[dbo].[tbl_Address]
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
	end 
End
GO
