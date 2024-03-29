USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[UpdateSalesRep]
	@SalesRepID int,
	@SalesNumber varchar(50),
	@FirstName varchar(50),
	@LastName varchar(50),
	@Email varchar(50),
	@Password varchar(50)
AS

-- Open encryption key		
OPEN SYMMETRIC KEY WebEstEncryptionKey
   DECRYPTION BY CERTIFICATE WebEstEncryptionCertificate
   

UPDATE	SalesRep
SET		SalesNumber = @SalesNumber,
		FirstName = @FirstName,
		LastName = @LastName,
		Email = @Email,
		UserName = Lower(Left(@FirstName, 1) + @LastName),
		Password = EncryptByKey(Key_GUID('WebEstEncryptionKey'), @Password)
WHERE	SalesRepID = @SalesRepID


GO
