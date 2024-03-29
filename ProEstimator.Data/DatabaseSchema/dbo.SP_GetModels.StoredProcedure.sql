USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
 
 
CREATE PROCEDURE [dbo].[SP_GetModels] 
	@Year Int, 
	@MakeID Int 
AS 
 
BEGIN	 
	select distinct a.ModelId, Model 
	from vinn.dbo.vehicle_service_xref a with(nolock) 
	inner join vinn.dbo.makes b with(nolock) 
	on b.makeid = a.makeid 
	inner join vinn.dbo.models c with(nolock) 
	on c.modelid = a.modelid 
	where VinYear = @Year 
	and a.makeid = @MakeID 
	order by Model 
END 
 
GO
