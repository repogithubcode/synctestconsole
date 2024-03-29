USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
 
CREATE PROCEDURE [dbo].[EMS_Export_GetProfileRates2] 
	@AdminInfoID Int 
AS 
 
SET NOCOUNT ON 

declare @CreatorID int

select @CreatorID = CreatorID from AdminInfo as a where id = @AdminInfoID

if @CreatorID = 56027
begin
 exec [dbo].[EMS_Export_GetProfileRates2_201] @AdminInfoID 
end 
else 
begin



DECLARE @PolicyNumber VarChar(25) 
DECLARE @ClaimNumber VarChar(25) 
DECLARE @CustomerProfilesID Int 
DECLARE @OwnerID Int 
 
SELECT @CustomerProfilesID = CustomerProfiles.id 
FROM AdminInfo AdminInfo WITH (NOLOCK) 
LEFT JOIN CustomerProfiles CustomerProfiles WITH (NOLOCK) ON 
	(CustomerProfiles.id = AdminInfo.CustomerProfilesID) 
WHERE AdminInfo.ID = @AdminInfoID 
 
IF @CustomerProfilesID IS NULL  
BEGIN 
	SELECT @OwnerID = CreatorID 
	FROM AdminInfo WITH (NOLOCK) 
	WHERE AdminInfo.ID = @AdminInfoID 
 
	SELECT @CustomerProfilesID = CustomerProfiles.id 
	FROM CustomerProfiles CustomerProfiles WITH (NOLOCK) 
	WHERE CustomerProfiles.DefaultFlag <> 0 AND 
		CustomerProfiles.OwnerID = @OwnerID 
END 
 
SELECT DISTINCT 
	CAST(LEFT(ISNULL(CodeListRateTypes.Code,''),4) as nvarchar(4)) 		LBR_TYPE, 
	CAST(LEFT(	CASE	WHEN CodeListRateTypes.Code IS NOT NULL THEN CodeListRateTypes.Description 
			ELSE 'Unknown' 
		END,20) as nvarchar(20))				LBR_DESC, 
	CustomerProfileRates.Rate		LBR_RATE, 
	CustomerProfileRates.Taxable		LBR_TAX_IN, 
	CustomerProfilesMisc.TaxRate		LBR_TAXP, 
	CustomerProfileRates.DiscountMarkup	LBR_ADJP, 
	NULL LBR_TX_TY1,	 
	NULL LBR_TX_IN1,	 
	NULL LBR_TX_TY2,	 
	NULL LBR_TX_IN2, 
	NULL LBR_TX_TY3,	 
	NULL LBR_TX_IN3,	 
	NULL LBR_TX_TY4,	 
	NULL LBR_TX_IN4, 
	NULL LBR_TX_TY5,	 
	NULL LBR_TX_IN5  
 
FROM AdminInfo AdminInfo WITH (NOLOCK) 
INNER JOIN CustomerProfiles CustomerProfiles WITH (NOLOCK) ON 
	(CustomerProfiles.id = AdminInfo.CustomerProfilesID) 
INNER JOIN CustomerProfilesMisc CustomerProfilesMisc WITH (NOLOCK) ON 
	(CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id) 
 
LEFT JOIN CustomerProfileRates CustomerProfileRates WITH (NOLOCK) ON 
	(CustomerProfileRates.CustomerProfilesID = CustomerProfiles.id) 
 
LEFT JOIN RateTypes RateTypes WITH (NOLOCK) ON 
	(RateTypes.id = CustomerProfileRates.RateType) 
 
LEFT JOIN CodeList CodeListRateTypes WITH (NOLOCK) ON 
	(CodeListRateTypes.MasterCodeListTabsID = 28 AND --IN (28,33) AND 
	 CodeListRateTypes.RateTypeID = CustomerProfileRates.RateType) 
 
WHERE AdminInfo.ID = @AdminInfoID  AND 
	CodeListRateTypes.Code IS NOT NULL 
and CAST(LEFT(ISNULL(CodeListRateTypes.Code,''),4) as varchar(4))  not in  
('LA2S','LA2U','LA3S','LA3U','LABA','LABS','LAE2','LAE3','LAET','LAFA','LAFS','LAGA','LAGS','LAMA','LAMS','LARA' 
,'LARN','LARS','LASA','LASS','LATT','LAUT')  
 
 
 
 end
 
GO
