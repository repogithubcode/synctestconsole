USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		Ezra
-- Create date: 11/2/2022
-- Description:	For the admin page, return Part match summary info for the passed search string.
--				Note that this returns 2 tables.  
--				The first table shows the parts that matched and how many vehicles made the match.
--				The second table shows details of vehicles that don't have a parts match.
-- =============================================
CREATE PROCEDURE [dbo].[LinkRule_GetPartMatchDetailsSummary]
	@Search				VARCHAR(MAX) -- = 'Detail.Prtc_Description LIKE ''%door shell%'''
AS
BEGIN
	-- SET NOCOUNT ON added to prevent extra result sets from
	-- interfering with SELECT statements.
	SET NOCOUNT ON;

	-- Get all cars (made on or after 2000) in a temp table
	Create TABLE #AllCars 
	(
		  ServiceBarcode		int
		, Make					varchar(50)
		, Model					varchar(50)
		, YearStart				int
		, YearEnd				int
	)

	INSERT INTO #AllCars
	SELECT 
		  Vehicle_Service_Xref.Service_Barcode
		, Makes.Make
		, Models.Model
		, MIN(VinYear) AS YearStart
		, MAX(VinYear) AS YearEnd
	FROM Vinn.dbo.Vehicle_Service_Xref
	LEFT OUTER JOIN Vinn.dbo.Makes ON Vehicle_Service_Xref.MakeID = Makes.MakeID
	LEFT OUTER JOIN Vinn.dbo.Models ON Vehicle_Service_Xref.ModelID = Models.ModelID
	LEFT OUTER JOIN Mitchell3.dbo.Service ON Vehicle_Service_Xref.Service_Barcode = Service.Service_Barcode
	WHERE Vehicle_Service_Xref.VinYear >= 2000
		AND VinYear BETWEEN from_year AND to_year
	GROUP BY
		  Vehicle_Service_Xref.Service_Barcode
		, Makes.Make
		, Models.Model
	ORDER BY Make, Model



	-- Get a list of all service barcodes and part descriptions that match the searched words
	Create TABLE #Base 
	(
		  ServiceBarcode		int
		, Category				varchar(100)
		, Prtc_Description		varchar(100)
	)

	DECLARE @Query NVARCHAR(MAX) = 
	'SELECT DISTINCT Category.Service_BarCode, Category.Category, CASE WHEN Prtc_Description LIKE ''L %'' OR Prtc_Description LIKE ''R %'' THEN ''L/R '' + SUBSTRING(Prtc_Description, 3, LEN(Prtc_Description) - 2) ELSE Prtc_Description END AS Prtc_Description
	FROM Mitchell3.dbo.Category
	LEFT OUTER JOIN Mitchell3.dbo.Subcategory ON Subcategory.Service_BarCode = Category.Service_BarCode AND Subcategory.nheader = Category.nheader
	LEFT OUTER JOIN Mitchell3.dbo.Detail ON Category.Service_BarCode = Detail.Service_BarCode AND Category.nheader = Detail.nheader
	LEFT OUTER JOIN Vinn.dbo.Vehicle_Service_Xref ON Category.Service_BarCode = Vehicle_Service_Xref.Service_Barcode
	WHERE ' + @Search

	INSERT INTO #Base
	EXECUTE sp_executesql @Query


	-- Get all part matches
	SELECT 
		  Category
		, Prtc_Description AS PartDescription
		, COUNT(*) AS VehicleCount
	FROM #Base 
	GROUP BY Category, Prtc_Description
	ORDER BY COUNT(*) DESC

	-- Select all cars that DON'T match the search
	SELECT 
		  AllCars.ServiceBarcode
		, AllCars.Make
		, AllCars.Model
		, MIN(YearStart) AS YearStart
		, MAX(YearEnd) AS YearEnd
	FROM #AllCars AllCars
	LEFT OUTER JOIN #Base Base ON AllCars.ServiceBarcode = Base.ServiceBarcode 
	WHERE Base.ServiceBarcode IS NULL
	GROUP BY 
		  AllCars.ServiceBarcode
		, AllCars.Make
		, AllCars.Model
	ORDER BY Make, Model

END
GO
