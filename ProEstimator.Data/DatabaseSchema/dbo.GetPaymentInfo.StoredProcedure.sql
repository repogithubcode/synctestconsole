USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create PROCEDURE [dbo].[GetPaymentInfo]
	@AdminInfoID INT
AS
BEGIN
	SELECT * 
	FROM PaymentInfo 
	WHERE AdminInfoID = @AdminInfoID
	ORDER BY PaymentDate DESC
END
GO
