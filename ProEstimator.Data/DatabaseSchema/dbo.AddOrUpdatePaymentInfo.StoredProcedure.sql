USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


CREATE procedure [dbo].[AddOrUpdatePaymentInfo]    
	@PaymentId int,
	@AdminInfoID int,  
	@ContactsID int,  
	@PayeeName varchar(50),  
	@PaymentType varchar(15),  
	@PaymentDate datetime,  
	@CheckNumber varchar(10),  
	@Amount money,  
	@Memo text,  
	@WhoPays varchar(15)  
as  
begin  
IF EXISTS (SELECT * FROM PaymentInfo with(nolock) WHERE PaymentId = @PaymentID)
	BEGIN
		UPDATE PaymentInfo
		SET
			AdminInfoID = @AdminInfoID,  
			ContactsID = @ContactsID,   
			PayeeName = @PayeeName,  
			PaymentType = @PaymentType,  
			PaymentDate = @PaymentDate,  
			CheckNumber = @CheckNumber,  
			Amount = @Amount,  
			Memo = @Memo,  
			WhoPays = @WhoPays  
		WHERE PaymentId = @PaymentId
		
		SELECT @PaymentId		
	END
ELSE
	BEGIN
		insert into PaymentInfo   
		values( 
			@AdminInfoID,  
			@ContactsID,   
			@PayeeName ,  
			@PaymentType,  
			@PaymentDate ,  
			@CheckNumber ,  
			@Amount,  
			@Memo,  
			@WhoPays   
		)  

		SELECT SCOPE_IDENTITY()
	END
end

GO
