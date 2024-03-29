USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 2/4/2020
-- Description:	Log a feature hit to be sent to SuccessBox
-- =============================================
--SuccessBox_LogFeature-01.sql
CREATE PROCEDURE [dbo].[SuccessBox_LogFeature]
	  @LoginID				int
	, @ModuleID				tinyint
	, @Feature				varchar(50)
	, @ActiveLoginID		int
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	INSERT INTO SuccessBoxFeatureLog
	(
			LoginID
		, TimeStamp
		, ModuleID
		, Feature
		, ActiveLoginID
	)
	VALUES
	(
			@LoginID
		, GETDATE()
		, @ModuleID
		, @Feature
		, @ActiveLoginID
	)
END
GO
