USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER OFF
GO




CREATE PROCEDURE [dbo].[GetEstimateReportTotalsOnly2]
	@AdminInfoID Int,
	@SupplementVersion Bit = NULL,
	@AlertText VarChar(100) = NULL
AS
SET NOCOUNT ON
DECLARE @VehicleValue Money
DECLARE @TotalLossPerc Decimal(9,2)
DECLARE @Total Money


SELECT @Total = CONVERT(Money, REPLACE(REPLACE(AdminInfo.GrandTotal ,',',''),'$',''))
FROM AdminInfo WITH (NOLOCK)
WHERE AdminInfo.id = @AdminInfoID

IF @AlertText IS NOT NULL
BEGIN
	SELECT @AlertText = ''
	SELECT @VehicleValue = VehicleInfo.VehicleValue
	FROM EstimationData WITH (NOLOCK)
	INNER JOIN VehicleInfo WITH (NOLOCK)
		 ON (VehicleInfo.EstimationDataId = EstimationData.Id)
	WHERE EstimationData.AdminInfoID = @AdminInfoID
	
	IF ISNULL(@VehicleValue,0)>0
	BEGIN
		SELECT @TotalLossPerc = CustomerProfilesMisc.TotalLossPerc
		FROM AdminInfo WITH (NOLOCK)
		INNER JOIN CustomerProfiles WITH (NOLOCK) 
			ON (CustomerProfiles.ID = AdminInfo.CustomerProfilesID)
		INNER JOIN CustomerProfilesMisc WITH (NOLOCK)
			ON (CustomerProfilesMisc.CustomerProfilesID = CustomerProfiles.id)
		WHERE AdminInfo.ID = @AdminInfoID

		IF ISNULL(@TotalLossPerc,0)>0
		BEGIN
			IF @VehicleValue * (@TotalLossPerc/100) < @Total
			BEGIN
				SELECT @AlertText = 'Estimate Total has exceeded ' + FocusWrite.dbo.FormatNumber(@TotalLossPerc,1) + '% of the vehicle''s value.'
			END
		END
	END
END

SELECT AdminInfo.GrandTotal 'Total',
	AdminInfo.BettermentTotal 'BettermentTotal',
	@AlertText 'AlertText'
FROM AdminInfo WITH (NOLOCK)
WHERE AdminInfo.id = @AdminInfoID
 


GO
