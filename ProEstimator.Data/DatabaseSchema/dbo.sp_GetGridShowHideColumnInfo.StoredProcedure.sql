USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- [dbo].[sp_GetGridShowHideColumnInfo] 101083, 'estimate-grid'    
CREATE PROCEDURE [dbo].[sp_GetGridShowHideColumnInfo]  
 @LoginID int ,
 @GridControlID varchar(50)
AS  
BEGIN  
	IF EXISTS (select gci.GridControlID, gci.ColumnName, gci.HeaderText, ISNULL(gcilmp.Visible, 1) AS Visible
	from GridColumnInfo gci left join GridColumnInfoLoginMapping gcilmp 
	on gci.ID = gcilmp.GridColumnInfoID
	where gcilmp.LoginID = @LoginID and gci.GridControlID = @GridControlID)
	BEGIN 
		select gci.GridControlID, gci.ColumnName, gci.HeaderText, ISNULL(gcilmp.Visible, 1) AS Visible
		from GridColumnInfo gci left join GridColumnInfoLoginMapping gcilmp 
		on gci.ID = gcilmp.GridColumnInfoID
		where gcilmp.LoginID = 101083 and gci.GridControlID = @GridControlID
	END
	ELSE
	BEGIN
		select gci.GridControlID, gci.ColumnName, gci.HeaderText, 1 AS Visible
		from GridColumnInfo gci 
		where gci.GridControlID = @GridControlID
	END
END  
  
GO
