USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE procedure [dbo].[FrameMatrix_GetVehicleFrameDetails] --'Cadillac','DeVille',2004 
@CarMake nvarchar(50), 
@CarModel nvarchar(200), 
@CarYear int 
as 
begin 
select  
carid as 'CarID', 
carmake as 'CarMakes', 
carmodel as 'CarModels', 
caryear as 'Year', 
carframedetails as 'CarFrameDetails',  
carfile as dwf,  
replace(carfile,'.dwf','.png') as jpg 
from FrameMatrix.dbo.etl_VehicleMatrix 
where carMake=@CarMake   
and carYear=@CarYear   
and carModel=@CarModel  
 
end 
 
 
GO
