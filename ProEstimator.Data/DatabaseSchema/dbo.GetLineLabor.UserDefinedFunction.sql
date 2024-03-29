USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- User Defined Function  
  
  
  
CREATE FUNCTION [dbo].[GetLineLabor] (@EstimationLineItemsID Int, @BodyLaborAmount Real, @OverlapAmount1 Real, @OverlapAmount2 Real, @LaborMinimum Real, @OverlapLaborRate Real, @DollarsHours Char(1), @LaborFilter VARCHAR(5))  
RETURNS VarChar(500) AS    
BEGIN 	  
	DECLARE @LaborList VarChar(500)  
	DECLARE @LaborTotal REAL = 0  
	DECLARE @OverlapInfo VarChar(25)  
	DECLARE @MainLabor VarChar(25)  
	DECLARE @ServiceBarcode VarChar(10)  
	DECLARE @AdminInfoID Int  
	DECLARE @Changed VarChar(10)  
	DECLARE @ActionCode VarChar(25)  
	DECLARE @CustomerProfilesID INT 
 
	SELECT @LaborList = '', @Changed = ''  
  
	-- Get the Admin Info ID and serivce barcode 
	SELECT   
		@AdminInfoID = EstimationData.AdminInfoID,  
		@ActionCode = EstimationLineItems.ActionCode , 
		@CustomerProfilesID = AdminInfo.CustomerProfilesID 
	FROM EstimationData  
	INNER JOIN EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID  
	INNER JOIN AdminInfo ON AdminInfo.ID = EstimationData.AdminInfoID 
	WHERE EstimationLineItems.id = @EstimationLineItemsID   
  
	SELECT @ServiceBarcode = FocusWrite.Dbo.GetServiceBarcode(@AdminInfoID)  
  
	-- Make sure Labor values are 0 instead of null 
	SET @BodyLaborAmount = ROUND(ISNULL(@BodyLaborAmount, 0), 1) 
	SET @LaborMinimum = ROUND(ISNULL(@LaborMinimum, 0), 1) 
	SET @OverlapAmount1 = ROUND(ISNULL(@OverlapAmount1, 0), 1) 
	  
	-- Figure out if the labor is "included" 
	DECLARE @OverlapInclude BIT = 0  
	IF 	 
		(@LaborFilter = 'Extra' OR @LaborFilter = '') AND @ActionCode IN ('R+R','R+I','Replace')  
		AND  
		( 
			(@OverlapAmount2 IS NOT NULL AND @OverlapAmount2 = 0)  
			OR  
			( 
				ABS(@BodyLaborAmount) > 0 
				AND 
				( 
					( 
						@LaborMinimum = 0 
						AND (ABS(@BodyLaborAmount) <= ABS(@OverlapAmount1)) 
					) 
					OR  
					( 
						@LaborMinimum <> 0  
						AND (ABS(@BodyLaborAmount) - ABS(@LaborMinimum) <= 0) 
					) 
				)  
			)					 
		)  
		SELECT   
			@OverlapInclude = 1  
	  
	SELECT	  
		@LaborList = @LaborList +   
		CASE 	  
			WHEN @DollarsHours = 'H' THEN  
				CASE  
					WHEN EstimationLineLabor.LaborTime IS NOT NULL  
						-- Need to do the total calculation twice, the first time to make sure it's not 0 
						AND  
						( 
							EstimationLineLabor.LaborTime  
							+ CASE WHEN EstimationlineLabor.LaborType = 1 AND @OverlapAmount1 > -90  THEN @OverlapAmount1 ELSE 0 END 
							+ CASE WHEN EstimationlineLabor.LaborType = 1 AND @OverlapAmount2 > -90  THEN ISNULL(@OverlapAmount2, 0) ELSE 0 END	-- 4/9/2017 - Ezra - remove overlap from main labor display 
							- 
							CASE  
								WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction  
								WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction  
								ELSE 0  
							END  
						) <> 0 
 
						-- Now do the calculation to add to the line 
						THEN FocusWrite.dbo.FormatNumber(EstimationLineLabor.LaborTime  
							+ CASE WHEN EstimationlineLabor.LaborType = 1 AND @OverlapAmount1 > -90  THEN @OverlapAmount1 ELSE 0 END 
							+ CASE WHEN EstimationlineLabor.LaborType = 1 AND @OverlapAmount2 > -90  THEN ISNULL(@OverlapAmount2, 0) ELSE 0 END	-- 4/9/2017 - Ezra - remove overlap from main labor display 
							- 
							CASE  
								WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction  
								WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction  
								ELSE 0  
							END  
						, 1) + ' hrs. ' + ISNULL(LaborTypes.LaborType, '')  ELSE ''  
					END  
			WHEN @DollarsHours = 'D' THEN  
				CASE   
					WHEN EstimationLineLabor.LaborTime IS NOT NULL THEN   
						'$' + FocusWrite.dbo.FormatNumber((EstimationLineLabor.LaborTime - CASE WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction ELSE 0 END) * ISNULL(ISNULL(CustomerProfileRates2.Rate,CustomerProfileRates.rate),0),2) + ' ' + ISNULL(LaborTypes.LaborType, '')   
					ELSE ''   
				END  
			END +   
		CASE WHEN ISNULL(EstimationLineLabor.LaborCost,0) <> 0 THEN ', $' + FocusWrite.dbo.FormatNumber(EstimationLineLabor.LaborCost,2) + ' ' + ISNULL(LaborTypes.LaborType, '')  ELSE '' END + ' ' +  
		-- Ezra 6/1/2017 - betterment on labor items was started but abandoned for first release.  Come back and finish this eventually  
		--CASE WHEN ISNULL(EstimationLineItems.BettermentType, '') = 'P' AND EstimationLineLabor.BettermentFlag <> 0 THEN '(Bet)' ELSE '' END +   
		CASE WHEN EstimationLineLabor.SubletFlag <> 0 THEN '(Sub)' ELSE '' END + ', ',  
 
		@LaborTotal += ROUND( 
							EstimationLineLabor.LaborTime -  
							CASE  
								WHEN EstimationLineLabor.AdjacentDeduction = 1 THEN CustomerProfilesPaint.AdjacentDeduction  
								WHEN EstimationLineLabor.AdjacentDeduction = 2 THEN CustomerProfilesPaint.NonAdjacentDeduction  
							ELSE 0 END 
						, 1),  
 
		@ActionCode = EstimationLineItems.ActionCode  
 
	FROM EstimationLineItems 
	LEFT JOIN EstimationLineLabor ON EstimationLineLabor.EstimationLineItemsID = EstimationLineItems.ID  
	LEFT JOIN LaborTypes ON LaborTypes.ID = EstimationLineLabor.LaborType  
 
	LEFT JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates.RateType = LaborTypes.RateTypesID  
	LEFT JOIN CustomerProfileRates CustomerProfileRates2 ON CustomerProfileRates2.CustomerProfilesID = @CustomerProfilesID AND CustomerProfileRates2.RateType = CustomerProfileRates.IncludeIn  
	LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = @CustomerProfilesID  
 
	WHERE   
		EstimationLineItems.ID = @EstimationLineItemsID   
  
		AND EstimationLineLabor.LaborType > 0 
		AND (EstimationLineLabor.LaborType <> 1 OR @OverlapInclude = 0)  
  
		-- 11/8/16 - Ezra - Only use labor items with "Include" checked  
		AND ((ISNULL(EstimationLineLabor.Include, 1) = 1 OR EstimationLineLabor.LaborType NOT IN (20, 21, 22, 26)))  
  
		-- @LaborFilter is either empty to add no filter, 'Main' to show only body labor and base coat, or 'Extra' to show the rest  
		AND  
		(  
			@LaborFilter = ''  
 
			-- Paint Panel - Clearcoat, Base Coat, Paint Panel - None, 3 Stage, 2 Stage, 2 Tone 
			OR @LaborFilter = 'Main' AND EstimationlineLabor.LaborType IN (9, 16, 17, 18, 19, 29)  
 
			-- Clearcoat, Edging, Underside, Blend, 3 Stg Allow, 2 Tone Allow 
			OR @LaborFilter = 'Paint' AND EstimationlineLabor.LaborType IN (20, 21, 22, 26, 27, 28)  
 
			-- Body, Frame, Structure, Mechanical, Detail, Cleanump, Other, Electrical, Glass 
			OR @LaborFilter = 'Extra' AND EstimationlineLabor.LaborType IN (1, 2, 3, 4, 5, 6, 8, 24, 25)  
		)  
  
	SELECT	@LaborList = @LaborList +   
		CASE  
			WHEN EstimationLineLabor.SubletFlag <> 0 THEN '(Sub)'  
			ELSE ''  
		--END + ', ',  
		END 
 
	FROM AdminInfo  
	INNER JOIN EstimationData ON EstimationData.AdminInfoID = AdminInfo.ID  
	INNER JOIN EstimationLineItems ON EstimationLineItems.EstimationDataID = EstimationData.ID  
	LEFT JOIN EstimationLineLabor ON EstimationLineLabor.EstimationLineItemsID = EstimationLineItems.ID  
	LEFT JOIN LaborTypes ON LaborTypes.ID = EstimationLineLabor.LaborType  
	LEFT JOIN LaborTypes LaborTypesPaint ON LaborTypesPaint.ID = EstimationLineLabor.LaborType  
	LEFT JOIN CustomerProfilesPaint ON CustomerProfilesPaint.CustomerProfilesID = AdminInfo.CustomerProfilesID  
	LEFT JOIN CustomerProfileRates ON CustomerProfileRates.CustomerProfilesID = AdminInfo.CustomerProfilesID AND CustomerProfileRates.RateType = LaborTypes.RateTypesID  
	LEFT JOIN CustomerProfileRates CustomerProfileRates2 ON CustomerProfileRates2.CustomerProfilesID = AdminInfo.CustomerProfilesID AND CustomerProfileRates2.RateType = CustomerProfileRates.IncludeIn  
	WHERE   
		EstimationLineItems.ID = @EstimationLineItemsID  
		AND ISNULL(EstimationLineLabor.AdjacentDeduction,0) > 0   
		AND EstimationLineLabor.LaborType IN (9,19,18,29)  
 
  
	SELECT @LaborList = RTRIM(@LaborList)  
	IF LEN(@LaborList) > 2   
		SELECT @LaborList = LEFT(@LaborList ,LEN(@LaborList) - 1)  
	ELSE  
		SELECT @LaborList = ''  
	SELECT @LaborList = ISNULL(@LaborList,'')  
 
	-- Now add in overlap information  
	IF 	@OverlapInclude = 1  
		SELECT   
			@LaborList = 'Included' +  
			CASE WHEN ISNULL(@LaborList,'') <> '' THEN ', ' ELSE '' END + @LaborList  
  
	IF 	(@LaborMinimum = 0 AND (ABS(@BodyLaborAmount) > ABS(@OverlapAmount1) AND ABS(@BodyLaborAmount) > 0)) OR  
		(@LaborMinimum <> 0 AND (ABS(@BodyLaborAmount) - ABS(@LaborMinimum) > 0 AND ABS(@BodyLaborAmount) > 0))  
	BEGIN  
		IF @BodyLaborAmount > 0  
		BEGIN  
			SELECT @MainLabor = ''  
 
			IF @OverlapAmount1 <> 0  
			BEGIN  
				IF (SELECT ISNULL(EstimationLineItems.PartOfOverhaul, 0)  
					FROM AdminInfo WITH (NOLOCK)  
					INNER JOIN EstimationData WITH (NOLOCK) ON (EstimationData.AdminInfoID = AdminInfo.ID)  
					INNER JOIN EstimationLineItems WITH (NOLOCK) ON (EstimationLineItems.EstimationDataID = EstimationData.ID)  
					WHERE EstimationLineItems.ID = @EstimationLineItemsID ) = 0  
				BEGIN  
					IF @OverlapAmount2 IS NOT NULL AND @OverlapAmount2 = 0  
						SELECT @OverlapAmount1 = -99.9  
						  
					IF @BodyLaborAmount + @OverlapAmount1 < @LaborMinimum		SELECT @OverlapAmount1 = ABS(@BodyLaborAmount - @LaborMinimum)  
					ELSE IF ROUND(ABS(@OverlapAmount1),1) > @BodyLaborAmount		SELECT @OverlapAmount1 = 0  
					ELSE  									SELECT @OverlapAmount1 = ABS(@OverlapAmount1)  
		  
					IF @OverlapAmount1 <> 0  
					BEGIN  
						IF @DollarsHours = 'D'  
							SELECT @OverlapInfo = '-$' +   
								FocusWrite.dbo.FormatNumber(@OverlapAmount1 * @OverlapLaborRate,2) + ' hrs. Overlap',  
								@LaborList = @MainLabor + CASE WHEN ISNULL(@MainLabor,'') <> '' AND ISNULL(@OverlapInfo,'') <> '' THEN ', ' ELSE '' END +  
								@OverlapInfo + CASE WHEN ISNULL(@OverlapInfo,'') <> '' AND ISNULL(@LaborList,'') <> '' THEN ', ' ELSE '' END +  
								@LaborList  
					END  
				END  
			END  
			ELSE  
			BEGIN  
				SELECT @LaborList = @MainLabor +   
					CASE WHEN ISNULL(@MainLabor,'') <> '' AND ISNULL(@LaborList,'') <> '' THEN ', ' ELSE '' END +  
					@LaborList  
			END  
		END  
	END  
 
	IF SUBSTRING(@LaborList,1,2) = ', '	  
		SELECT @LaborList = SUBSTRING(@LaborList,3,LEN(@LaborList))  
  
	RETURN   
		CASE  
			WHEN @DollarsHours = ''  
				THEN CAST(@LaborTotal AS VARCHAR)  
				ELSE REPLACE(REPLACE(@LaborList, ' ,', ','), ',,', ',')  
		END  
END  
  
  
  
GO
