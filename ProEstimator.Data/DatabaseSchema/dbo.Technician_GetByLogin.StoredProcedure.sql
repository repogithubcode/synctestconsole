USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure

CREATE PROCEDURE [dbo].[Technician_GetByLogin] 
	@LoginID			int,
	@IsDeleted			bit = 0
AS 
BEGIN 
	-- SET NOCOUNT ON added to prevent extra result sets from 
	-- interfering with SELECT statements. 
	SET NOCOUNT ON; 
 
    SELECT *, CASE WHEN TimeStamp IS NULL THEN 0 ELSE 1 END AS IsDeleted
	FROM TechniciansData 
	WHERE 
		LoginID = @LoginID 
		AND 
		(	(@IsDeleted = 1 AND TimeStamp IS NOT NULL)
			OR (@IsDeleted = 0 AND TimeStamp IS NULL)
		)
		
	ORDER BY ISNULL(OrderNo, 0) 
END 
GO
