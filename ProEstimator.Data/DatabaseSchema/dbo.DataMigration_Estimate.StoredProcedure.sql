USE [FocusWrite]
GO
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
-- Stored Procedure

-- =============================================
-- Author:		<Author,,Name>
-- Create date: <Create Date,,>
-- Description:	<Description,,>
-- =============================================
CREATE PROCEDURE [dbo].[DataMigration_Estimate]
	@AdminInfoID		int
AS
BEGIN


	/*
		If the database to import data from is in another server, first set up a Linked Server.
			In the object explorer for this server, open Server Objects and add to Linked Servers.  See google for help.

		It doesn't seem possible to pass a server name and database name as parameters to a stored procedure, so before running this make sure the server name/database name are correct.
		Tables in the external database are referenced out like [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Logins 
		Before running the query, do a text find/replace to change [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite to whatever the actual server and database name are

	*/


	DECLARE @time DATETIME = GETDATE()
	print '#### Starting...'
		DECLARE @LoginID INT
		SET @LoginID = (SELECT CreatorID FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo WHERE id = @AdminInfoID)

		DECLARE @EstimationDataID INT
		SET @EstimationDataID = (select id from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData  where admininfoid = @AdminInfoID)

			
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Get a copy of the EstimationLineItems table from WebEst so it only has to be queried once
	print '#### make temp line table...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @WebEstLineItems TABLE
	(
		[id] [int] NOT NULL,
		[EstimationDataID] [int] NOT NULL,
		[StepID] [int] NULL,
		[PartNumber] [varchar](50) NULL,
		[PartSource] [varchar](10) NULL,
		[ActionCode] [varchar](20) NULL,
		[Description] [varchar](80) NULL,
		[Price] [money] NULL,
		[Qty] [tinyint] NULL,
		[LaborTime] [real] NULL,
		[PaintTime] [real] NULL,
		[Other] [real] NULL,
		[ImageID] [int] NULL,
		[ActionDescription] [varchar](80) NULL,
		[PartOfOverhaul] [bit] NULL,
		[PartSourceVendorID] [int] NULL,
		[BettermentPartsFlag] [bit] NULL,
		[SubletPartsFlag] [bit] NULL,
		[BettermentOperationFlag] [bit] NULL,
		[SubletOperationFlag] [bit] NULL,
		[SupplementVersion] [tinyint] NULL,
		[LineNumber] [int] NULL,
		[UniqueSequenceNumber] [int] NULL,
		[ModifiesID] [int] NULL,
		[ACDCode] [char](1) NULL,
		[BettermentPercentage] [real] NULL,
		[CustomerPrice] [real] NULL,
		[AutomaticCharge] [bit] NULL,
		[SourcePartNumber] [varchar](25) NULL,
		[SectionID] [int] NULL,
		[VehiclePosition] [varchar](5) NULL,
		[Barcode] [varchar](10) NULL,
		[dbPrice] [money] NULL,
		[SourceVendorContactsID] [int] NULL,
		[AutoAdd] [bit] NULL,
		[Suppress] [bit] NULL,
		[AutoAddBarcodeParent] [varchar](10) NULL,
		[Date_Entered] [datetime] NOT NULL
	)

	INSERT INTO @WebEstLineItems
	SELECT 
		[id],
		[EstimationDataID],
		[StepID],
		[PartNumber],
		[PartSource],
		[ActionCode],
		[Description],
		[Price],
		[Qty],
		[LaborTime],
		[PaintTime],
		[Other],
		[ImageID],
		[ActionDescription],
		[PartOfOverhaul],
		[PartSourceVendorID],
		'',--[BettermentPartsFlag],
		[SubletPartsFlag],
		'',--[BettermentOperationFlag],
		[SubletOperationFlag],
		[SupplementVersion],
		[LineNumber],
		[UniqueSequenceNumber],
		[ModifiesID],
		[ACDCode],
		0,--[BettermentPercentage],
		[CustomerPrice],
		[AutomaticCharge],
		[SourcePartNumber],
		[SectionID],
		[VehiclePosition],
		[Barcode],
		[dbPrice],
		0,--[SourceVendorContactsID],
		[AutoAdd],
		[Suppress],
		[AutoAddBarcodeParent],
		[Date_Entered]
	FROM  [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationLineItems  
	where EstimationLineItems.EstimationDataID = @EstimationDataID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Make a copy of the EstimationLineItems for ProE.  
	-- This table will be filled after the line items are copied from WebEst into ProE so the huge ProE table doesn't have to be queried.
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @ProEEstimationLineItems TABLE
	(
		[id] [int]NOT NULL,
		[EstimationDataID] [int] NOT NULL,
		[StepID] [int] NULL,
		[PartNumber] [varchar](50) NULL,
		[PartSource] [varchar](10) NULL,
		[ActionCode] [varchar](20) NULL,
		[Description] [varchar](80) NULL,
		[Price] [money] NULL,
		[Qty] [int] NULL,
		[LaborTime] [real] NULL,
		[PaintTime] [real] NULL,
		[Other] [real] NULL,
		[ImageID] [int] NULL,
		[ActionDescription] [varchar](80) NULL,
		[PartOfOverhaul] [bit] NULL,
		[PartSourceVendorID] [int] NULL,
		[BettermentParts] [bit] NULL,
		[SubletPartsFlag] [bit] NULL,
		[BettermentMaterials] [bit] NULL,
		[SubletOperationFlag] [bit] NULL,
		[SupplementVersion] [tinyint] NULL,
		[LineNumber] [int] NULL,
		[UniqueSequenceNumber] [int] NULL,
		[ModifiesID] [int] NULL,
		[ACDCode] [char](1) NULL,
		[CustomerPrice] [real] NULL,
		[AutomaticCharge] [bit] NULL,
		[SourcePartNumber] [varchar](25) NULL,
		[SectionID] [int] NULL,
		[VehiclePosition] [varchar](5) NULL,
		[Barcode] [varchar](10) NULL,
		[dbPrice] [money] NULL,
		[AutoAdd] [bit] NULL,
		[Suppress] [bit] NULL,
		[AutoAddBarcodeParent] [varchar](10) NULL,
		[Date_Entered] [datetime] NOT NULL,
		[BettermentType] [char](10) NULL,
		[BettermentValue] [float] NULL,
		[IsPartsQuantity] [bit] NULL,
		[IsLaborQuantity] [bit] NULL,
		[IsPaintQuantity] [bit] NULL,
		[PresetShellID] [int] NULL,
		[ParentLineID] [int] NULL
	)


	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Copy the AdminInfo record.  This turns on IDENTITY_INSERT so the same ID can be copied from WebEst
	print '#### Inserting into AdminInfo...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	SET IDENTITY_INSERT AdminInfo on
	INSERT INTO AdminInfo
				(id
				,[CreatorID]
				,[Description]
				,[CustomerProfilesID]
				,[GrandTotal]
				,[BettermentTotal]
				,[EstimateNumber]
				,[WorkOrderNumber]
				,[PrintDescription]
				,[Archived]
				,[Deleted]
				,[ClaimNumber]
				,[LastView]
				,[IsImported])
	SELECT AdminInfo.ID, CreatorID, Description, CustomerProfilesID, GrandTotal, BettermentTotal, EstimateNumber, WorkOrderNumber, PrintDescription, Archived, Deleted, '', EstimationData.EstimationDate, 1
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo  
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData  ON EstimationData.AdminInfoID = AdminInfo.ID 
	WHERE AdminInfo.id = @AdminInfoID

	SET IDENTITY_INSERT AdminInfo off

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Copy the EstimationData record, copies the same EstimationDataID
	print '#### Inserting into EstimationData...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	SET IDENTITY_INSERT EstimationData ON

	-- First do a direct copy from the old database, leaving out new fields
	INSERT INTO EstimationData
	(
		id
		,[AdminInfoID]
		,[EstimationDate]
		,[DateOfLoss]
		,[CoverageType]
		,[EstimateLocation]
		,[TransactionLevel]
		,[LockLevel]
		,[LastLineNumber]
		,[EstimationLineItemIDLocked]
		,[Note]
		,[PrintNote]
		,[AssignmentDate]
		,[ReportTextHeader]
		,[AlternateIdentitiesID]
		,[SupplementVersion]
		,[NextUniqueSequenceNumber]
	)
	SELECT TOP 1
		  ed.id
		, ed.AdminInfoID
		, ed.EstimationDate
		, CAST(ContactItems.ItemText AS DATETIME)
		, ed.CoverageType
		, ed.EstimateLocation
		, ed.TransactionLevel
		, ed.LockLevel
		, ed.LastLineNumber
		, ed.EstimationLineItemIDLocked
		, ed.Note
		, ed.PrintNote
		, ed.AssignmentDate
		, ed.ReportTextHeader
		, ed.AlternateIdentitiesID
		, 
		(
			SELECT MAX(EstimationLineItems.SupplementVersion) As SupplementVersion
			FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData  
			JOIN @WebEstLineItems AS EstimationLineItems   ON EstimationLineItems.EstimationDataID = EstimationData.ID
			WHERE EstimationData.AdminInfoID = @AdminInfoID
		)
		, ed.NextUniqueSequenceNumber
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData ed  

	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoToContacts ON AdminInfoToContacts.AdminInfoID = @AdminInfoID
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts ON AdminInfoToContacts.ContactsID = Contacts.id AND Contacts.ContactTypeID = 1 AND Contacts.ContactSubTypeID = 31
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ON ContactItems.ContactsID = Contacts.id AND ContactItemTypeID = 57

	WHERE ed.id = @EstimationDataID
		--AND Contacts.ContactTypeID = 1 AND Contacts.ContactSubTypeID = 31 AND ContactItemTypeID = 57

	SET IDENTITY_INSERT EstimationData off

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Updated the copied EstimationData record with the new fields that weren't in WebEst
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Updating EstimationData extra information...'

	UPDATE EstimationData 
	SET 
		  Deductible = (CASE WHEN ISNUMERIC(ciDeductible.ItemText) = 1 THEN CAST(ISNULL(ciDeductible.ItemText, 0) AS MONEY) ELSE 0 END)
		, InsuranceCompanyName = ciInsuranceCompanyName.ItemText 
		, ClaimantSameAsOwner = CAST(ISNULL(ciClaimantSameAs.ItemText, 0) AS BIT)
		, InsuredSameAsOwner = CAST(ISNULL(ciInsuredSameAs.ItemText, 0) AS BIT)
		, EstimatorID = EstimatorsData.EstimatorID

	FROM AdminInfo  
	JOIN EstimationData ed  ON AdminInfo.id = ed.AdminInfoID

	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoToContacts atc   ON ed.AdminInfoID = atc.AdminInfoID 
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts OtherInfo  ON atc.ContactsID = OtherInfo.id AND OtherInfo.ContactTypeID = 1 AND OtherInfo.ContactSubTypeID = 31 -- Other Info
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciDeductible ON ciDeductible.ContactsID = OtherInfo.id AND ciDeductible.ContactItemTypeID = 40			-- Deductable

	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts InsuranceCompanyInfo  ON atc.ContactsID = InsuranceCompanyInfo.id AND OtherInfo.ContactTypeID = 1 AND InsuranceCompanyInfo.ContactSubTypeID = 5		-- Insurance Company
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciInsuranceCompanyName  ON ciInsuranceCompanyName.ContactsID = InsuranceCompanyInfo.id AND ciInsuranceCompanyName.ContactItemTypeID = 9 -- Insurance Company Name

	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts ClaimantPerson  ON atc.ContactsID = ClaimantPerson.id AND OtherInfo.ContactTypeID = 1 AND ClaimantPerson.ContactSubTypeID = 3								-- Claimant
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciClaimantSameAs  ON ciClaimantSameAs.ContactsID = ClaimantPerson.id AND ciClaimantSameAs.ContactItemTypeID = 28	-- Same As Owner

	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts InsuredPerson  ON atc.ContactsID = InsuredPerson.id AND OtherInfo.ContactTypeID = 1 AND InsuredPerson.ContactSubTypeID = 2									-- Insured
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciInsuredSameAs  ON ciInsuredSameAs.ContactsID = InsuredPerson.id AND ciInsuredSameAs.ContactItemTypeID = 28		-- Same As Owner

	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts Estimator  ON atc.ContactsID = Estimator.id AND OtherInfo.ContactTypeID = 1 AND InsuredPerson.ContactSubTypeID = 23
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciEstimatorFirst  ON ciEstimatorFirst.ContactsID = Estimator.id AND ciEstimatorFirst.ContactItemTypeID = 2
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciEstimatorLast  ON ciEstimatorLast.ContactsID = Estimator.id AND ciEstimatorLast.ContactItemTypeID = 6
		LEFT OUTER JOIN EstimatorsData  ON EstimatorsData.AuthorFirstName = ciEstimatorFirst.ItemText AND EstimatorsData.AuthorLastName = ciEstimatorLast.ItemText

	WHERE AdminInfo.id = @AdminInfoID



	UPDATE EstimationData 
	SET RepairFacilityVendorID = 
	(
		SELECT Vendor.ID
		FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData ed
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoToContacts atc   ON ed.AdminInfoID = atc.AdminInfoID 

		-- Repair Facility
		JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts RepairFacilityContact ON atc.ContactsID = RepairFacilityContact.ID AND RepairFacilityContact.ContactTypeID = 2 AND RepairFacilityContact.ContactSubTypeID = 26
			LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciRepairFacility ON RepairFacilityContact.ID = ciRepairFacility.ContactsID AND ciRepairFacility.ContactItemTypeID = 9
			LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItemTypes ON ciRepairFacility.ContactItemTypeID = ContactItemTypes.id

		LEFT OUTER JOIN Vendor ON ciRepairFacility.ItemText = Vendor.CompanyName AND Vendor.LoginsID = @LoginID
		WHERE ed.AdminInfoID = @AdminInfoID
	)
	WHERE EstimationData.AdminInfoID = @AdminInfoID


	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Update the Policy Number in the ProE EstimationData table
	print '#### Inserting into EstimationData PolicyNumber...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	UPDATE EstimationData 
	SET 
		PolicyNumber = ciPolicyNumber.ItemText
	FROM AdminInfo  
	JOIN EstimationData ed  ON AdminInfo.id = ed.AdminInfoID
	LEFT JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoToContacts atc  ON ed.AdminInfoID = atc.AdminInfoID 
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts AS OtherInfo  ON atc.ContactsID = OtherInfo.id AND OtherInfo.ContactTypeID = 2 AND OtherInfo.ContactSubTypeID = 31 -- Other Info
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciPolicyNumber  ON ciPolicyNumber.ContactsID = OtherInfo.id AND ciPolicyNumber.ContactItemTypeID = 23	-- Policy Number
	WHERE AdminInfo.id = @AdminInfoID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting into EstimationData ClaimNumber...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	UPDATE EstimationData
	SET ClaimNumber = ciClaimNumber.ItemText
	FROM AdminInfo  
	JOIN EstimationData ed  ON AdminInfo.id = ed.AdminInfoID
	LEFT JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoToContacts atc  ON ed.AdminInfoID = atc.AdminInfoID 
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts AS OtherInfo  ON atc.ContactsID = OtherInfo.id AND OtherInfo.ContactTypeID = 2 AND OtherInfo.ContactSubTypeID = 31 -- Other Info
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems ciClaimNumber  ON ciClaimNumber.ContactsID = OtherInfo.id AND ciClaimNumber.ContactItemTypeID = 29	-- Claim Number
	WHERE AdminInfo.id = @AdminInfoID


	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Copy the EstimationLineItems table from WebEst
	-- The lines in ProE will have new auto incremented IDs.  The ImageID field stores the WebEst line ID
	print '#### Inserting into EstimationLineItems...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO EstimationLineItems
	(
		[EstimationDataID]
		,[StepID]
		,[PartNumber]
		,[PartSource]
		,[ActionCode]
		,[Description]
		,[Price]
		,[Qty]
		,[LaborTime]
		,[PaintTime]
		,[Other]
		,[ImageID]
		,[ActionDescription]
		,[PartOfOverhaul]
		,[PartSourceVendorID]
		,[BettermentParts]
		,[SubletPartsFlag]
		,[SubletOperationFlag]
		,[SupplementVersion]
		,[LineNumber]
		,[UniqueSequenceNumber]
		,[ModifiesID]
		,[ACDCode]
		,[CustomerPrice]
		,[AutomaticCharge]
		,[SourcePartNumber]
		,[SectionID]
		,[VehiclePosition]
		,[Barcode]
		,[dbPrice]
		,[AutoAdd]
		,[Suppress]
		,[AutoAddBarcodeParent]
		,[Date_Entered]
		,[BettermentType]
		,[BettermentValue])
	select 		
		[EstimationDataID]
		,[StepID]
		,[PartNumber]
		,[PartSource]
		,[ActionCode]
		,[Description]
		,[Price]
		,dbo.GetLineItemQuantity_WebEst(eli.id)
		,[LaborTime]
		,[PaintTime]
		,[Other]
		,eli.[ID]					-- Copy the old ID into the ImageID field (which isn't used) to link old to new
		,[ActionDescription]
		,[PartOfOverhaul]
		,[PartSourceVendorID]
		,[BettermentPartsFlag]
		,[SubletPartsFlag]
		,[SubletOperationFlag]
		,eli.[SupplementVersion]
		,[LineNumber]
		,[UniqueSequenceNumber]
		,[ModifiesID]
		,[ACDCode]
		,[CustomerPrice]
		,[AutomaticCharge]
		,[SourcePartNumber]
		,[SectionID]
		,[VehiclePosition]
		,[Barcode]
		,[dbPrice]
		,[AutoAdd]
		,[Suppress]
		,[AutoAddBarcodeParent]
		,[Date_Entered]
		,CASE WHEN ISNULL(BettermentPercentage, 0) > 0 THEN 'P' ELSE '' END
		,[BettermentPercentage]
	FROM  @WebEstLineItems AS eli 
	ORDER BY eli.id

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();	

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Update the Modifies ids with the new ProE line IDs
	print '#### Update Modifies Ids...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	DECLARE @ModifiedLines TABLE
	(
		LineItemID      INT
		, ModifiesID	INT
	)

	INSERT INTO @ModifiedLines
	SELECT EstimationLineItems.id, Modified.ID
	FROM EstimationLineItems 
	LEFT OUTER JOIN EstimationLineItems Modified  ON EstimationLineItems.ModifiesID = Modified.ImageID
	WHERE EstimationLineItems.EstimationDataID = @EstimationDataID

	UPDATE EstimationLineItems
	SET EstimationLineItems.ModifiesID = ModifiedLines.ModifiesID
	FROM @ModifiedLines ModifiedLines
	WHERE EstimationLineItems.id = ModifiedLines.LineItemID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();	

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Ezra - 5/2/2019
	-- The overlap calculations come up differently in ProE than in Web Est.
	-- Get the deductions calculated by webest by calling the report stored procedure and storing the data in a temp #Line table

	print '#### Filling #Line table...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	create table  #Line (Step varchar(200)
		,Description varchar(200)
		,PartNumber varchar(50)
		,Price varchar(50)
		,Qty int
		,PartSource varchar(20)
		,AdminInfoId int
		, LineId int
		, ModifiedID int
		, LaborLineItems varchar(250)
		,Printed varchar(250)
		,Notes varchar(5000)
		, BettermentOperationFlag bit
		, BettermentPartsFlag bit
		, SubletOperationFlag bit
		,SubletPartsFlag bit
		, LineNumber int
		,ModifiesLineNumber int
		,Locked int
		, Modified Int
		, SupplementVersion varchar(50)
		, Location int
		, StepSupp varchar(150)
		, LaborCost varchar(50)
		, LaborTime varchar(150)
		,PaintCost varchar(50)
		, Labor_Cost varchar(50)
		, PaintTime varchar(50)
		, OtherCost varchar(50)
		,LaborLineItemsTotal varchar(50)
		, OverlapAmount varchar(100)
		,a int
		,b int
		,c int
		,d int
		,Line_Number int
	)

	insert into #Line
	EXECUTE [WEB-EST-PROE\WEBESTARCHIVE].Mitchell3.dbo.App_Reports @Report = 'LineItemsFullReport', @AdminInfoID = @AdminInfoID, @PrintPrivateNotes = 0, @PrintPublicNotes = 0

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Update the EstimationLineItems table's LaborTime field to store the overlapped hours calculated by WebEst 
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Updating EstimationLineItems...'

	UPDATE EstimationLineItems
	SET 
		EstimationLineItems.LaborTime = CASE WHEN Line.LaborTime = 'Included' THEN -99
		ELSE CAST(ISNULL(REPLACE(REPLACE(REPLACE(Line.OverlapAmount, ' hrs. Overlap', ''), 'Overlap', ''), '$', ''), '0') AS REAL)
		END
	FROM  @WebEstLineItems AS EstimationLineItemsWebEst
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationLineLabor ON EstimationLineItemsWebEst.ID = EstimationLineLabor.EstimationLineItemsID
	LEFT OUTER JOIN #Line Line ON EstimationLineItemsWebEst.id = Line.LineId 
	LEFT OUTER JOIN EstimationLineItems ON EstimationLineItemsWebEst.ID = EstimationLineItems.ImageID
	WHERE  EstimationLineLabor.LaborType IN (1, 3, 4)

	DROP TABLE #Line

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Fill the #ProEEstimationLineItems temp table with a copy of the new line items in the ProE database.  This is to speed up queries that use this data.
	print '#### Filling @ProEEstimationLineItems...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO @ProEEstimationLineItems
	SELECT [id],
		[EstimationDataID],
		[StepID],
		[PartNumber],
		[PartSource],
		[ActionCode],
		[Description],
		[Price],
		[Qty],
		[LaborTime],
		[PaintTime],
		[Other],
		[ImageID],
		[ActionDescription],
		[PartOfOverhaul],
		[PartSourceVendorID],
		[BettermentParts],
		[SubletPartsFlag],
		[BettermentMaterials],
		[SubletOperationFlag],
		[SupplementVersion],
		[LineNumber],
		[UniqueSequenceNumber],
		[ModifiesID],
		[ACDCode],
		[CustomerPrice],
		[AutomaticCharge],
		[SourcePartNumber],
		[SectionID],
		[VehiclePosition],
		[Barcode],
		[dbPrice],
		[AutoAdd],
		[Suppress],
		[AutoAddBarcodeParent],
		[Date_Entered],
		[BettermentType],
		[BettermentValue],
		[IsPartsQuantity],
		[IsLaborQuantity],
		[IsPaintQuantity],
		[PresetShellID],
		[ParentLineID]
	FROM EstimationLineItems
	WHERE EstimationDataID = @EstimationDataID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- The EstimationData table records the last line item ID in the last supplement.  Get the ProE line item ID and update the EstimationData table
	print '#### Updating EstimationLineItemIDLocked...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	DECLARE @newLockedID INT = 
	(
		SELECT ISNULL(EstimationLineItems.id, 0)
		FROM EstimationData  
		LEFT OUTER JOIN @ProEEstimationLineItems AS EstimationLineItems  ON EstimationLineItems.ImageID = EstimationData.EstimationLineItemIDLocked
		WHERE EstimationData.AdminInfoID = @AdminInfoID
	)

	if (@newLockedID > 0)
		UPDATE EstimationData
		SET EstimationLineItemIDLocked = @newLockedID
		WHERE id = @EstimationDataID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Copy the EstimationLineLabor lines from WebEst. This will generate a new ID, the WebEst ID is stored in a new WebEstID field in ProE
	print '#### Inserting into EstimationLineLabor...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO EstimationLineLabor
	(
			[WebEstID]
		,[EstimationLineItemsID]
		,[LaborType]
		,[LaborSubType]
		,[LaborTime]
		,[LaborCost]
		,[BettermentFlag]
		,[SubletFlag]
		,[UniqueSequenceNumber]
		,[ModifiesID]
		,[AdjacentDeduction]
		,[MajorPanel]
		,[BettermentPercentage]
		,[dbLaborTime]
		,[AdjacentDeductionLock]
		,[barcode]
		,[Lock]
		,[include]
	)
	select 
			EstimationLineLabor.ID
		,EstimationLineItems.ID
		,[LaborType]
		,[LaborSubType]
		,EstimationLineLabor.LaborTime
		,[LaborCost]
		,[BettermentFlag]
		,[SubletFlag]
		,EstimationLineLabor.UniqueSequenceNumber
		,EstimationLineLabor.ModifiesID
		,[AdjacentDeduction]
		,[MajorPanel]
		,[BettermentPercentage]
		,[dbLaborTime]
		,[AdjacentDeductionLock]
		,EstimationLineLabor.barcode
		,[Lock]
		, 1
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationLineLabor  
	LEFT OUTER JOIN @ProEEstimationLineItems AS EstimationLineItems  ON EstimationLineItems.ImageID = EstimationLineLabor.EstimationLineItemsID
	where EstimationLineItemsID in 
	(
		select id
		from @WebEstLineItems AS EstimationLineItems
		where estimationdataid in 
		(
			select id 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData  
			where admininfoid = @AdminInfoID 
		)
	)

	 print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting into EstimationOverlap...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO EstimationOverlap
	(
			[EstimationLineItemsID1]
		,[EstimationLineItemsID2]
		,[OverlapAdjacentFlag]
		,[Amount]
		,[SectionOverlapsID]
		,[Minimum]
		,[UserOverride]
		,[UserAccepted]
		,[UserResponded]
		,[SupplementLevel]
	)
	select 
			ELI1.ID
		, ELI2.ID
		, OverlapAdjacentFlag
		, Amount
		, SectionOverlapsID
		, Minimum
		, UserOverride
		, UserAccepted
		, UserResponded
		, SupplementLevel
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationOverlap  
	LEFT OUTER JOIN @ProEEstimationLineItems AS ELI1   ON EstimationOverlap.EstimationLineItemsID1 = ELI1.ImageID
	LEFT OUTER JOIN @ProEEstimationLineItems AS ELI2   ON EstimationOverlap.EstimationLineItemsID2 = ELI2.ImageID
	where EstimationLineItemsID1 in (select id from @WebEstLineItems)

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting into EstimationNotes...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO EstimationNotes
	(
			[EstimationLineItemsID]
		,[Printed]
		,[Notes]
	)  
	select 
			EstimationLineItems.ID
		, Printed
		, Notes
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationNotes  
	LEFT OUTER JOIN @ProEEstimationLineItems AS EstimationLineItems   ON EstimationNotes.EstimationLineItemsID = EstimationLineItems.ImageID
	where EstimationLineItemsID in
	(
		select id
		from @WebEstLineItems AS EstimationLineItems
		where estimationdataid in 
		(
			select id 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData  
			where admininfoid = @AdminInfoID 
		)
	)

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting into VehicleInfo...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO VehicleInfo
	(
		 [EstimationDataId]
		,[VehicleID]
		,[AdditionalOptions]
		,[Color]
		,[ColorCode]
		,[MilesIn]
		,[MilesOut]
		,[License]
		,[State]
		,[Condition]
		,[ExtColor]
		,[ExtColorCode]
		,[IntColor]
		,[IntColorCode]
		,[TrimLevel]
		,[Vin]
		,[VinDecode]
		,[InspectionDate]
		,[ExtColorCodeChar]
		,[IntColorCodeChar]
		,[ProductionDate]
		,[BodyType]
		,[Service_Barcode]
		,[DefaultPaintType]
		,[DriveType]
		,[VehicleValue]
		,[Year]
		,[MakeID]
		,[ModelID]
		,[SubModelID]
		,[EngineType]
		,[TransmissionType]
		,[paintcode]
		,[POI_ID]
	)
	SELECT 
		 vi.[EstimationDataId]
		,vi.[VehicleID]
		,vi.[AdditionalOptions]
		,vi.[Color]
		,vi.[ColorCode]
		,vi.[MilesIn]
		,vi.[MilesOut]
		,vi.[License]
		,vi.[State]
		,vi.[Condition]
		,vi.[ExtColor]
		,vi.[ExtColorCode]
		,vi.[IntColor]
		,vi.[IntColorCode]
		,vi.[TrimLevel]
		,vi.[Vin]
		,vi.[VinDecode]
		,vi.[InspectionDate]
		,vi.[ExtColorCodeChar]
		,vi.[IntColorCodeChar]
		--,SUBSTRING(UPPER(DATENAME(month, vi.[ProductionDate])), 0, 4) + '-' + CAST(YEAR(vi.[ProductionDate]) AS VARCHAR(10))
		,vi.[ProductionDate]
		,vi.[BodyType]
		,vi.[Service_Barcode]
		,CASE WHEN vi.[DefaultPaintType] = 9 THEN 19 ELSE vi.[DefaultPaintType] END AS [DefaultPaintType]
		,vi.[DriveType]
		,vi.[VehicleValue]
		,vi.[Year]
		,vi.[MakeID]
		,vi.[ModelID]
		,vi.[SubModelID]
		,vi.[EngineType]
		,vi.[TransmissionType]
		,vi.[paintcode]
		,vi.[POI_ID]
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData ed   
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.VehicleInfo vi  ON vi.EstimationDataID = ed.id 
	WHERE ed.AdminInfoID = @AdminInfoID

	DECLARE @vehicleInfoID int = (SELECT CAST(SCOPE_IDENTITY() AS INT))

	DECLARE @vinnXrefMake INT = (
		SELECT TOP 1 x.makeId 
		FROM Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
		where e.AdminInfoID = @AdminInfoID
	)

	DECLARE @vinnXrefModel INT = (
		SELECT TOP 1 x.ModelID 
		FROM Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
		where e.AdminInfoID = @AdminInfoID
	)

	DECLARE @vinnXrefYear INT = (
		SELECT TOP 1 x.VinYear 
		FROM Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
		where e.AdminInfoID = @AdminInfoID
	)

	DECLARE @vinnXrefSubModel INT = (
		SELECT TOP 1 x.SubmodelID 
		FROM Focuswrite.dbo.EstimationData as e with(nolock)
		inner join Focuswrite.dbo.VehicleInfo as v with(nolock)
			on e.id = v.EstimationDataId
		inner join vinn.dbo.vehicle_service_xref x
			on x.VehicleID = v.VehicleID
		where e.AdminInfoID = @AdminInfoID
	)

	DECLARE @importedVehicleYear INT = (SELECT TOP 1 vi.[Year]
		FROM FocusWrite.dbo.EstimationData ed   
		JOIN FocusWrite.dbo.VehicleInfo vi  ON vi.EstimationDataID = ed.id 
		WHERE ed.AdminInfoID = @AdminInfoID
	)

	IF(@importedVehicleYear IS NULL)
		BEGIN
			UPDATE FocusWrite.dbo.VehicleInfo
			SET [Year] = @vinnXrefYear, 
				[MakeID] = @vinnXrefMake, 
				[ModelID] = @vinnXrefModel, 
				[SubModelID] = @vinnXrefSubModel
			WHERE ID = @vehicleInfoID
		END

	INSERT INTO VehicleInfoManual
	(
		[VehicleInfoID]
		,[Country]
		,[Manufacturer]
		,[Make]
		,[Model]
		,[ModelYear]
		,[SubModel]
		,[Service_Barcode]
		,[ManualSelection]
	)
	select
			@VehicleInfoID
		,v.[Country]
		,v.[Manufacturer]
		,v.[Make]
		,v.[Model]
		,v.[ModelYear]
		,v.[SubModel]
		,v.[Service_Barcode]
		,1
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.VehicleInfoManual as v  
		inner join [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.VehicleInfo as vi   on vi.id = v.VehicleInfoID
		inner join [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData as e   on e.id = vi.EstimationDataId
		inner join [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo as a  on a.id = e.AdminInfoID 
	where a.id = @AdminInfoID


	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting into JobStatusHistory...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------


	-- Job Status History
	INSERT INTO [focuswrite].[dbo].[JobStatusHistory]
	SELECT 
		  [AdminInfoID]
		, [JobStatusID]
		, [ActionDate]
	FROM [WEB-EST-PROE\WEBESTARCHIVE].[focuswrite].[dbo].[JobStatusHistory]
	WHERE AdminInfoID = @adminInfoId

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	-- the Owner contact has it's phone numbers stored differently than the other contacts so it is dont here instead of the ImportContact sp
	CREATE TABLE #blank (Empty varchar(50));
	INSERT INTO #blank (Empty) Values ('empty');

	-- Get all fields for the contact in an easy to use table
	CREATE TABLE #ownerFields (Text varchar(50), Tag varchar(50), Description varchar(50))
	INSERT INTO #ownerFields
	SELECT DISTINCT
		  ItemText
		, Qualifier
		, ContactItemTypes.Description
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo  
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoTOContacts  ON AdminInfoTOContacts.AdminInfoID = AdminInfo.id
	LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts  ON Contacts.ID = AdminInfoTOContacts.ContactsID AND ContactTypeID = 1 AND ContactSubTypeID = 1
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems  ON ContactItems.ContactsID = Contacts.id
	JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItemTypes  ON ContactItemTypes.id = ContactItems.ContactItemTypeID
	WHERE AdminInfo.id = @AdminInfoID 

	-- Get the phone numbers in their own table
	CREATE TABLE #phoneFields (PhoneNumber varchar(20), Tag varchar(5), Extension varchar(10), Row int)
	INSERT INTO #phoneFields
	SELECT ItemText, Qualifier, Extension, row_number() OVER (PARTITION BY Description ORDER BY Description)
	FROM
	(
		SELECT DISTINCT
			  ItemText
			, REPLACE(Qualifier, 'FX', 'HF') AS Qualifier
			, ISNULL(Extension, '') AS Extension
			, ContactItemTypes.Description
		FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo  
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoTOContacts  ON AdminInfoTOContacts.AdminInfoID = AdminInfo.id
		LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts  ON Contacts.ID = AdminInfoTOContacts.ContactsID AND ContactTypeID = 1 AND ContactSubTypeID = 1
		JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems  ON ContactItems.ContactsID = Contacts.id
		JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItemTypes  ON ContactItemTypes.id = ContactItems.ContactItemTypeID
		WHERE AdminInfo.id = @AdminInfoID AND ContactItemTypes.Description = 'Number' --AND Qualifier <> 'FX'
	) Base

	-- Search for an existing customer attached to this Login with the same first/last combo
	-- Ezra - 7/25/2019 - not doing this any more, every imported estimate gets it's own contact record
	DECLARE @CustomerID INT = 0
	--ISNULL((
	--	SELECT MAX(Customer.ID)
	--	FROM Customer
	--	JOIN tbl_ContactPerson ON Customer.ContactID = tbl_ContactPerson.ContactID
	--	WHERE 
	--		LoginID = @LoginID 
	--		AND FirstName = ISNULL((SELECT Text FROM #ownerFields WHERE Description = 'First'), '')
	--		AND LastName = ISNULL((SELECT Text FROM #ownerFields WHERE Description = 'Last'), '')
	--		--AND Phone1 = ISNULL((SELECT Text FROM #ownerFields WHERE Description = 'Number' AND Qualifier = 'CP'), '')
	--		--AND (ISNULL(FirstName, '') <> '' OR ISNULL(LastName, '') <> '' OR ISNULL(Phone1, '') <> '')
	--		AND (ISNULL(FirstName, '') <> '' OR ISNULL(LastName, '') <> '')
	--), 0)

	IF @CustomerID = 0
		BEGIN
			---------------------------------------------------------------------------------------------------------------------------------------------------------------
			print '#### Inserting owner contact information...'
			---------------------------------------------------------------------------------------------------------------------------------------------------------------

			-- Create a ContactPerson record for the customer
			DECLARE @contactID int = 0
			DECLARE @addressID int = 0

			INSERT INTO tbl_ContactPerson 
			(
				  AdminInfoID
				, FirstName
				, MiddleName
				, LastName
				, Email
				, BusinessName

				, Phone1
				, PhoneNumberType1
				, Extension1

				, Phone2
				, PhoneNumberType2
				, Extension2

				, Phone3
				, PhoneNumberType3
				, Extension3
			)
			SELECT TOP 1 
				  @AdminInfoID
				, firstName.Text
				, middleName.Text
				, lastName.Text
				, email.Text
				, biz.Text

				, CAST(dbo.getNumericValue(phone1.PhoneNumber) AS VARCHAR(15))
				, phone1.Tag
				, phone1.Extension

				, CAST(dbo.getNumericValue(phone2.PhoneNumber) AS VARCHAR(15))
				, phone2.Tag
				, phone2.Extension

				, CAST(dbo.getNumericValue(phone3.PhoneNumber) AS VARCHAR(15))
				, phone3.Tag
				, phone3.Extension
			FROM #blank
			LEFT OUTER JOIN #ownerFields AS firstName ON firstName.Description = 'First'
			LEFT OUTER JOIN #ownerFields AS lastName ON lastName.Description = 'Last'
			LEFT OUTER JOIN #ownerFields AS middleName ON middleName.Description = 'Middle'
			LEFT OUTER JOIN #ownerFields AS email ON email.Description = 'Email Address'
			LEFT OUTER JOIN #ownerFields AS biz ON biz.Description = 'Alias'

			LEFT OUTER JOIN #phoneFields AS phone1 ON phone1.Row = 1
			LEFT OUTER JOIN #phoneFields AS phone2 ON phone2.Row = 2
			LEFT OUTER JOIN #phoneFields AS phone3 ON phone3.Row = 3

			SET @contactID = (SELECT CAST(SCOPE_IDENTITY() AS INT))


			print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

			---------------------------------------------------------------------------------------------------------------------------------------------------------------
			print '#### Inserting owner address information...'
			---------------------------------------------------------------------------------------------------------------------------------------------------------------

			-- Create an Address record for the customer
			INSERT INTO tbl_Address (AdminInfoID, ContactsID, Address1, Address2, City, State, Country, zip, TimeZone)
			SELECT @AdminInfoID, @contactID, address1.Text, address2.Text, city.Text, state.Text, '', zip.Text, timeZone.Text
			FROM #blank
			LEFT OUTER JOIN #ownerFields AS address1 ON address1.Description = 'Line 1'
			LEFT OUTER JOIN #ownerFields AS address2 ON address2.Description = 'Line 2'
			LEFT OUTER JOIN #ownerFields AS city ON city.Description = 'City'
			LEFT OUTER JOIN #ownerFields AS state ON state.Description = 'State'
			LEFT OUTER JOIN #ownerFields AS zip ON zip.Description = 'Zip'
			LEFT OUTER JOIN #ownerFields AS timeZone ON timeZone.Description = 'TimeZone'

			SET @addressID = (SELECT CAST(SCOPE_IDENTITY() AS INT))

			INSERT INTO Customer
			(
				  LoginID
				, ContactID
				, AddressID
				, IsDeleted
			)
			VALUES
			(
				  @LoginID
				, @contactID
				, @addressID
				, 0
			)

			SET @customerID = (SELECT CAST(SCOPE_IDENTITY() AS INT))

			DROP TABLE #ownerFields
			DROP TABLE #phoneFields
			DROP TABLE #blank

			print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

		END

	UPDATE AdminInfo
	SET CustomerID = @CustomerID
	WHERE AdminInfo.id = @AdminInfoID

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Inserting Extra contacts...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	EXEC DataMigration_ImportContact @adminInfoID = @AdminInfoID, @contactSubTypeID = 7
	EXEC DataMigration_ImportContact @adminInfoID = @AdminInfoID, @contactSubTypeID = 11
	EXEC DataMigration_ImportContact @adminInfoID = @AdminInfoID, @contactSubTypeID = 20
	EXEC DataMigration_ImportContact @adminInfoID = @AdminInfoID, @contactSubTypeID = 3, @importAddress = 1
	EXEC DataMigration_ImportContact @adminInfoID = @AdminInfoID, @contactSubTypeID = 2, @importAddress = 1

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Updating EstimationData ClaimantSamesOwner...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	BEGIN TRY  
		UPDATE EstimationData
		SET ClaimantSameAsOwner = 
			(
			SELECT ISNULL(ItemText, 0) AS SameAsOwner
			FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo  
			JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoTOContacts  ON AdminInfoTOContacts.AdminInfoID = AdminInfo.id
			LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts  ON Contacts.ID = AdminInfoTOContacts.ContactsID AND ContactTypeID = 1 AND ContactSubTypeID = 3
			JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems  ON ContactItems.ContactsID = Contacts.id AND ContactItems.ContactItemTypeID = 28
			WHERE AdminInfo.id = @AdminInfoID
		)
		WHERE EstimationData.AdminInfoID = @AdminInfoID  
	END TRY  
	BEGIN CATCH  
		 print '#### Could not update ClaimantSamesOwner...'
	END CATCH  

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();


	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Updating EstimationData InsuredSameAsOwner...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	BEGIN TRY  
		UPDATE EstimationData
		SET InsuredSameAsOwner = 
		(
			SELECT DISTINCT ISNULL(ItemText, 0) AS SameAsOwner
			FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo  
			JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfoTOContacts  ON AdminInfoTOContacts.AdminInfoID = AdminInfo.id
			LEFT OUTER JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.Contacts  ON Contacts.ID = AdminInfoTOContacts.ContactsID AND ContactTypeID = 1 AND ContactSubTypeID = 2
			JOIN [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.ContactItems  ON ContactItems.ContactsID = Contacts.id AND ContactItems.ContactItemTypeID = 28
			WHERE AdminInfo.id = @AdminInfoID
		)
		WHERE EstimationData.AdminInfoID = @AdminInfoID 
	END TRY  
	BEGIN CATCH  
		 print '#### Could not update InsuredSameAsOwner...'
	END CATCH  

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	-- Import the CustomerProfile attached to the estimate
	print '#### Importing customer profile...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	DECLARE @customerProfileID INT = (SELECT CustomerProfilesID FROM AdminInfo  WHERE id = @AdminInfoID)

	INSERT INTO [CustomerProfiles]
	(
		[OwnerID]
		,[OwnerType]
		,[ProfileName]
		,[Description]
		,[DefaultFlag]
		,[CreationDate]
		,[CreatedBy]
		,[OriginalID]
		,[AdminInfoID]
		,[DefaultPreset]
	)
	SELECT 
			[OwnerID]
		,[OwnerType]
		,[ProfileName]
		,[Description]
		,0
		,[CreationDate]
		,[CreatedBy]
		,@customerProfileID
		,@AdminInfoID		-- Ezra 6/11/2019 - this was 0, changed back to AminInfoID so the profile doesn't show in the globals.  Not sure why it was set to 0
		,0
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfiles 
	WHERE id = @customerProfileID

	DECLARE @customerProfileIDTarget INT = (SELECT CAST(SCOPE_IDENTITY() AS INT))

	UPDATE [CustomerProfiles] SET [OriginalID] = @customerProfileIDTarget WHERE ID = @customerProfileIDTarget

	UPDATE AdminInfo
	SET CustomerProfilesID = @customerProfileIDTarget
	WHERE id = @AdminInfoID

	INSERT INTO [CustomerProfileRates]
	(
		[CustomerProfilesID]
		,[RateType]
		,[Rate]
		,[CapType]
		,[Cap]
		,[Taxable]
		,[DiscountMarkup]
		,[IncludeIn]
	)
	select
			@customerProfileIDTarget
		,[RateType]
		,[Rate]
		,[CapType]
		,[Cap]
		,[Taxable]
		,[DiscountMarkup]
		,[IncludeIn]
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfileRates  
	where CustomerProfilesID = @customerProfileID


	INSERT INTO [CustomerProfilePresets]
	(
			[CustomerProfilesID]
		,[StepID]
		,[PartSource]
		,[ActionCode]
		,[ActionDescription]
		,[PartNumber]
		,[Description]
		,[Price]
		,[PartOfOverhaul]
		,[Qty]
		,[LaborTime]
		,[PaintTime]
		,[Other]
		,[ImageID]
		,[PartSourceVendorID]
		,[SubletOperationFlag]
		,[BettermentOperationFlag]
	)
	select
			@customerProfileIDTarget
		,[StepID]
		,[PartSource]
		,[ActionCode]
		,[ActionDescription]
		,[PartNumber]
		,[Description]
		,[Price]
		,[PartOfOverhaul]
		,[Qty]
		,[LaborTime]
		,[PaintTime]
		,[Other]
		,[ImageID]
		,[PartSourceVendorID]
		,[SubletOperationFlag]
		,[BettermentOperationFlag]
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets  
	where CustomerProfilesID = @customerProfileID

	DECLARE @newCustomerProfilePresets INT = (SELECT CAST(SCOPE_IDENTITY() AS INT))


	INSERT INTO [CustomerProfilePresetsLabor]
	(
			[CustomerProfilePresetsID]
		,[LaborType]
		,[LaborSubType]
		,[LaborTime]
		,[LaborCost]
		,[AdjacentDeduction]
		,[MajorPanel]
	)
	select 
			@newCustomerProfilePresets
		,[LaborType]
		,[LaborSubType]
		,[LaborTime]
		,[LaborCost]
		,[AdjacentDeduction]
		,[MajorPanel]
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresetsLabor  
	where CustomerProfilePresetsID in 
	(
		select id
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets  
		where CustomerProfilesID = @customerProfileID
	)


	INSERT INTO [CustomerProfilePresetsNotes]
	(
			[CustomerProfilePresetsID]
		,[Printed]
		,[Notes]
	)
	select 
			@newCustomerProfilePresets
		,[Printed]
		,[Notes]
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresetsNotes 
	where CustomerProfilePresetsID in 
	(
		select id
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets 
		where CustomerProfilesID = @customerProfileID
	)


	INSERT INTO [CustomerProfilePrint]
	(
			[CustomerProfilesID]
		,[GraphicsQuality]
		,[NoHeaderLogo]
		,[NoInsuranceSection]
		,[NoPhotos]
		,[FooterText]
		,[PrintPrivateNotes]
		,[PrintPublicNotes]
		,[ContactOption]
		,[SupplementOption]
		,[OrderBy]
		,[UseBigLetters]
		,[SeparateLabor]
		,[EstimateNumber]
		,[AdminInfoID]
		,[Dollars]
		,[GreyBars]
		,[NoVehicleAccessories]
		,[PrintPaymentInfo]
	)
	select 
			@customerProfileIDTarget
		,[GraphicsQuality]
		,[NoHeaderLogo]
		,[NoInsuranceSection]
		,[NoPhotos]
		,[FooterText]
		,[PrintPrivateNotes]
		,[PrintPublicNotes]
		,[ContactOption]
		,[SupplementOption]
		,[OrderBy]
		,[UseBigLetters]
		,[SeparateLabor]
		,[EstimateNumber]
		,[AdminInfoID]
		,[Dollars]
		,[GreyBars]
		,[NoVehicleAccessories]
		,[PrintPaymentInfo]
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePrint 
	where CustomerProfilesID = @customerProfileID


	INSERT INTO [CustomerProfilesMisc]
	(
			[CustomerProfilesID]
		,[TaxRate]
		,[SecondTaxRateStart]
		,[SecondTaxRate]
		,[ChargeForRadiatorRefilling]
		,[RadiatorChargeAmount]
		,[ChargeForACRefilling]
		,[ACChargeHow]
		,[ACChargeAmount]
		,[LKQText]
		,[TotalLossPerc]
		,[IncludeStructureInBody]
		,[ChargeForAimingHeadlights]
		,[ChargeForPowerUnits]
		,[ChargeForRefrigRecovery]
		,[SuppressAddRelatedPrompt]
		,[ChargeSuppliesOnAllOperations]
		,[SuppLevel]
		,[DoNotMarkChanges]
		,UseSepPartLaborTax
		,PartTax
		,LaborTax
	)
	select 
			@customerProfileIDTarget
		,[TaxRate]
		,[SecondTaxRateStart]
		,[SecondTaxRate]
		,[ChargeForRadiatorRefilling]
		,[RadiatorChargeAmount]
		,[ChargeForACRefilling]
		,[ACChargeHow]
		,[ACChargeAmount]
		,[LKQText]
		,[TotalLossPerc]
		,[IncludeStructureInBody]
		,[ChargeForAimingHeadlights]
		,[ChargeForPowerUnits]
		,[ChargeForRefrigRecovery]
		,[SuppressAddRelatedPrompt]
		,[ChargeSuppliesOnAllOperations]
		,[SuppLevel]
		,[DoNotMarkChanges]
		,UseSepPartLaborTax
		,PartTax
		,LaborTax
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilesMisc 
	where CustomerProfilesID = @customerProfileID


	INSERT INTO [CustomerProfilesPaint]
	(
			[CustomerProfilesID]
		,[AllowDeductions]
		,[AdjacentDeduction]
		,[NonAdjacentDeduction]
		,[EdgeInteriorTimes]
		,[MajorClearCoat]
		,[MajorThreeStage]
		,[MajorTwoTone]
		,[OverlapClearCoat]
		,[OverlapThreeStage]
		,[OverLapTwoTone]
		,[ClearCoatCap]
		,[DeductFinishOverlap]
		,[TotalClearcoatWithPaint]
		,[NoClearcoatCap]
		,[ThreeStageInner]
		,[ThreeStagePillars]
		,[ThreeStateInterior]
		,[TwoToneInner]
		,[TwoTonePillars]
		,[TwoToneInterior]
		,[Blend]
		,[Underside]
		,[Edging]
		,[ManualPaintOverlap]
		,[AutomaticOverlap]
	)
	select
			@customerProfileIDTarget
		,[AllowDeductions]
		,[AdjacentDeduction]
		,[NonAdjacentDeduction]
		,[EdgeInteriorTimes]
		,[MajorClearCoat]
		,[MajorThreeStage]
		,[MajorTwoTone]
		,[OverlapClearCoat]
		,[OverlapThreeStage]
		,[OverLapTwoTone]
		,[ClearCoatCap]
		,[DeductFinishOverlap]
		,[TotalClearcoatWithPaint]
		,[NoClearcoatCap]
		,[ThreeStageInner]
		,[ThreeStagePillars]
		,[ThreeStateInterior]
		,[TwoToneInner]
		,[TwoTonePillars]
		,[TwoToneInterior]
		,[Blend]
		,[Underside]
		,[Edging]
		,[ManualPaintOverlap]
		,1 
	from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilesPaint 
	where CustomerProfilesID = CASE 
		WHEN EXISTS (SELECT * FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilesPaint WHERE CustomerProfilesID = @customerProfileID) THEN @customerProfileID 
		ELSE (SELECT ID FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfiles WHERE CreatedBy = @LoginID AND DefaultFlag = 1) 
	END

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### Insert payment info...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	INSERT INTO PaymentInfo
	(
			AdminInfoID
		, ContactsID
		, PayeeName
		, PaymentType
		, PaymentDate
		, CheckNumber
		, Amount
		, Memo
		, WhoPays
	)
	SELECT
			PaymentInfo.AdminInfoID
		, PaymentInfo.ContactsID
		, PaymentInfo.PayeeName
		, PaymentInfo.PaymentType
		, PaymentInfo.PaymentDate
		, PaymentInfo.CheckNumber
		, PaymentInfo.Amount
		, PaymentInfo.Memo
		, PaymentInfo.WhoPays
	FROM [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.PaymentInfo with(nolock)
	WHERE PaymentInfo.AdminInfoID = @AdminInfoID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### fix lock level...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	EXEC  [Admin].[FixLockLevelForEstimate] @LoginID = @LoginID, @AdminInfoId = @AdminInfoID
	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();

	---------------------------------------------------------------------------------------------------------------------------------------------------------------
	print '#### update admin totals...'
	---------------------------------------------------------------------------------------------------------------------------------------------------------------

	EXEC UpdateAdminTotals @AdminInfoID = @AdminInfoID

	print 'Milliseconds: ' + CAST(DATEDIFF(millisecond, @time, GETDATE()) AS VARCHAR);  SET @time = GETDATE();


			/*
		END
	ELSE
		BEGIN
			-- Import the profile directly, preserving all IDs

			SET IDENTITY_INSERT CustomerProfileRates on
			INSERT INTO [CustomerProfileRates]
			(
					id
				,[CustomerProfilesID]
				,[RateType]
				,[Rate]
				,[CapType]
				,[Cap]
				,[Taxable]
				,[DiscountMarkup]
				,[IncludeIn]
			)
			select *
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfileRates
			where CustomerProfilesID = @customerProfileID
			SET IDENTITY_INSERT CustomerProfileRates off


			SET IDENTITY_INSERT CustomerProfilePresets on
			INSERT INTO [CustomerProfilePresets]
			(
				 id
				,[CustomerProfilesID]
				,[StepID]
				,[PartSource]
				,[ActionCode]
				,[ActionDescription]
				,[PartNumber]
				,[Description]
				,[Price]
				,[PartOfOverhaul]
				,[Qty]
				,[LaborTime]
				,[PaintTime]
				,[Other]
				,[ImageID]
				,[PartSourceVendorID]
				,[SubletOperationFlag]
				,[BettermentOperationFlag]
			)
			select *
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets
			where CustomerProfilesID = @customerProfileID
			SET IDENTITY_INSERT CustomerProfilePresets off


			SET IDENTITY_INSERT CustomerProfilePresetsLabor on
			INSERT INTO [CustomerProfilePresetsLabor]
			(
				 id
				,[CustomerProfilePresetsID]
				,[LaborType]
				,[LaborSubType]
				,[LaborTime]
				,[LaborCost]
				,[AdjacentDeduction]
				,[MajorPanel]
			)
			select * 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresetsLabor
			where CustomerProfilePresetsID in 
			(
				select id
				from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets
				where CustomerProfilesID = @customerProfileID
			)
			SET IDENTITY_INSERT CustomerProfilePresetsLabor off


			SET IDENTITY_INSERT CustomerProfilePresetsNotes on
			INSERT INTO [CustomerProfilePresetsNotes]
			(
				 id
				,[CustomerProfilePresetsID]
				,[Printed]
				,[Notes]
			)
			select * 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresetsNotes
			where CustomerProfilePresetsID in 
			(
				select id
				from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePresets
				where CustomerProfilesID = @customerProfileID
			)
			SET IDENTITY_INSERT CustomerProfilePresetsNotes off


			SET IDENTITY_INSERT CustomerProfilePrint on
			INSERT INTO [CustomerProfilePrint]
			(
				 id
				,[CustomerProfilesID]
				,[GraphicsQuality]
				,[NoHeaderLogo]
				,[NoInsuranceSection]
				,[NoPhotos]
				,[FooterText]
				,[PrintPrivateNotes]
				,[PrintPublicNotes]
				,[ContactOption]
				,[SupplementOption]
				,[OrderBy]
				,[UseBigLetters]
				,[SeparateLabor]
				,[EstimateNumber]
				,[AdminInfoID]
				,[Dollars]
				,[GreyBars]
				,[NoVehicleAccessories]
				,[PrintPaymentInfo]
			)
			select *
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilePrint
			where CustomerProfilesID = @customerProfileID
			SET IDENTITY_INSERT CustomerProfilePrint off


			SET IDENTITY_INSERT CustomerProfilesMisc on
			INSERT INTO [CustomerProfilesMisc]
			(
				 id
				,[CustomerProfilesID]
				,[TaxRate]
				,[SecondTaxRateStart]
				,[SecondTaxRate]
				,[ChargeForRadiatorRefilling]
				,[RadiatorChargeAmount]
				,[ChargeForACRefilling]
				,[ACChargeHow]
				,[ACChargeAmount]
				,[LKQText]
				,[TotalLossPerc]
				,[IncludeStructureInBody]
				,[ChargeForAimingHeadlights]
				,[ChargeForPowerUnits]
				,[ChargeForRefrigRecovery]
				,[SuppressAddRelatedPrompt]
				,[ChargeSuppliesOnAllOperations]
				,[SuppLevel]
				,[DoNotMarkChanges]
				,UseSepPartLaborTax
				,PartTax
				,LaborTax
			)
			select * 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilesMisc
			where CustomerProfilesID = @customerProfileID
			SET IDENTITY_INSERT CustomerProfilesMisc off


			SET IDENTITY_INSERT CustomerProfilesPaint on
			INSERT INTO [CustomerProfilesPaint]
			(
				 id
				,[CustomerProfilesID]
				,[AllowDeductions]
				,[AdjacentDeduction]
				,[NonAdjacentDeduction]
				,[EdgeInteriorTimes]
				,[MajorClearCoat]
				,[MajorThreeStage]
				,[MajorTwoTone]
				,[OverlapClearCoat]
				,[OverlapThreeStage]
				,[OverLapTwoTone]
				,[ClearCoatCap]
				,[DeductFinishOverlap]
				,[TotalClearcoatWithPaint]
				,[NoClearcoatCap]
				,[ThreeStageInner]
				,[ThreeStagePillars]
				,[ThreeStateInterior]
				,[TwoToneInner]
				,[TwoTonePillars]
				,[TwoToneInterior]
				,[Blend]
				,[Underside]
				,[Edging]
				,[ManualPaintOverlap]
				,[AutomaticOverlap]
			)
			select *,1 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfilesPaint
			where CustomerProfilesID = @customerProfileID
			SET IDENTITY_INSERT CustomerProfilesPaint off


			SET IDENTITY_INSERT [CustomerProfiles] on
			INSERT INTO [CustomerProfiles]
			(
				 id
				,[OwnerID]
				,[OwnerType]
				,[ProfileName]
				,[Description]
				,[DefaultFlag]
				,[CreationDate]
				,[CreatedBy]
				,[OriginalID]
				,[AdminInfoID]
				,[DefaultPreset]
			)
			select * 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.CustomerProfiles
			WHERE id = @customerProfileID
			SET IDENTITY_INSERT CustomerProfiles off
		END
	*/

		/*
		-- TODO - import and copy images
		SET IDENTITY_INSERT FocusWrite.dbo.EstimationImages ON
		INSERT INTO [FocusWrite].[dbo].[EstimationImages]
				   (id,[EstimationDataID]
				   ,[ImageURL]
				   ,[Caption])

		select *
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationImages
		where estimationdataid in (select id 
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData
		where admininfoid in 
		(select id
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.AdminInfo 
		where CreatorID = @LoginID))
		SET IDENTITY_INSERT FocusWrite.dbo.EstimationImages off
		*/


		/*
		TODO - import this test data to local and uncomment
	print convert(varchar, getdate(), 21)
	print '#### Inserting into EstimateLineItemsChanges...'

		INSERT INTO EstimateLineItemsChanges
				   ([VehicleID]
				   ,[EstimationDataID]
				   ,[PartNumber]
				   ,[BarCode]
				   ,[ID]) 
		select *
		from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimateLineItemsChanges
		where estimationdataid in 
		(
			select id 
			from [WEB-EST-PROE\WEBESTARCHIVE].FocusWrite.dbo.EstimationData
			where admininfoid = @AdminInfoID 
		)
		*/


END
GO
