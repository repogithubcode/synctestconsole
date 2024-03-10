/*CREATE TABLE [dbo].[VehicleTransmissionMapping](
	[ID] [int] IDENTITY(1,1) NOT NULL,
	[TransCode] [char](1) NULL,
	[Description] [varchar](50) NULL,
	[TransmissionID] [int] NULL
) ON [PRIMARY]*/

Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('A', '3 speed manual', 17)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('B', '4 speed manual', 6)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('C', '4 speed manual w/overdrive', 6)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('D', '5 speed manual', 9)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('E', '5 speed manual w/overdrive', 9)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('F', '6 speed manual', 11)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('G', '3 speed automatic', 2)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('H', '3 speed automatic w/overdrive', 2)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('I', '4 speed automatic', 4)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('J', '4 speed automatic w/overdrive', 4)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('K', '4 speed automatic w/electronic overdrive', 4)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('L', 'ECVT automatic', 12)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('M', 'Unknown / other manual', 18)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('N', 'Unknown / other automatic', 12)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('O', 'Unknown / other', 1)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('P', '5 speed automatic', 8)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('Q', '5 speed automatic w/overdrive', 8)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('R', '6 speed automatic', 19)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('S', '7 speed automatic', 20)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('T', '8 speed automatic', 29)
Insert Into VehicleTransmissionMapping(TransCode, Description, TransmissionID) Values('W', '6 speed automatic A', 19)
