USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[UnsubscribeHistory_Save] 
	@ID int,
	@UnsubscribeID int,
	@TimeStamp DateTime,
	@Event varchar(20),
	@SalesRep varchar(50)
AS 
BEGIN 
	IF (@ID > 0 ) 
		BEGIN 
			UPDATE UnsubscribeHistory
			SET
				UnsubscribeID = @UnsubscribeID,
				TimeStamp = @TimeStamp,
				Event = @Event,
				SalesRep = @SalesRep
			WHERE ID = @ID
 
			SELECT @ID
		END 
	ELSE 
		INSERT INTO UnsubscribeHistory
		(
			UnsubscribeID,
			TimeStamp,
			Event,
			SalesRep
		) 
		VALUES
		(
			@UnsubscribeID
			, @TimeStamp
			, @Event
			, @SalesRep
		)
 
		SELECT CAST(SCOPE_IDENTITY() AS INT)
END
GO
