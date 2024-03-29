USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO







create PROCEDURE [dbo].[EMS_Env_201]
	@AdminInfoID Int,
	@SWVersion VarChar(10) = '2.01',
	@ExportType VarChar(25),
	@EstimateMode VarChar(10)
	
AS

DECLARE @LockLevel int
DECLARE @MaxEstimationLineItemsID Int
SELECT @LockLevel = EstimationData.LockLevel,
	@MaxEstimationLineItemsID = Max(EstimationLineItems.ID)
FROM EstimationData EstimationData WITH (NOLOCK)
INNER JOIN  EstimationLineItems EstimationLineItems WITH (NOLOCK) ON
	(EstimationLineItems.EstimationDataID = EstimationData.ID)
WHERE EstimationData.AdminInfoID = @AdminInfoID
GROUP BY EstimationData.LockLevel

SELECT
	@AdminInfoID	'AdminInfoID',
	CAST('M' as nvarchar(1))		'EST_SYSTEM',
	CAST(@SWVersion as nvarchar(10))	'SW_VERSION',
	CAST('FW' as nvarchar(2))		'DB_VERSION',
	GetDate()	'DB_DATE',
	CAST(@AdminInfoID as nvarchar(8))	'UNQFILE_ID',
	CAST('' as nvarchar(1))		'RO_ID',
	CAST(@AdminInfoID as nvarchar(8))		'ESTFILE_ID',
	isnull(EstimationData.LockLevel,0)		'SUPP_NO',
	CAST('USA' as nvarchar(3))		'EST_CTRY',
	CAST('Web-Est' as nvarchar(7))		'TOP_SECRET',
	CAST('' as nvarchar(1))		'H_TRANS_ID',
	CAST('' as nvarchar(1))		'H_CTRL_NO',
	CASE	WHEN @ExportType IN ('Full', 'Estimate') AND ISNULL(@LockLevel,0)=0 THEN CAST('E' as nvarchar(1))
		WHEN @ExportType IN ('Full', 'Estimate') AND ISNULL(@LockLevel,0)>0 THEN CAST('S' as nvarchar(1))
		WHEN @ExportType = 'Final' THEN CAST('F' as nvarchar(1))
		WHEN @ExportType = 'Assign' THEN CAST('A' as nvarchar(1))
		ELSE CAST('E' as nvarchar(1))
	END		'TRANS_TYPE',
	CASE	WHEN @EstimateMode = 'Committed' THEN 1
		ELSE 1
	END		'STATUS',
	CONVERT (nvarchar(10),GetDate(),101) as	'CREATE_DT',
	left(replace(CONVERT (nvarchar(10),GetDate(),108),':',''),4) as 'CREATE_TM',	
	1		'INCL_ADMIN',
	CASE 	WHEN  @ExportType IN ('Full', 'Estimate', 'Assign') THEN 1
		ELSE 0
	END		'INCL_VEH',
	CASE 	WHEN  @ExportType IN ('Full', 'Estimate') THEN 1
		ELSE 0
	END		'INCL_EST',
	CASE 	WHEN  @ExportType IN ('Full', 'Estimate') THEN 1
		ELSE 0
	END		'INCL_PROFL',
	CASE 	WHEN  @ExportType IN ('Full', 'Estimate') THEN 1
		ELSE 0
	END		'INCL_TOTAL',
	CASE 	WHEN  @ExportType IN ('Full', 'Estimate') THEN 1
		ELSE 0
	END		'INCL_VENDR',
	CAST('2.0' as nvarchar(3))		'EMS_VER' 
FROM EstimationData EstimationData WITH (NOLOCK)
WHERE EstimationData.AdminInfoID = @AdminInfoID








GO
