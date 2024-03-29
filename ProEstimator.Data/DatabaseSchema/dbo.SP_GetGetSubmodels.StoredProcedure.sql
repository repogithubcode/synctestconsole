USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO



Create PROCEDURE [dbo].[SP_GetGetSubmodels]
	@Year Int = NULL,
	@MakeID Int = NULL,
	@ModelID Int = NULL
AS

BEGIN
	SELECT CONVERT(Int,0) 'id', CONVERT(VarChar(50),'') 'Submodel'
	UNION		
	select distinct  a.SubmodelId,isnull(nullif(Submodel,' '),'Base') as Submodel
	from vinn.dbo.vehicle_service_xref a
	inner join vinn.dbo.makes b
	on b.makeid = a.makeid
	inner join vinn.dbo.models c
	on c.modelid = a.modelid
	inner join vinn.dbo.submodels d
	on d.submodelid = a.submodelid
	where VinYear = @Year
	and a.makeid = @MakeID
	and c.modelid = @ModelID
	order by Submodel

END

GO
