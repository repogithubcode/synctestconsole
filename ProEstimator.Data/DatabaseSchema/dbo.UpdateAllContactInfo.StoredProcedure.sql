USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



CREATE PROCEDURE [dbo].[UpdateAllContactInfo]
	@AdminInfoID Int,
	@TextBoxOFName VarChar(50) = '', 
	@TextBoxOLname VarChar(50) = '', 
	@TextBoxOAddLine1 VarChar(50) = '',
	@TextBoxOAddLine2 VarChar(50) = '', 
	@TextBoxOCity VarChar(50) = '', 
	@DropDownListOState VarChar(50) = '', 
	@TextBoxOZip VarChar(50) = '', 
	@DropDownListOCommCode1 VarChar(5) = '', 
	@TextboxOPhone1 VarChar(50) = '', 
	@DropDownListOCommCode2 VarChar(5) = '', 
	@TextboxOPhone2 VarChar(50) = '', 
	@DropDownListOCommCode3 VarChar(5) = '', 
	@TextboxOPhone3 VarChar(50) = '', 
	@TextBoxCFName VarChar(50) = '', 
	@TextBoxCLname VarChar(50) = '', 
	@TextBoxCAddLine1 VarChar(50) = '', 
	@TextBoxCAddLine2 VarChar(50) = '', 
	@TextBoxCCity VarChar(50) = '', 
	@DropDownListCState VarChar(50) = '', 
	@TextBoxCZip VarChar(50) = '', 
	@DropDownListCCommCode1 VarChar(5) = '', 
	@TextboxCPhone1 VarChar(50) = '', 
	@DropDownListCCommCode2 VarChar(5) = '', 
	@TextboxCPhone2 VarChar(50) = '', 
	@DropDownListCCommCode3 VarChar(5) = '', 
	@TextboxCPhone3 VarChar(50) = '', 
	@TextBoxIFName VarChar(50) = '', 
	@TextBoxILname VarChar(50) = '', 
	@TextBoxIAddLine1 VarChar(50) = '', 
	@TextBoxIAddLine2 VarChar(50) = '', 
	@TextBoxICity VarChar(50) = '', 
	@DropDownListIState VarChar(50) = '', 
	@TextBoxIZip VarChar(50) = '', 
	@DropDownListICommCode1 VarChar(5) = '', 
	@TextboxIPhone1 VarChar(50) = '', 
	@DropDownListICommCode2 VarChar(5) = '', 
	@TextboxIPhone2 VarChar(50) = '', 
	@DropDownListICommCode3 VarChar(5) = '',
	@TextboxIPhone3 VarChar(50) = '',
	@TextboxIncComp VarChar(50) = '', 
	@TextboxAgFirstName VarChar(50) = '', 
	@TextboxAgLastName VarChar(50) = '', 
	@TextboxAdjFirstName VarChar(50) = '', 
	@TextboxAdjLastName VarChar(50) = '', 
	@TextboxClFirstName VarChar(50) = '', 
	@TextboxClLastName VarChar(50) = '', 
	@TextboxIPolicyNumber VarChar(50) = '', 
	@TextboxIClaimNumber VarChar(50) = '',
	@SameAsOwnerC Bit = 0,
	@SameAsOwnerI Bit = 0
AS


IF EXISTS (	SELECT AdminInfoID
		FROM AllContactInfo WITH (NOLOCK)
		WHERE AdminInfoID = @AdminInfoID	)
BEGIN
	UPDATE AllContactInfo WITH (ROWLOCK)
	SET 	TextBoxOFName = @TextBoxOFName , 
		TextBoxOLname = @TextBoxOLname , 
		TextBoxOAddLine1 = @TextBoxOAddLine1 ,
		TextBoxOAddLine2 = @TextBoxOAddLine2 , 
		TextBoxOCity = @TextBoxOCity , 
		DropDownListOState = @DropDownListOState , 
		TextBoxOZip = @TextBoxOZip , 
		DropDownListOCommCode1 = @DropDownListOCommCode1 , 
		TextboxOPhone1 = @TextboxOPhone1 , 
		DropDownListOCommCode2 = @DropDownListOCommCode2 , 
		TextboxOPhone2 = @TextboxOPhone2 , 
		DropDownListOCommCode3 = @DropDownListOCommCode3 , 
		TextboxOPhone3 = @TextboxOPhone3 , 
		TextBoxCFName = @TextBoxCFName , 
		TextBoxCLname = @TextBoxCLname , 
		TextBoxCAddLine1 = @TextBoxCAddLine1 , 
		TextBoxCAddLine2 = @TextBoxCAddLine2 , 
		TextBoxCCity = @TextBoxCCity , 
		DropDownListCState = @DropDownListCState , 
		TextBoxCZip = @TextBoxCZip , 
		DropDownListCCommCode1 = @DropDownListCCommCode1 , 
		TextboxCPhone1 = @TextboxCPhone1 , 
		DropDownListCCommCode2 = @DropDownListCCommCode2 , 
		TextboxCPhone2 = @TextboxCPhone2 , 
		DropDownListCCommCode3 = @DropDownListCCommCode3 , 
		TextboxCPhone3 = @TextboxCPhone3 , 
		TextBoxIFName = @TextBoxIFName , 
		TextBoxILname = @TextBoxILname , 
		TextBoxIAddLine1 = @TextBoxIAddLine1 , 
		TextBoxIAddLine2 = @TextBoxIAddLine2 , 
		TextBoxICity = @TextBoxICity , 
		DropDownListIState = @DropDownListIState , 
		TextBoxIZip = @TextBoxIZip , 
		DropDownListICommCode1 = @DropDownListICommCode1 , 
		TextboxIPhone1 = @TextboxIPhone1 , 
		DropDownListICommCode2 = @DropDownListICommCode2 , 
		TextboxIPhone2 = @TextboxIPhone2 , 
		DropDownListICommCode3 = @DropDownListICommCode3,
		TextboxIPhone3 = @TextboxIPhone3 ,
		TextboxIncComp = @TextboxIncComp , 
		TextboxAgFirstName = @TextboxAgFirstName , 
		TextboxAgLastName = @TextboxAgLastName , 
		TextboxAdjFirstName = @TextboxAdjFirstName , 
		TextboxAdjLastName = @TextboxAdjLastName , 
		TextboxClFirstName = @TextboxClFirstName , 
		TextboxClLastName = @TextboxClLastName , 
		TextboxIPolicyNumber = @TextboxIPolicyNumber , 
		TextboxIClaimNumber = @TextboxIClaimNumber,
		SameAsOwnerC = @SameAsOwnerC,
		SameAsOwnerI = @SameAsOwnerI
	WHERE AdminInfoID = @AdminInfoID 
END
ELSE
BEGIN
	INSERT INTO AllContactInfo  WITH (ROWLOCK)
		VALUES ( @AdminInfoID, @TextBoxOFName, @TextBoxOLname, @TextBoxOAddLine1, @TextBoxOAddLine2, @TextBoxOCity, 
			@DropDownListOState, @TextBoxOZip, @DropDownListOCommCode1, @TextboxOPhone1, @DropDownListOCommCode2, 
			@TextboxOPhone2, @DropDownListOCommCode3, @TextboxOPhone3, @TextBoxCFName, @TextBoxCLname, 
			@TextBoxCAddLine1, @TextBoxCAddLine2, @TextBoxCCity, @DropDownListCState, @TextBoxCZip, 
			@DropDownListCCommCode1, @TextboxCPhone1, @DropDownListCCommCode2, @TextboxCPhone2, 
			@DropDownListCCommCode3, @TextboxCPhone3, @TextBoxIFName, @TextBoxILname, @TextBoxIAddLine1, 
			@TextBoxIAddLine2, @TextBoxICity, @DropDownListIState, @TextBoxIZip, @DropDownListICommCode1, 
			@TextboxIPhone1, @DropDownListICommCode2, @TextboxIPhone2, @DropDownListICommCode3, @TextboxIPhone3,
			@TextboxIncComp, @TextboxAgFirstName, @TextboxAgLastName, @TextboxAdjFirstName, @TextboxAdjLastName, 
			@TextboxClFirstName, @TextboxClLastName, @TextboxIPolicyNumber, @TextboxIClaimNumber, @SameAsOwnerC, @SameAsOwnerI )
END



GO
