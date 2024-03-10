SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- =============================================
-- Author:		T. Gibson
-- Create date: 6/14/2019
-- Description:	nightly job to update renewed 
--				status for renewal report table
-- =============================================
ALTER PROCEDURE [Admin].[UpdateRenewalReport_PE] 
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
SELECT c.loginid AS  'loginid', 
       s.salesrepid AS  'salesrepid', 
       s.firstname							           AS SalesRep, 
       Isnull(o.companyname, '')                       AS CompanyName, 
       ( Isnull(cp.firstname, '') + ' ' 
         + Isnull(cp.lastname, '') )                   AS Contact, 
       cp.phone1                                       AS Phone, 
       ca.state                                        AS State, 
       --c.contractid,   
       CONVERT(BIT, Isnull(fc.contractid, 0))          AS FrameData, 
       CONVERT(BIT, Isnull(e.contractid, 0))           AS EMS, 
       CONVERT(BIT, Isnull(multi.contractid, 0))       AS Multi, 
       CONVERT(BIT, Isnull(pdr.contractid, 0))         AS PDR, 
       Isnull(pNext.termtotal, pCurrent.termtotal)     AS RenewalAmount, 
       CONVERT(VARCHAR(10), c.expirationdate + 1, 101) AS RenewalDate, 
	   (SELECT [Admin].[Getlifetimeestimatesforlogin] (l.id)) AS 'estcounttotal' ,
	   (SELECT [Admin].[GetCurrentContractEstimatesForLogin] (l.id)) AS 'estcountcur' ,
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
       FROM   invoice 
       WHERE  LoginID = c.LoginID 
              AND paid = 0
			   AND DueDate <= GETDATE()
			   AND DueDate >= c.EffectiveDate)                AS PastDue, 
       c.notes,
	   'PE'											   AS 'Platform',
	   GETDATE()									   AS 'CreateDate'
FROM   vwcontractlogins c (nolock) 
       INNER JOIN logins l (nolock) 
               ON c.loginid = l.id 
                  AND l.doubtfulaccount = 0 
       INNER JOIN contractpricelevels p (nolock) 
               ON c.contractpricelevelid = p.contractpricelevelid 
                  AND p.pricelevel >= 0 
       LEFT JOIN vwcontractpricelevelterms pCurrent 
              ON p.contracttermid = pCurrent.contracttermid 
                 AND pCurrent.pricelevel = p.pricelevel 
                 AND pCurrent.termactive = 1 
                 AND pCurrent.pricelevelactive = 1 
       LEFT JOIN vwcontractpricelevelterms pNext 
              ON p.contracttermid = pNext.previouscontracttermid 
                 AND pNext.pricelevel = p.pricelevel 
                 AND pNext.termactive = 1 
                 AND pNext.pricelevelactive = 1 
       LEFT JOIN organizationinfo o (nolock) 
              ON l.organizationid = o.id 
       LEFT JOIN tbl_contactperson cp (nolock) 
              ON cp.contactid = l.contactsid 
       LEFT JOIN tbl_address ca (nolock) 
              ON ca.contactsid = o.OrgInfoContactsID 
       LEFT JOIN salesrep s (nolock) 
              ON l.salesrepid = s.salesrepid 
       LEFT JOIN contract fc (nolock) 
              ON fc.parentcontractid = c.contractid 
                 AND fc.contracttypeid = 2 -- Frame Data      
                 AND fc.active = 1 
       LEFT JOIN contract e (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 5 -- EMS      
                 AND e.active = 1 
       LEFT JOIN contract multi (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 8 -- Multi      
                 AND e.active = 1 
       LEFT JOIN contract pdr (nolock) 
              ON e.parentcontractid = c.contractid 
                 AND e.contracttypeid = 7 -- pdr      
                 AND e.active = 1 
       LEFT JOIN contractlogins cl 
              ON cl.loginid = l.id 
       INNER JOIN contract est (nolock) 
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
       AND s.salesrepid IN (SELECT salesrepid 
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
          companyname, 
          cp.firstname, 
          cp.lastname, 
          cp.phone1, 
          state, 
          fc.contractid, 
          e.contractid, 
          multi.contractid, 
          pdr.contractid, 
          p.pricelevel, 
          pNext.termtotal, 
          pCurrent.termtotal, 
          l.id,
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
             AND [platform] = 'PE'

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
             AND [platform] = 'PE'

	--SELECT	loginid, 
 --            renewaldate, 
 --            estcounttotal, 
 --            estcountcur, 
 --            hasrenewed, 
 --            pastdue 
 --     FROM   [dbo].[renewalreport] 
 --     WHERE  loginid = @loginId 
 --            AND renewaldate = @RenewalDate 
 --            AND [platform] = 'PE' 

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
			   [Active])  
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
GO
