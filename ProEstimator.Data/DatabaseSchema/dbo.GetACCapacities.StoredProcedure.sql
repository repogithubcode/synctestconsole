USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

-- ============================================= 
-- Author:		Ezra 
-- Create date: 8/31/2018 
-- Description:	<Description,,> 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetACCapacities] 
	@AdminInfoID			INT 
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    DECLARE @Note VarChar(8000) = '' 
 
	DECLARE @Service_Barcode VarChar(10) = Mitchell3.Dbo.GetServiceBarcode(@AdmininfoID) 
 
	SELECT @Note = @Note + Note + '<BR>' 
	FROM 
	( 
		SELECT DISTINCT note_text 'Note' 
		FROM Mitchell3.dbo.Part_Note_Xref  
		INNER JOIN Mitchell3.dbo.Note ON Note.Service_Barcode = Part_Note_Xref.Service_Barcode AND Note.Note_Id = Part_Note_Xref.Note_id  
		WHERE 
			Part_Note_Xref.Service_Barcode = @Service_BarCode  
			AND	Part_Note_Xref.nSection = 0  
			AND	Part_Note_Xref.nPart = 0 
			AND Part_Note_Xref.Note_Type = 65 
 
		UNION 
 
		--Get Notes For Subcategory 
		SELECT	DISTINCT note_text 'note' 
		FROM Mitchell3.dbo.Part_Note_Xref  
		INNER JOIN Mitchell3.dbo.Note ON Note.Service_Barcode = Part_Note_Xref.Service_Barcode AND Note.Note_Id = Part_Note_Xref.Note_id 
		WHERE 	 
			Part_Note_Xref.Service_Barcode = @Service_BarCode  
			AND Part_Note_Xref.nSection = 0  
			AND Part_Note_Xref.nPart = 0 
			AND Part_Note_Xref.Note_Type = 65 
	) base 
	WHERE Note NOT LIKE '%procedure explanations%' 
 
	SELECT REPLACE(@note,'''','´') 
END 
GO
