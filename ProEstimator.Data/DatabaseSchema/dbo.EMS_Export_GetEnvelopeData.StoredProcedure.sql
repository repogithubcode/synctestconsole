USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[EMS_Export_GetEnvelopeData] 
	@AdminInfoID Int, 
	@ExportType VarChar(25) = 'Full', 
	@EstimateMode VarChar(10) = 'Committed' 
AS 


declare @CreatorID int

select @CreatorID = CreatorID from AdminInfo as a where id = @AdminInfoID

if @CreatorID = 56027
begin
 exec [dbo].[EMS_Export_GetEnvelopeData_201] @AdminInfoID ,'Full','Committed' 
end 
else 
begin
 
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
	--NULL AS 'EST_SYSTEM', 
	--NULL AS 'SW_VERSION', 
	--NULL AS 'DB_VERSION', 
	--NULL AS 'DB_DATE', 
	--NULL AS 'UNQFILE_ID', 
	--NULL AS 'RO_ID', 
	--NULL AS 'ESTFILE_ID', 
	--NULL AS 'SUPP_NO', 
	--NULL AS 'EST_CTRY', 
	--NULL AS 'TOP_SECRET', 
	--NULL AS 'H_TRANS_ID', 
	--NULL AS 'H_CTRL_NO', 
	--NULL AS 		'TRANS_TYPE', 
	--NULL AS 	'STATUS', 
	--NULL AS 'CREATE_DT', 
	--NULL AS 'CREATE_TM',	 
	--NULL AS  TRANSMT_DT, 
	--NULL AS  TRANSMT_TM, 
	--NULL AS 		'INCL_ADMIN', 
	--NULL AS 		'INCL_VEH', 
	--NULL AS 		'INCL_EST', 
	--NULL AS 		'INCL_PROFL', 
	--NULL AS 		'INCL_TOTAL', 
	--NULL AS 		'INCL_VENDR', 
	--NULL AS 'EMS_VER'  
	CAST('O' as nvarchar(1))		'EST_SYSTEM', 
	CAST('2.6' as nvarchar(10))	'SW_VERSION', 
	CAST('FW' as nvarchar(2))		'DB_VERSION', 
	replace(left(convert(varchar, GETDATE(), 101),5), '0', '') + right(convert(varchar, GETDATE(),101), 5)	'DB_DATE', 
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
	replace(left(convert(varchar, GETDATE(), 101),5), '0', '')+right(convert(varchar, GETDATE(),101), 5) as	'CREATE_DT', 
	left(replace(CONVERT (nvarchar(10),GetDate(),108),':',''),4) as 'CREATE_TM',	 
	NULL AS TRANSMT_DT, 
	NULL AS TRANSMT_TM, 
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
 
 
 
 End
 
 
 
 
GO
