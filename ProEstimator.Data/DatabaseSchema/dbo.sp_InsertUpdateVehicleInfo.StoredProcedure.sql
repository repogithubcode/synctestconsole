USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE procedure [dbo].[sp_InsertUpdateVehicleInfo] 
	@AdminInfoID int, 
	@VehicleID int = null,	  
	@MilesIn decimal(9,1) = null, 
	@MilesOut decimal(9,1) = null, 
	@License varchar(15) = null, 
	@State varchar(20) = null, 
	@Condition varchar(500) = null, 
	@ExtColor varchar(100) = null,
	@ExtColor2 varchar(100) = null,
	@IntColor varchar(100) = null, 
	@TrimLevel varchar(20) = null, 
	@Vin varchar(25) = null, 
	@VinDecode varchar(50) = null, 
	@ProductionDate varchar(15) = null, 
	@DriveType int = null, 
	@VehicleValue money = null, 
	@Year int = null, 
	@MakeID int = null, 
	@ModelID int = null, 
	@SubModelID int = null, 
	@EngineType int = null, 
	@TransmissionType int = null, 
	@paintcode nvarchar(50) = null, 
	@POI int = null, 
	@BodyType int = null, 
	@Service_Barcode varchar(10) = null, 
	@DefaultPaintType int	= 9, 
	@StockNumber varchar(50), 
	@POILabel1 tinyint = 0, 
	@POIOption1 tinyint = 0, 
	@POICustom1	varchar(500) = '', 
	@POILabel2 tinyint = 0, 
	@POIOption2 tinyint = 0, 
	@POICustom2	varchar(500) = '',
	@PaintCode2 varchar(50) = null
 as 
 begin 
 declare @vehInfoID int = 0  
 declare @EstimationId int 
  
 select @EstimationId = id  
 from EstimationData  
 where AdminInfoID = @AdminInfoID 
  
 select @vehInfoID = id  
 from VehicleInfo 
 where EstimationDataId = @EstimationId  
  
 if(@vehInfoID > 0) 
	Begin 
		update VehicleInfo 
		set		[VehicleID] = @VehicleID 
			   ,[MilesIn] = @MilesIn 
			   ,[MilesOut] = @MilesOut 
			   ,[License] = @License 
			   ,[State] = @State 
			   ,[Condition] = @Condition 
			   ,[ExtColor] = @ExtColor 
			   ,[IntColor] = @IntColor 
			   ,[TrimLevel] = @TrimLevel 
			   ,[Vin] = @Vin 
			   ,[VinDecode] = @VinDecode 
			   ,[ProductionDate] = @ProductionDate 
			   ,[BodyType] = @BodyType 
			   ,[Service_Barcode] = @Service_Barcode 
			   ,[DriveType] = @DriveType 
			   ,[VehicleValue] = @VehicleValue 
			   ,[Year] = @Year 
			   ,[MakeID] = @MakeID 
			   ,[ModelID] = @ModelID 
			   ,[SubModelID] = @SubModelID 
			   ,[EngineType] = @EngineType 
			   ,[TransmissionType] = @TransmissionType 
			   ,[paintcode] = @paintcode 
			   ,[POI_ID] = @POI 
			   ,[DefaultPaintType]= @DefaultPaintType 
			   ,[StockNumber] = @StockNumber 
			   ,[POILabel1] = @POILabel1 
			   ,[POIOption1] = @POIOption1 
			   ,[POICustom1] = @POICustom1 
			   ,[POILabel2] = @POILabel2 
			   ,[POIOption2] = @POIOption2 
			   ,[POICustom2] = @POICustom2
			   ,[ExtColor2] = @ExtColor2
			   ,[PaintCode2] = @PaintCode2
		  where EstimationDataId = @EstimationId 
 
		  SELECT @vehInfoID 
	end  
 else 
 begin 
INSERT INTO [FocusWrite].[dbo].[VehicleInfo] 
           ([EstimationDataId] 
           ,[VehicleID] 
           ,[MilesIn] 
           ,[MilesOut] 
           ,[License] 
           ,[State] 
           ,[Condition] 
           ,[ExtColor] 
           ,[IntColor] 
           ,[TrimLevel] 
           ,[Vin] 
           ,[VinDecode] 
           ,[ProductionDate] 
           ,[BodyType] 
           ,[Service_Barcode] 
           ,[DriveType] 
           ,[VehicleValue] 
           ,[Year] 
           ,[MakeID] 
           ,[ModelID] 
           ,[SubModelID] 
           ,[EngineType] 
           ,[TransmissionType] 
           ,[paintcode] 
           ,[POI_ID] 
           ,[DefaultPaintType] 
		   ,[StockNumber] 
		   ,[POILabel1] 
			,[POIOption1] 
			,[POICustom1] 
			,[POILabel2] 
			,[POIOption2] 
			,[POICustom2]
			,[ExtColor2]
			,[PaintCode2]
		) 
VALUES (@EstimationId 
		,@VehicleID 
		,@MilesIn 
		,@MilesOut 
		,@License 
		,@State 
		,@Condition 
		,@ExtColor 
		,@IntColor 
		,@TrimLevel 
		,@Vin 
		,@VinDecode 
		,@ProductionDate 
		,@BodyType 
		,@Service_Barcode 
		,@DriveType 
		,@VehicleValue 
		,@Year 
		,@MakeID 
		,@ModelID 
		,@SubModelID 
		,@EngineType 
		,@TransmissionType 
		,@paintcode 
		,@POI 
		,@DefaultPaintType 
		,@StockNumber 
		,@POILabel1 
		,@POIOption1 
		,@POICustom1 
		,@POILabel2 
		,@POIOption2 
		,@POICustom2
		,@ExtColor2
		,@PaintCode2
		) 
 
		SELECT CAST(SCOPE_IDENTITY() AS INT) 
end  
 end 
GO
