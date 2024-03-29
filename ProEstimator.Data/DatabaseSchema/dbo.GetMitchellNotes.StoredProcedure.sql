USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- ============================================= 
-- Author:		Ezra 
-- Create date: 9/6/2019 
-- Description:	Get notes for a vehicle by note ID 
--	 
--				Known Note IDs: 
--					185 paint code location 
--					228 Clear Coat Identificationx 
-- ============================================= 
CREATE PROCEDURE [dbo].[GetMitchellNotes] 
	@AdminInfoID			int, 
	@NoteID					int 
AS 
BEGIN 
	 
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
			AND Part_Note_Xref.Note_id = @NoteID 
	) base 
 
	SELECT REPLACE(@note,'''','´') 
 
END 
GO
