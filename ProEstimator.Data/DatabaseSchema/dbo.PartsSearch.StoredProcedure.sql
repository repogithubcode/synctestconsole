USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[PartsSearch] 
	@Year			INT, 
	@Make			INT, 
	@Model			INT, 
	@SubModelID		INT = 0, 
	@EngineID		INT = 0, 
	@Search 		VARCHAR(30) = '' 
AS 
BEGIN 
 
	SELECT DISTINCT TOP 200  
		CAST(MinYear AS VARCHAR) + ' - ' + CAST(MaxYear AS VARCHAR) + ' ' + Vehicle AS Vehicle 
		, Service_Barcode 
		, nheader 
		, nsection 
		, npart 
		, barcode 
		, Part_Number 
		, Description 
		, Price 
	FROM 
	( 
		SELECT DISTINCT 
			  MIN(xref.VinYear) AS MinYear 
			, MAX(xref.VinYear) AS MaxYear 
			, Makes.Make + ' ' + Models.Model + CASE WHEN ISNULL(SubModels.Submodel, '') <> '' THEN ' - ' + SubModels.Submodel ELSE '' END + CASE WHEN ISNULL(Engines.Engine, '') <> '' THEN ' - ' + Engines.Engine ELSE '' END AS Vehicle 
			, Detail.Service_Barcode 
			, Detail.nheader 
			, Detail.nsection 
			, Detail.npart 
			, Detail.barcode 
			, Detail.Part_Number 
			, ISNULL(Detail.Part_Text, '') AS Description 
			, CASE  
				WHEN GetDate() > ISNULL(Detail.Effective_Date_1, '01/01/2000') THEN ISNULL(CONVERT(Money, detail.Price_1) / 100, 0) 
				ELSE  CONVERT(Money, ISNULL(detail.Price_2, 0)) / 100 
			END 'Price' 
		FROM Mitchell3.dbo.Detail 
		JOIN Vinn.dbo.Vehicle_Service_Xref xref ON Detail.Service_BarCode = xref.Service_Barcode 
		JOIN Vinn.dbo.Makes ON xref.MakeID = Makes.MakeID  
		JOIN Vinn.dbo.Models ON xref.ModelID = Models.ModelID 
		JOIN Vinn.dbo.Submodels ON xref.SubModelID = SubModels.SubmodelID 
		JOIN Vinn.dbo.Engines ON xref.EngineID = ENgines.EngineID 
		WHERE 
			(xref.MakeID = @Make OR @Make = 0) 
			AND (xref.ModelID = @Model OR @Model = 0) 
			AND (@EngineID = 0 OR Engines.EngineID = @EngineID) 
			AND (@SubModelID = 0 OR SubModels.SubModelID = @SubModelID) 
			AND  
			( 
				ISNULL(Detail.Part_Text, '') LIKE '%' + @Search + '%' 
				OR Part_Number LIKE '%' + @Search + '%' 
			) 
		GROUP BY 
			  Makes.Make 
			, Models.Model 
			, SubModels.Submodel 
			, Engines.Engine 
			, Detail.Service_Barcode 
			, Detail.nheader 
			, Detail.nsection 
			, Detail.npart 
			, Detail.barcode 
			, Detail.Part_Number 
			, Detail.Part_Text 
			, detail.Price_1 
			, detail.Price_2 
			, detail.Effective_Date_1 
	) AS Base 
	WHERE 
		(@Year = 0 OR @Year BETWEEN MinYear AND MaxYear) 
		AND (ISNULL(Part_Number, '') <> '' OR ISNULL(Price, 0) > 0) 
		ORDER BY Part_Number, Description 
END 
GO
