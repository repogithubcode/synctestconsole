USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO




CREATE PROCEDURE [dbo].[ImportUpdatedServices]
AS

INSERT INTO Service
SELECT *
FROM MitchellImport3.dbo.Service Service2 with(nolock)

INSERT INTO Category
SELECT *
FROM MitchellImport3.dbo.Category with(nolock)

INSERT INTO Subcategory
SELECT *
FROM MitchellImport3.dbo.Subcategory with(nolock)

INSERT INTO Part
SELECT *
FROM MitchellImport3.dbo.Part with(nolock)

INSERT INTO Detail
SELECT *
FROM MitchellImport3.dbo.Detail with(nolock)

INSERT INTO Matrix
SELECT *
FROM MitchellImport3.dbo.Matrix with(nolock)

INSERT INTO Detail_Note_Xref
SELECT *
FROM MitchellImport3.dbo.Detail_Note_Xref with(nolock)

INSERT INTO Part_Note_Xref
SELECT *
FROM MitchellImport3.dbo.Part_Note_Xref with(nolock)

INSERT INTO Hotspot
SELECT *
FROM MitchellImport3.dbo.Hotspot with(nolock)

INSERT INTO Note
SELECT *
FROM MitchellImport3.dbo.Note with(nolock) 

INSERT INTO Graphic
SELECT *
FROM MitchellImport3.dbo.Graphic with(nolock)




GO
