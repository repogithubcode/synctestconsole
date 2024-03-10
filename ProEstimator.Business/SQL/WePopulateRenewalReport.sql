SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 6/14/2019
-- Description:	nightly job to update renewed 
--				status for renewal report table
--				Web Est Version
-- =============================================
ALTER PROCEDURE [Admin].[UpdateRenewalReport_WE] 
	@year  INT = 2019, 
    @month    INT = 6,
	@loginid INT = 0,  
	@salesrepid INT = 0,  
	@salesrep NVARCHAR(150) = NULL,  
	@companyname NVARCHAR(500) = NULL,  
	@contact NVARCHAR(200) = NULL,  
	@phone NVARCHAR(25) = NULL,  
	@state NVARCHAR(25) = NULL,  
	@framedata BIT = NULL,  
	@ems BIT = NULL,  
	@multi BIT = NULL,  
	@pdr BIT = NULL,  
	@renewalamount REAL = NULL,  
	@renewaldate DATE = NULL,  
	@estcounttotal INT = NULL,  
	@estcountcur INT = NULL,  
	@yearswe INT = NULL,  
	@willrenew BIT = NULL,  
	@willnotrenew BIT = NULL,  
	@hasrenewed BIT = NULL,  
	@pastdue REAL = NULL,  
	@notes NVARCHAR(2000) = NULL,
	@platform NVARCHAR(20) = NULL,
	@createDate DATETIME = NULL,
	@existingRecordExists INT = NULL
AS
BEGIN

DECLARE PE_CURSOR CURSOR
	LOCAL STATIC READ_ONLY FORWARD_ONLY
FOR
SELECT c.loginid, 
       s.salesrepid, 
       s.firstname							           AS SalesRep, 
	   isNull(ci_CompanyName.ItemText, '')			   AS CompanyName,
       ( isNull(ci_FirstName.ItemText, '') + ' ' 
	   + isNull(ci_LastName.ItemText, '') )			   AS Contact,
		  
       --cp.phone1                                       AS Phone, 
	   (
				SELECT		TOP 1 ci.ItemText AS Phone
				FROM		[10.0.7.11,63693].FocusWrite.dbo.ContactItems ci (NOLOCK)
				WHERE		ci.ContactsID = l.ContactsID
				AND			ci.ContactItemTypeID = 17	-- Phone
				AND			ci.Qualifier NOT IN ('FX', 'HF')
				ORDER BY	( CASE
								WHEN ci.Qualifier = 'WF' THEN 1	-- Work Phone
								WHEN ci.Qualifier = 'WP' THEN 2	-- Business Phone
								WHEN ci.Qualifier = 'WC' THEN 3	-- Work Cell
								WHEN ci.Qualifier = 'CP' THEN 4	-- Cell Phone
								WHEN ci.Qualifier = 'HP' THEN 5 -- Home Phone
								WHEN ci.Qualifier = 'NP' THEN 6 -- Evening Phone
								WHEN ci.Qualifier = 'PC' THEN 7 -- Personal Cell Phone
								WHEN ci.Qualifier = 'VM' THEN 8 -- Voice Mail
								ELSE 99
							END )
			)										   AS Phone,

       [State].ItemText								   AS State, 
       --c.contractid,   
       CONVERT(BIT, Isnull(fc.contractid, 0))          AS FrameData, 
       CONVERT(BIT, Isnull(e.contractid, 0))           AS EMS, 
       CONVERT(BIT, Isnull(multi.contractid, 0))       AS Multi, 
       CONVERT(BIT, Isnull(pdr.contractid, 0))         AS PDR, 
       Isnull(pNext.termtotal, pCurrent.termtotal)     AS RenewalAmount, 
       CONVERT(VARCHAR(10), c.expirationdate + 1, 101) AS RenewalDate, 
	   (SELECT [dbo].[WE_Getlifetimeestimatesforlogin] (l.id)) AS 'estcounttotal' ,
	   (SELECT [dbo].[WE_GetCurrentContractEstimatesForLogin] (l.id)) AS 'estcountcur' ,
       (SELECT Count(DISTINCT Year(est.effectivedate))
		FROM [Contract] est
		WHERE ContractID IN (
			SELECT ContractID
			FROM vwContractLogins
		WHERE LoginID = c.LoginID
		)
		AND YEAR(est.ExpirationDate) != YEAR(GETDATE()) + 1)     AS yearswe, 
       c.willrenew                                     AS WillRenew, 
       CASE 
         WHEN COALESCE(c.willrenew, 0) = 0 THEN 1 
         ELSE 0 
       END                                             AS WillNotRenew, 
       dbo.Hasrenewed(c.loginid, c.expirationdate)     AS hasRenewed, 
       (SELECT Sum(invoiceamount) 
        FROM   [10.0.7.11,63693].FocusWrite.dbo.invoice 
        WHERE  LoginID = c.LoginID 
              AND paid = 0
			   AND DueDate <= GETDATE()
			   AND DueDate >= c.EffectiveDate)                AS PastDue, 
       c.notes,
	   'WE'											   AS 'Platform',
	   GETDATE()									   AS 'CreateDate'
FROM   [10.0.7.11,63693].FocusWrite.dbo.vwcontractlogins c (nolock) 
       INNER JOIN [10.0.7.11,63693].FocusWrite.dbo.logins l (nolock) 
               ON c.loginid = l.id 
                  AND l.doubtfulaccount = 0 
       INNER JOIN [10.0.7.11,63693].FocusWrite.dbo.contractpricelevels p (nolock) 
               ON c.contractpricelevelid = p.contractpricelevelid 
                  AND p.pricelevel >= 0 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.vwcontractpricelevelterms pCurrent 
              ON p.contracttermid = pCurrent.contracttermid 
                 AND pCurrent.pricelevel = p.pricelevel 
                 AND pCurrent.termactive = 1 
                 AND pCurrent.pricelevelactive = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.vwcontractpricelevelterms pNext 
              ON p.contracttermid = pNext.previouscontracttermid 
                 AND pNext.pricelevel = p.pricelevel 
                 AND pNext.termactive = 1 
                 AND pNext.pricelevelactive = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.organizationinfo o (nolock) 
              ON l.organizationid = o.id 
	   LEFT JOIN	[10.0.7.11,63693].FocusWrite.dbo.ContactItems ci_FirstName (NOLOCK)
				ON l.ContactsID = ci_FirstName.ContactsID
				AND ci_FirstName.ContactItemTypeID = 2
	   LEFT JOIN	[10.0.7.11,63693].FocusWrite.dbo.ContactItems ci_LastName (NOLOCK)
				ON l.ContactsID = ci_LastName.ContactsID
				AND ci_LastName.ContactItemTypeID = 6
		INNER JOIN	[10.0.7.11,63693].FocusWrite.dbo.ContactItems [State] (NOLOCK)
				ON [State].ContactsID = o.OrgInfoContactsID
				AND [State].ContactItemTypeID = 13
		LEFT JOIN	[10.0.7.11,63693].FocusWrite.dbo.ContactItems ci_CompanyName
				ON o.OrgInfoContactsID = ci_CompanyName.ContactsID
				AND ci_CompanyName.ContactItemTypeID = 9
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.salesrep s (nolock) 
              ON l.salesrepid = s.salesrepid 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.contract fc (nolock) 
              ON fc.parentcontractid = c.contractid 
                 AND fc.contracttypeid = 2 -- Frame Data      
                 AND fc.active = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.contract e (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 5 -- EMS      
                 AND e.active = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.contract multi (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 8 -- Multi      
                 AND e.active = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.contract pdr (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 7 -- pdr      
                 AND e.active = 1 
       LEFT JOIN [10.0.7.11,63693].FocusWrite.dbo.contractlogins cl 
              ON cl.loginid = l.id 
       INNER JOIN [10.0.7.11,63693].FocusWrite.dbo.contract est (nolock) 
              ON est.contractid = cl.contractid 
                 AND est.contracttypeid = 1 -- est
				 AND (est.ExpirationDate - est.EffectiveDate) > 363
WHERE  c.contracttypeid = 1 
       AND c.primarylogin = 1 
       AND YEAR(c.expirationdate) = @year 
	   AND MONTH(c.ExpirationDate) = @month 
	   AND (c.ExpirationDate - c.EffectiveDate) > 363
       AND l.staffaccount <> 1 
	   AND l.DoubtfulAccount <> 1
       AND s.salesrepid IN (SELECT salesrepid		-- USE PE SalesRep filter
                            FROM   salesrep 
                            WHERE  salesrepid = CASE 
                                                  WHEN @SalesRepID > 0 THEN 
                                                  @SalesRepID 
                                                  ELSE salesrepid 
                                                END) 
GROUP  BY c.loginid, 
          c.willrenew, 
          c.contractid, 
          c.expirationdate, 
          c.notes, 
          s.salesrepid, 
          s.firstname, 
          s.lastname, 
		  ci_CompanyName.ItemText,
		  ci_FirstName.ItemText,
		  ci_LastName.ItemText,
		  [State].ItemText,
          fc.contractid, 
          e.contractid, 
          multi.contractid, 
          pdr.contractid, 
          p.pricelevel, 
          pNext.termtotal, 
          pCurrent.termtotal, 
          l.id,
		  l.ContactsID,
	  c.EffectiveDate
ORDER  BY c.expirationdate 

OPEN PE_CURSOR
FETCH NEXT FROM PE_CURSOR INTO 
		@loginid,  
		@salesrepid,  
		@salesrep,  
		@companyname,  
		@contact,  
		@phone,  
		@state,  
		@framedata,  
		@ems,  
		@multi,  
		@pdr,  
		@renewalamount,  
		@renewaldate,  
		@estcounttotal,  
		@estcountcur,  
		@yearswe,  
		@willrenew,  
		@willnotrenew,  
		@hasrenewed,  
		@pastdue,  
		@notes,
		@platform,
		@createDate
WHILE @@FETCH_STATUS = 0
BEGIN

	SELECT @existingRecordExists = COUNT(loginid)
	FROM	[dbo].[renewalreport] 
      WHERE  loginid = @loginId 
             AND renewaldate = @RenewalDate 
             AND [platform] = 'WE'

	IF @existingRecordExists > 0
	BEGIN

	PRINT 'UPDATE RECORD'

		UPDATE [dbo].[renewalreport]
	SET		estcounttotal = @estcounttotal, 
             estcountcur = @estcountcur, 
             hasrenewed = @hasRenewed, 
             pastdue = @PastDue,
			 [state] = @state
	WHERE  loginid = @loginId 
             AND renewaldate = @RenewalDate 
             AND [platform] = 'WE'

	--SELECT	loginid, 
 --            renewaldate, 
 --            estcounttotal, 
 --            estcountcur, 
 --            hasrenewed, 
 --            pastdue 
 --     FROM   [dbo].[renewalreport] 
 --     WHERE  loginid = @loginId 
 --            AND renewaldate = @RenewalDate 
 --            AND [platform] = 'WE' 

	--SELECT 
	--	@loginId AS 'loginId', 
	--	@RenewalDate AS 'RenewalDate', 
	--	@estcounttotal AS 'estcounttotal', 
	--	@estcountcur AS 'estcountcur', 
	--	@hasRenewed AS 'hasRenewed', 
	--	@PastDue AS 'PastDue'

	END
	ELSE
	BEGIN

	PRINT 'INSERT RECORD'
	INSERT INTO [renewalreport]  
            ([loginid],  
             [salesrepid],  
             [salesrep],  
             [companyname],  
             [contact],  
             [phone],  
             [state],  
             [framedata],  
             [ems],  
             [multi],  
             [pdr],  
             [renewalamount],  
             [renewaldate],  
             [estcounttotal],  
             [estcountcur],  
             [yearswe],  
             [willrenew],  
             [willnotrenew],  
             [hasrenewed],  
             [pastdue],  
             [notes],
			   [platform],
			   [createDate],
			   [active])  
	SELECT
		@loginid,  
		@salesrepid,  
		@salesrep,  
		@companyname,  
		@contact,  
		@phone,  
		@state,  
		@framedata,  
		@ems,  
		@multi,  
		@pdr,  
		@renewalamount,  
		@renewaldate,  
		@estcounttotal,  
		@estcountcur,  
		@yearswe,  
		0,  
		0,  
		@hasrenewed,  
		@pastdue,  
		@notes,
		@platform,
		@createDate,
		1

	END

	FETCH NEXT FROM PE_CURSOR INTO 
		@loginid,  
		@salesrepid,  
		@salesrep,  
		@companyname,  
		@contact,  
		@phone,  
		@state,  
		@framedata,  
		@ems,  
		@multi,  
		@pdr,  
		@renewalamount,  
		@renewaldate,  
		@estcounttotal,  
		@estcountcur,  
		@yearswe,  
		@willrenew,  
		@willnotrenew,  
		@hasrenewed,  
		@pastdue,  
		@notes,
		@platform,
		@createDate
END
CLOSE PE_CURSOR
DEALLOCATE PE_CURSOR




END
