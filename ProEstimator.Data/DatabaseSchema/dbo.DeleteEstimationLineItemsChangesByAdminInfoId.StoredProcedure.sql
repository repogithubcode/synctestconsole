USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[DeleteEstimationLineItemsChangesByAdminInfoId]
	@AdminInfoId int
AS
DELETE FROM EstimationLineItemsChanges


GO
