USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 7/18/2017 
--				10/23/2019 - Updated for big contract overhaul.  Add ons moved to their own table.  Unused columns removed. 
-- Description:	One SP to either update or insert a contract.  Returns the contractID.  This should only be used by the Contract model class.
-- Updated 10/7/2022 : AddOrUpdateContract-01.sql (for a new column AutoRenew)
-- ============================================= 
CREATE PROCEDURE [dbo].[AddOrUpdateContract]   
	@ContractID				int, 
	@LoginID				int, 
	@ContractPriceLevelID	int, 
	@EffectiveDate			date, 
	@ExpirationDate			date, 
	@IsSigned				bit, 
	@Notes					varchar(2000), 
	@PromoID				int, 
	@Active					bit, 
	@DateCreated			datetime, 
	@WillRenew				bit, 
	@WillNotRenew			bit, 
	@IsDeleted				bit,
	@AutoRenew              bit,
	@IgnoreAutoPay			bit
AS 
BEGIN 
IF @ContractID > 0 
	BEGIN 
		UPDATE [Contracts] 
		SET   
		    LoginID = @LoginID, 
			ContractPriceLevelID = @ContractPriceLevelID, 
			EffectiveDate = @EffectiveDate, 
			ExpirationDate = @ExpirationDate, 
			IsSigned = @IsSigned, 
			Notes = @Notes, 
			PromoID = @PromoID, 
			Active = @Active, 
			DateCreated = @DateCreated, 
			WillRenew = @WillRenew, 
			WillNotRenew = @WillNotRenew, 
			IsDeleted = @IsDeleted,
			AutoRenew = @AutoRenew,
			IgnoreAutoPay = @IgnoreAutoPay
		WHERE ContractID = @ContractID 
		 
		SELECT @ContractID		 
	END 
ELSE 
	BEGIN 
		INSERT INTO [Contracts] 
        ( 
			LoginID, 
			ContractPriceLevelID, 
			EffectiveDate, 
			ExpirationDate, 
			IsSigned, 
			Notes, 
			PromoID, 
			Active, 
			DateCreated, 
			WillRenew, 
			WillNotRenew, 
			IsDeleted,
			AutoRenew,
			IgnoreAutoPay
		 ) 
		 VALUES 
		 ( 
			@LoginID, 
			@ContractPriceLevelID, 
			@EffectiveDate, 
			@ExpirationDate, 
			@IsSigned, 
			@Notes, 
			@PromoID, 
			@Active, 
			@DateCreated, 
			@WillRenew, 
			@WillNotRenew, 
			@IsDeleted,
			@AutoRenew,
			@IgnoreAutoPay
		) 
            
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
	END 
end 
GO
