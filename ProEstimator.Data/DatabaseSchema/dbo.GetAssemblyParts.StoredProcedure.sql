USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--GetAssemblyParts-01.sql

CREATE PROCEDURE [dbo].[GetAssemblyParts]    
	@ServiceBarCode varchar(6),
	@nHeader Int,
	@nSection Int,
	@BarCode varchar(6) = null
AS
Begin
	SET NOCOUNT ON

SELECT
	   DetailAssembly.barcode AS AssemblyBarCode
     , DetailAssembly.Part_Number AS AssemblyPartNumber
     , DetailAssembly.Prtc_Description AS AssemblyPartDescription
     , Detail.barcode AS PartBarcode
     , Detail.Part_Number AS PartNumber
     , Detail.Prtc_Description AS PartDescription
FROM Mitchell3.Dbo.Matrix with(nolock)
INNER JOIN Mitchell3.Dbo.Detail with(nolock) on 
	Detail.Service_Barcode = Matrix.Service_Barcode
	AND Detail.nheader = @nHeader
	AND Detail.nsection = @nSection
    AND Matrix.Parent_Barcode = Detail.barcode
INNER JOIN Mitchell3.Dbo.Detail As DetailAssembly with(nolock) on 
	DetailAssembly.Service_Barcode = Matrix.Service_Barcode
	AND DetailAssembly.nheader = @nHeader
	AND DetailAssembly.nsection = @nSection
    AND Matrix.Child_Barcode = DetailAssembly.barcode
WHERE 
	Matrix.Service_Barcode = @ServiceBarCode 
	AND Relationship = 2 
	AND 
	(
		isnull(@BarCode, '') = '' 
		OR
		Child_Barcode = @BarCode
	)
ORDER BY DetailAssembly.barcode, Detail.barcode

End
GO
