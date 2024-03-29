USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


-- =============================================
-- Author:		Ezra
-- Create date: 8/11/2016
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[AddOrUpdateEstimationLineItem]
   @ID int
   , @EstimationDataID int
   , @StepID int
   , @PartNumber varchar(50)
   , @PartSource varchar(10)
   , @ActionCode varchar(20)
   , @Description varchar(80)
   , @Price money
   , @Qty tinyint
   , @LaborTime real
   , @PaintTime real
   , @Other real
   , @ImageID int
   , @ActionDescription varchar(80)
   , @PartOfOverhaul bit
   , @PartSourceVendorID int
   , @BettermentPartsFlag bit
   , @SubletPartsFlag bit
   , @BettermentOperationFlag bit
   , @SubletOperationFlag bit
   , @SupplementVersion tinyint
   , @LineNumber int
   , @UniqueSequenceNumber int
   , @ModifiesID int
   , @ACDCode char(1)
   , @BettermentPercentage real
   , @CustomerPrice real
   , @AutomaticCharge bit
   , @SourcePartNumber varchar(25)
   , @SectionID int
   , @VehiclePosition varchar(5)
   , @Barcode varchar(10)
   , @dbPrice money
   , @SourceVendorContactsID int
   , @AutoAdd bit
   , @Suppress bit
   , @AutoAddBarcodeParent varchar(10)
   , @Date_Entered datetime
AS
BEGIN
	IF EXISTS (SELECT * FROM EstimationLineItems WHERE id = @ID)
		BEGIN
			UPDATE [FocusWrite].[dbo].[EstimationLineItems]
				SET [EstimationDataID] = @EstimationDataID
				  ,[StepID] = @StepID
				  ,[PartNumber] = @PartNumber
				  ,[PartSource] = @PartSource
				  ,[ActionCode] = @ActionCode
				  ,[Description] = @Description
				  ,[Price] = @Price
				  ,[Qty] = @Qty
				  ,[LaborTime] = @LaborTime
				  ,[PaintTime] = @PaintTime
				  ,[Other] = @Other
				  ,[ImageID] = @ImageID
				  ,[ActionDescription] = @ActionDescription
				  ,[PartOfOverhaul] = @PartOfOverhaul
				  ,[PartSourceVendorID] = @PartSourceVendorID
				  ,[BettermentPartsFlag] = @BettermentPartsFlag
				  ,[SubletPartsFlag] = @SubletPartsFlag
				  ,[BettermentOperationFlag] = @BettermentOperationFlag
				  ,[SubletOperationFlag] = @SubletOperationFlag
				  ,[SupplementVersion] = @SupplementVersion
				  ,[LineNumber] = @LineNumber
				  ,[UniqueSequenceNumber] = @UniqueSequenceNumber
				  ,[ModifiesID] = @ModifiesID
				  ,[ACDCode] = @ACDCode
				  ,[BettermentPercentage] = @BettermentPercentage
				  ,[CustomerPrice] = @CustomerPrice
				  ,[AutomaticCharge] = @AutomaticCharge
				  ,[SourcePartNumber] = @SourcePartNumber
				  ,[SectionID] = @SectionID
				  ,[VehiclePosition] = @VehiclePosition
				  ,[Barcode] = @Barcode
				  ,[dbPrice] = @dbPrice
				  ,[SourceVendorContactsID] = @SourceVendorContactsID
				  ,[AutoAdd] = @AutoAdd
				  ,[Suppress] = @Suppress
				  ,[AutoAddBarcodeParent] = @AutoAddBarcodeParent
				  ,[Date_Entered] = @Date_Entered
			WHERE [ID] = @ID
	
			SELECT @ID
		END
	ELSE
		BEGIN
			INSERT INTO [FocusWrite].[dbo].[EstimationLineItems]
			   ([EstimationDataID]
			   ,[StepID]
			   ,[PartNumber]
			   ,[PartSource]
			   ,[ActionCode]
			   ,[Description]
			   ,[Price]
			   ,[Qty]
			   ,[LaborTime]
			   ,[PaintTime]
			   ,[Other]
			   ,[ImageID]
			   ,[ActionDescription]
			   ,[PartOfOverhaul]
			   ,[PartSourceVendorID]
			   ,[BettermentPartsFlag]
			   ,[SubletPartsFlag]
			   ,[BettermentOperationFlag]
			   ,[SubletOperationFlag]
			   ,[SupplementVersion]
			   ,[LineNumber]
			   ,[UniqueSequenceNumber]
			   ,[ModifiesID]
			   ,[ACDCode]
			   ,[BettermentPercentage]
			   ,[CustomerPrice]
			   ,[AutomaticCharge]
			   ,[SourcePartNumber]
			   ,[SectionID]
			   ,[VehiclePosition]
			   ,[Barcode]
			   ,[dbPrice]
			   ,[SourceVendorContactsID]
			   ,[AutoAdd]
			   ,[Suppress]
			   ,[AutoAddBarcodeParent]
			   ,[Date_Entered])
		 VALUES
			   (@EstimationDataID
			   ,@StepID
			   ,@PartNumber
			   ,@PartSource
			   ,@ActionCode
			   ,@Description
			   ,@Price
			   ,@Qty
			   ,@LaborTime
			   ,@PaintTime
			   ,@Other
			   ,@ImageID
			   ,@ActionDescription
			   ,@PartOfOverhaul
			   ,@PartSourceVendorID
			   ,@BettermentPartsFlag
			   ,@SubletPartsFlag
			   ,@BettermentOperationFlag
			   ,@SubletOperationFlag
			   ,@SupplementVersion
			   ,@LineNumber
			   ,@UniqueSequenceNumber
			   ,@ModifiesID
			   ,@ACDCode
			   ,@BettermentPercentage
			   ,@CustomerPrice
			   ,@AutomaticCharge
			   ,@SourcePartNumber
			   ,@SectionID
			   ,@VehiclePosition
			   ,@Barcode
			   ,@dbPrice
			   ,@SourceVendorContactsID
			   ,@AutoAdd
			   ,@Suppress
			   ,@AutoAddBarcodeParent
			   ,@Date_Entered)
			   
			SELECT SCOPE_IDENTITY()
		END
END


GO
