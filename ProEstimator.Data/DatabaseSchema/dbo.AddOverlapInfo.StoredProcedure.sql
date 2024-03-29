USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO




CREATE PROCEDURE [dbo].[AddOverlapInfo]
	@EstimationLineItemsID Int,
	@VehicleID Int,
	@AdminInfoID Int,
	@SectionID Int,
	@StepsID Int
AS

CREATE TABLE dbo.#OverlapTemp (
	Amount float NULL ,
	Info varchar (100),
	VehiclePosition VarChar(5) NULL ,
	EstimationLineItemsID int NOT NULL ,
	VehicleSectionsID1 int NOT NULL ,
	VehicleSectionsID2 int NOT NULL ,
	StepListStepsID int NOT NULL ,
	EstimationLineItemsStepID int NULL ,
	EstimationLineItemsSectionID int NULL
) 

INSERT INTO #OverlapTemp
EXECUTE FocusWrite.dbo.GetOverlapAmountInfo @VehicleID,	@AdminInfoID, @SectionID, @StepsID

--SELECT @EstimationLineItemsID 'EstimationLineItemsIDNew', * 
--FROM #OverlapTemp 

INSERT INTO EstimationOverlap (EstimationLineItemsID1,EstimationLineItemsID2,OverlapAdjacentFlag,Amount)
SELECT @EstimationLineItemsID, EstimationLineItemsID, 'O', Amount
FROM #OverlapTemp


DROP TABLE #OverlapTemp




GO
