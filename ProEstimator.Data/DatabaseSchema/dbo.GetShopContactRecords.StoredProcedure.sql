USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[GetShopContactRecords]

AS

SELECT 	[name], 
	[address],	
	[city], 
	[state], 
	[zip], 
	[phone] 
FROM [BodyShopContacts].[dbo].[Shop] with(nolock)



GO
