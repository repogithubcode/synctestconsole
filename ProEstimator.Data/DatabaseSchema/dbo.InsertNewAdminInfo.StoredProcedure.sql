USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO


CREATE PROCEDURE [dbo].[InsertNewAdminInfo]
	@id int
	,@creatorid int
	,@description varchar(255)
	,@customerprofilesid int = NULL
	,@GrandTotal varchar(20) = NULL
	,@bettermenttotal varchar(20)  = NULL
	,@estimatenumber int  = NULL
	,@workordernumber int  = NULL
	,@printdescription bit = NULL
	,@archived bit = NULL
AS
if @id > 0
begin
	if exists (select id from admininfo with(nolock) where id = @id)
	begin
		raiserror('You cannot overwrite the admininfo %d',18,1,@id)
	end
	else
	begin
		set identity_insert AdminInfo ON
		INSERT INTO AdminInfo(
			[id], 
			[CreatorID], 
			[Description], 
			[CustomerProfilesID], 
			[GrandTotal], 
			[BettermentTotal], 
			[EstimateNumber], 
			[WorkOrderNumber], 
			[PrintDescription], 
			[Archived])
		VALUES(
			@id 
			,@creatorid 
			,@description 
			,@customerprofilesid 
			,@GrandTotal 
			,@bettermenttotal
			,@estimatenumber 
			,@workordernumber 
			,@printdescription 
			,@archived)
		set identity_insert AdminInfo OFF
	end
end
else if @id = 0
begin
	raiserror('You must specifiy either a valid admininfo id or -1',18,1)
end
else
begin
	INSERT INTO AdminInfo(		
		[CreatorID], 
		[Description], 
		[CustomerProfilesID], 
		[GrandTotal], 
		[BettermentTotal], 
		[EstimateNumber], 
		[WorkOrderNumber], 
		[PrintDescription], 
		[Archived])
	VALUES(		
		@creatorid 
		,@description 
		,@customerprofilesid 
		,@GrandTotal 
		,@bettermenttotal
		,@estimatenumber 
		,@workordernumber 
		,@printdescription 
		,@archived)
	
end
if @@error = 0 begin select @@identity end
else begin return 0 end


GO
