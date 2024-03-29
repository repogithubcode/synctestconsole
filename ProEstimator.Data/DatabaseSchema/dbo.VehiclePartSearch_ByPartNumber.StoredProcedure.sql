USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[VehiclePartSearch_ByPartNumber]
	@PartNumber			varchar(20)
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	Create TABLE #TableVehicleByPartNumber 
	( 
		Row_Num INT,
		Service_BarCode VARCHAR(500),
		Category VARCHAR(500),
		Subcategory VARCHAR(500),
		Part_Desc VARCHAR(500),
		Part_Number VARCHAR(500),
		Prtc_Description VARCHAR(500),
		Price_1 VARCHAR(500),
		Make VARCHAR(50),
		Model VARCHAR(50),
		MinYear VARCHAR(50),
		MaxYear VARCHAR(50),
		TotalVehicles INT
	);

	With CTE
	As
	(
		select Row_Number() Over(Partition by Category.Category
		, Subcategory.Subcategory
		, Part.Part_Desc
		, Detail.Part_Number
		, Detail.Prtc_Description
		, Detail.Price_1 
		  Order By Category.Category, Subcategory.Subcategory, Part.Part_Desc, Detail.Part_Number, 
		  Detail.Prtc_Description, Detail.Price_1) As Row_Num
		, MIN(Detail.Service_BarCode) AS Service_BarCode
		, Category.Category
		, Subcategory.Subcategory
		, Part.Part_Desc
		, Detail.Part_Number
		, Detail.Prtc_Description
		, Detail.Price_1
			FROM Mitchell3.dbo.Detail 
			LEFT OUTER JOIN Mitchell3.dbo.Part ON  
				Detail.Service_BarCode = Part.Service_BarCode  
				AND Detail.nheader = Part.nheader 
				AND Detail.nsection = Part.nsection 
				AND Detail.npart = Part.npart 
			LEFT OUTER JOIN Mitchell3.dbo.Subcategory ON 
				Detail.Service_BarCode = Subcategory.Service_BarCode 
				AND Detail.nheader = Subcategory.nheader 
				AND Detail.nsection = Subcategory.nsection 
			LEFT OUTER JOIN Mitchell3.dbo.Category ON 
				Detail.Service_BarCode = Category.Service_BarCode
				AND Detail.nheader = Category.nheader
			WHERE Detail.Part_Number_Unpunc LIKE '%' + @PartNumber + '%'
			GROUP BY Category, Subcategory, Part_Desc, Part_Number, Prtc_Description, Price_1
	)

	INSERT INTO #TableVehicleByPartNumber(Row_Num, Service_BarCode, Category, Subcategory, Part_Desc, Part_Number, Prtc_Description, Price_1)
	Select  Row_Num, Service_BarCode, Category, Subcategory, Part_Desc, Part_Number, Prtc_Description, Price_1
	From	CTE outCTE
	ORDER BY Category, Subcategory, Part_Desc, Part_Number, Prtc_Description, Price_1

	Create TABLE #TableVehicleDetails 
	( 
		Service_Barcode VARCHAR(50),
		Make VARCHAR(50),
		Model VARCHAR(50),
		VinYear VARCHAR(50)
	);

	INSERT INTO #TableVehicleDetails(Service_Barcode, VinYear, Make, Model)
	SELECT DISTINCT Vehicle_Service_Xref.Service_Barcode,
			Vehicle_Service_Xref.VinYear
		, ISNULL(Makes.Make, '') AS Make
		, ISNULL(Models.Model, '') AS Model
	FROM Vinn.dbo.Vehicle_Service_Xref
	LEFT OUTER JOIN Vinn.dbo.Makes ON Vehicle_Service_Xref.MakeID = Makes.MakeID
	LEFT OUTER JOIN Vinn.dbo.Models ON Vehicle_Service_Xref.ModelID = Models.ModelID
	LEFT OUTER JOIN Vinn.dbo.Submodels ON Vehicle_Service_Xref.SubmodelID = Submodels.SubmodelID
	LEFT OUTER JOIN Vinn.dbo.Engines ON Vehicle_Service_Xref.EngineID = Engines.EngineID
	LEFT OUTER JOIN Vinn.dbo.Bodys ON Vehicle_Service_Xref.BodyID = Bodys.BodyID
	WHERE Vehicle_Service_Xref.Service_Barcode IN (SELECT Service_BarCode FROM #TableVehicleByPartNumber)
	ORDER BY Service_BarCode ASC

	Create TABLE #TableVehicleDetailsMinMaxYear 
	( 
		Service_Barcode VARCHAR(50),
		Make VARCHAR(50),
		Model VARCHAR(50),
		MinYear VARCHAR(50),
		MaxYear VARCHAR(50),
		Total	INT
	);

	INSERT INTO #TableVehicleDetailsMinMaxYear(Service_Barcode, MinYear, MaxYear, Total)
	SELECT Service_Barcode,  MIN(VinYear), MAX(VinYear), COUNT(*)
	FROM	#TableVehicleDetails
	GROUP BY Service_Barcode
	
	UPDATE	TVBPN
	SET		TVBPN.Make = TVDMMY.Make, Model = TVDMMY.Model, MinYear = TVDMMY.MinYear, MaxYear = TVDMMY.MaxYear, 
			TotalVehicles = Table1.Total
	FROM	#TableVehicleByPartNumber TVBPN
	LEFT JOIN #TableVehicleDetailsMinMaxYear TVDMMY ON TVBPN.Service_Barcode = TVDMMY.Service_Barcode
	LEFT JOIN (SELECT Service_Barcode, COUNT(*) AS Total FROM #TableVehicleDetailsMinMaxYear GROUP BY Service_Barcode) AS Table1 
	ON Table1.Service_Barcode = TVDMMY.Service_Barcode

	SELECT Service_Barcode, Category, Subcategory, Part_Desc, Part_Number, Prtc_Description, Price_1, MinYear, MaxYear, Make, Model, TotalVehicles
	FROM	#TableVehicleByPartNumber
	ORDER BY Category, Subcategory, Part_Desc, Part_Number, Prtc_Description

END
GO
