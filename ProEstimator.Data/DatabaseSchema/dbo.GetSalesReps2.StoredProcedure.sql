USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE PROCEDURE [dbo].[GetSalesReps2] 
  @SalesRepID INT = NULL 
AS 
  -- Reset @SalesRepID if it is 0 (Admin) 
  IF @SalesRepID = 0 
  SET @SalesRepID = NULL 
  -- Open encryption key 
  OPEN symmetric key webestencryptionkey decryption BY certificate webestencryptioncertificate ;
  WITH salesreps AS 
  ( 
         SELECT ''                                         AS salesrepid , 
                ''                                         AS salesnumber , 
                'Please'                                   AS firstname , 
                'Choose'                                   AS lastname , 
                'Please Choose'                            AS fullname , 
                ('-' + ' - ' + 'Please' + ' ' + 'Choose' ) AS fulldescription , 
                ''                                         AS email , 
                ''                                         AS username , 
                ''                                         AS password 
         UNION ALL 
         SELECT salesrepid, 
                salesnumber, 
                firstname, 
                lastname, 
                ( firstname   + ' ' + lastname )                     AS fullname, 
                ( salesnumber + ' - ' + firstname + ' ' + lastname ) AS fulldescription, 
                email, 
                username, 
                CONVERT(VARCHAR, Decryptbykey(password)) AS password 
         FROM   salesrep with(nolock)
         WHERE  salesrepid = Isnull(@SalesRepID, salesrepid) 
                --AND   SalesRepID > 0 -- Omit Admin 
         AND    deleted = 0 ) 
  SELECT 
         CASE 
                WHEN salesnumber = '' THEN -1 
                ELSE salesrepid 
         END AS salesrepid, 
         salesnumber, 
         firstname, 
         lastname, 
         fullname, 
         fulldescription, 
         email, 
         username, 
         password 
  FROM   salesreps
GO
