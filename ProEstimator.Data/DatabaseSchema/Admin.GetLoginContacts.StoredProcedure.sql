USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [Admin].[GetLoginContacts] @LoginID     INT = NULL,
                                           @LoginName   VARCHAR(50) = NULL,
                                           @FirstName   VARCHAR(255) = NULL,
                                           @LastName    VARCHAR(255) = NULL,
                                           @CompanyName VARCHAR(255) = NULL
AS
    SET TRANSACTION isolation level READ uncommitted
    SET nocount ON

	;WITH contacts_cte
         AS (SELECT l.id                                       AS LoginID,
                    l.contactsid,
                    o.orginfocontactsid                        AS
                    OrgInfoContactsID,
                    Isnull(l.loginname, '')                    AS LoginName,
                    Isnull(l.organization, '')                 AS
                    LoginOrganization,
                    Isnull(person.firstname, '')               AS FirstName,
                    Isnull(person.lastname, '')                AS LastName,
                    ( Isnull(person.firstname, '') + ' '
                      + Isnull(person.lastname, '') )          AS FullName,
                    ( Isnull(LEFT(person.firstname, 1), '')
                      + Isnull(LEFT(person.lastname, 1), '') ) AS
                    CustomerInitials,
                    o.companyname                              AS CompanyName
             FROM   logins l (nolock)
                    LEFT JOIN tbl_contactperson person (nolock)
                           ON l.contactsid = person.contactid
                    LEFT JOIN organizationinfo o (nolock)
                           ON l.organizationid = o.id
             WHERE  l.contactsid IS NOT NULL
                    AND l.id = Isnull(@LoginID, l.id))
    SELECT c.loginid,
           c.contactsid,
           c.orginfocontactsid,
           c.loginname,
           c.loginorganization,
           c.firstname,
           c.lastname,
           c.fullname,
           c.customerinitials,
           c.companyname,
           Isnull(person.title, '')              AS JobTitle,
           Isnull([address].address1, '')        AS Address1,
           Isnull([address].address2, '')        AS Address2,
           Isnull([address].city, '')            AS City,
           Isnull([address].[state], '')         AS State,
           Isnull([address].zip, '')             AS Zip,
           Isnull(person.phone1, '')             AS Phone,
           Isnull(person.phonenumbertype1, 'WF') AS PhoneCode,
           Isnull(person.phone2, '')             AS Fax,
           Isnull(person.phonenumbertype2, 'FX') AS FaxCode,
           Isnull(person.email, '')              AS Email
    FROM   contacts_cte c with(nolock)
           LEFT JOIN tbl_address [address] (nolock)
                  ON c.orginfocontactsid = [address].contactsid
           LEFT JOIN tbl_contactperson person (nolock)
                  ON c.contactsid = person.contactid
    WHERE  c.loginid IN (SELECT loginid
                         FROM   contacts_cte with(nolock)
                         WHERE  firstname LIKE Isnull(@FirstName, '') + '%'
                                AND lastname LIKE Isnull(@LastName, '') + '%'
                                AND loginname = COALESCE(@LoginName, loginname,
                                                '')
                                AND ( loginorganization LIKE
                                      Isnull(@CompanyName, ''
                                      ) + '%'
                                       OR Isnull(companyname, '') LIKE
                                          Isnull( @CompanyName, '') + '%' )
                        )
    ORDER  BY c.lastname,
              c.firstname  



PRINT N'Altering [dbo].[InsertSalesBoard]'
GO
