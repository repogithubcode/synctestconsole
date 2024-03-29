USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. GIBSON
-- Create date: 6/2/2019
-- Description:	INSERT DISPLAY SETTINGS
-- =============================================
CREATE PROCEDURE [Admin].[InsertRenewalReportSettings]
	@SalesRepId INT = NULL
    ,@SettingsKey NVARCHAR(1000) = NULL
AS
BEGIN
INSERT INTO [dbo].[RenewalReportSettings]
           ([SalesRepId]
           ,[SettingsKey]
           ,[CreateDate]
           ,[ModifiedDate])
     VALUES
           (@SalesRepId
           ,@SettingsKey
           ,GETDATE()
           ,GETDATE())
END
GO
