/*CREATE TABLE [dbo].[VehicleDriveMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[DriveCode] [varchar](10) NULL,
	[DriveID] [int] NULL
) ON [PRIMARY]*/

Insert Into VehicleDriveMapping(DriveCode, DriveID) Values('2WD', 3)
Insert Into VehicleDriveMapping(DriveCode, DriveID) Values('4WD', 4)
Insert Into VehicleDriveMapping(DriveCode, DriveID) Values('AWD', 5)
Insert Into VehicleDriveMapping(DriveCode, DriveID) Values('FWD', 6)
Insert Into VehicleDriveMapping(DriveCode, DriveID) Values('RWD', 7)
