USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO
 
create PROCEDURE [dbo].[SelectAnnouncements] (@ID int) --  [InsertAnnouncements] @Name='test',@LoginID=10000,@ImagePaths='test2|test3' 
	  as 
	  begin 
	  select * from Announcement where @ID=ID  
	  end 
GO
