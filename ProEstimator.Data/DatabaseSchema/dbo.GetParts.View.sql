USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO








CREATE  VIEW [dbo].[GetParts]
AS
SELECT        dbo.AdminInfo.id AS AdminInfoID, dbo.RateTypes.id AS RateTypesID, dbo.RateTypes.RateName, ISNULL(EstimationLineItems_1.PartSource, '') AS PartSource, CONVERT(VarChar(50), 
                         CASE WHEN EstimationLineItems_1.PartSource = '' THEN 'Other' ELSE ISNULL(EstimationLineItems_1.PartSource, 'Other') END) + ' ' + 'Parts' AS [Type], SUM(CASE WHEN isnull(CustomerProfileRates.DiscountMarkup, 0) 
                         > 0 THEN round(ISNULL(EstimationLineItems_1.Price, 0) * ISNULL(EstimationLineItems_1.Qty, 1) + (ISNULL(EstimationLineItems_1.Price, 0) * ISNULL(EstimationLineItems_1.Qty, 1) * (CustomerProfileRates.DiscountMarkup / 100)), 2) 
                         ELSE round(ISNULL(round(EstimationLineItems_1.Price, 2), 0) * ISNULL(EstimationLineItems_1.Qty, 1), 2) END) AS Price, 
                         CASE WHEN EstimationLineItems_1.BettermentType <> '' THEN 1 ELSE 0 END AS Betterment, 
                         CASE WHEN EstimationLineItems_1.SubletOperationFlag <> 0 THEN 1 WHEN EstimationLineItems_1.SubletPartsFlag <> 0 THEN 1 ELSE 0 END AS Sublet, dbo.RateTypes.EMSCode1, dbo.RateTypes.EMSBetterment, 
                         dbo.RateTypes.EMSSublet, dbo.CustomerProfileRates.Rate, dbo.CustomerProfileRates.CapType, dbo.CustomerProfileRates.Cap, dbo.CustomerProfileRates.DiscountMarkup, dbo.CustomerProfileRates.Taxable, 
                         dbo.CustomerProfileRates.IncludeIn
FROM            dbo.AdminInfo INNER JOIN
                         dbo.EstimationData AS EstimationData_1 ON EstimationData_1.AdminInfoID = dbo.AdminInfo.id INNER JOIN
                         dbo.EstimationLineItems AS EstimationLineItems_1 ON EstimationLineItems_1.EstimationDataID = EstimationData_1.id LEFT OUTER JOIN
                         dbo.EstimationLineItems AS EstimationLineItemsOverridden ON EstimationLineItemsOverridden.ModifiesID = EstimationLineItems_1.id LEFT OUTER JOIN
                             (SELECT        'OEM Parts' AS 'Type', 8 AS 'RateTypesID'
                               UNION
                               SELECT        'LKQ Parts' AS Expr1, 9 AS Expr2
                               UNION
                               SELECT        'Reman Parts' AS Expr1, 13 AS Expr2
                               UNION
                               SELECT        'After Parts' AS Expr1, 10 AS Expr2
                               UNION
                               SELECT        'Other Parts' AS Expr1, 18 AS Expr2) AS PartRates ON PartRates.[Type] = CONVERT(VarChar(50), CASE WHEN EstimationLineItems_1.PartSource = '' THEN 'Other' ELSE ISNULL(EstimationLineItems_1.PartSource, 
                         'Other') END) + ' ' + 'Parts' LEFT OUTER JOIN
                         dbo.RateTypes ON dbo.RateTypes.id = PartRates.RateTypesID INNER JOIN
                         dbo.CustomerProfiles ON dbo.CustomerProfiles.id = dbo.AdminInfo.CustomerProfilesID LEFT OUTER JOIN
                         dbo.CustomerProfileRates ON dbo.CustomerProfileRates.CustomerProfilesID = dbo.CustomerProfiles.id AND dbo.CustomerProfileRates.RateType = dbo.RateTypes.id
WHERE        (EstimationLineItemsOverridden.ModifiesID IS NULL) AND (EstimationLineItems_1.id NOT IN
                             (SELECT DISTINCT EstimationOverlap.EstimationLineItemsID2
                               FROM            dbo.EstimationData AS EstimationData INNER JOIN
                                                         dbo.EstimationLineItems AS EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.id INNER JOIN
                                                         dbo.EstimationOverlap AS EstimationOverlap ON EstimationOverlap.EstimationLineItemsID2 = EstimationLineItems.id
                               WHERE        (EstimationData.AdminInfoID = dbo.AdminInfo.id) AND (EstimationOverlap.UserAccepted <> 0) AND (EstimationOverlap.OverlapAdjacentFlag = 'S')))
GROUP BY dbo.AdminInfo.id, dbo.RateTypes.id, dbo.RateTypes.RateName, ISNULL(EstimationLineItems_1.PartSource, ''), CONVERT(VarChar(50), 
                         CASE WHEN EstimationLineItems_1.PartSource = '' THEN 'Other' ELSE ISNULL(EstimationLineItems_1.PartSource, 'Other') END) + ' ' + 'Parts', 
                         CASE WHEN EstimationLineItems_1.BettermentType <> '' THEN 1 ELSE 0 END, 
                         CASE WHEN EstimationLineItems_1.SubletOperationFlag <> 0 THEN 1 WHEN EstimationLineItems_1.SubletPartsFlag <> 0 THEN 1 ELSE 0 END, dbo.RateTypes.EMSCode1, dbo.RateTypes.EMSBetterment, 
                         dbo.RateTypes.EMSSublet, dbo.CustomerProfileRates.Rate, dbo.CustomerProfileRates.CapType, dbo.CustomerProfileRates.Cap, dbo.CustomerProfileRates.DiscountMarkup, dbo.CustomerProfileRates.Taxable, 
                         dbo.CustomerProfileRates.IncludeIn






GO
