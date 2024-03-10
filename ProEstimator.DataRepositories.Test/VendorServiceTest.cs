using Xunit;

using ProEstimatorData.DataModel;
using ProEstimator.DataRepositories.Vendors;

namespace ProEstimator.Data.Test
{
    public class VendorServiceTest
    {
        private readonly IVendorRepository _vendorService;
        private readonly int _loginID = 56027; // Mike's account

        public VendorServiceTest()
        {
            _vendorService = new VendorRepository(); 
        }

        [Fact]
        public void Get_ReturnsCorrectVendor()
        {
            // Arrange
            int testId = 9; // ID of a known Vendor in test database

            // Act
            Vendor result = _vendorService.Get(testId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(testId, result.ID);
        }

        [Fact]
        public void GetUniversalList_ReturnsListOfVendors()
        {
            // Act
            var result = _vendorService.GetUniversalList(_loginID);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void GetPrivateList_ReturnsPrivateListOfVendors()
        {
            // Arrange
            var vendorType = VendorType.AfterMarket; 

            // Act
            var result = _vendorService.GetPrivateList(_loginID, vendorType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void GetAllForType_ReturnsAllVendorsForGivenType()
        {
            // Arrange
            var vendorType = VendorType.AfterMarket;

            // Act
            var result = _vendorService.GetAllForType(_loginID, vendorType);

            // Assert
            Assert.NotNull(result);
            Assert.True(result.Count > 0);
        }

        [Fact]
        public void SaveAndDeleteAVendor()
        {
            // Arrange
            Vendor testVendor = new Vendor
            {
                CompanyName = "TestCompany",
                FirstName = "John",
                LastName = "Doe",
                Email = "john.doe@testcompany.com",
                MobilePhone = "123-456-7890",
                WorkPhone = "098-765-4321",
                Address1 = "123 Test Street",
                Address2 = "Suite 456",
                City = "Testville",
                State = "TS",
                Zip = "12345",
                TimeZone = "PST",
                Universal = true,
                Type = VendorType.AfterMarket,
                FaxNumber = "111-222-3333",
                FileName = "samplefile.txt",
                Deleted = false,
                Extension = ".txt",
                FederalTaxID = "12-3456789",
                LicenseNumber = "LIC-12345",
                BarNumber = "BAR-54321",
                RegistrationNumber = "REG-11111"
            };
            int activeLoginId = 1;

            // Act
            var saveResult = _vendorService.Save(testVendor, activeLoginId);

            // Assert
            Assert.NotNull(saveResult);
            Assert.True(saveResult.Success);

            // Retrieve vendor from disk (database)
            var retrievedVendor = _vendorService.Get(testVendor.ID);

            // Assert: Check if the retrieved vendor matches the saved testVendor
            Assert.NotNull(retrievedVendor);
            Assert.Equal(testVendor.CompanyIDCode, retrievedVendor.CompanyIDCode);
            Assert.Equal(testVendor.CompanyName, retrievedVendor.CompanyName);
            Assert.Equal(testVendor.FirstName, retrievedVendor.FirstName);
            Assert.Equal(testVendor.LastName, retrievedVendor.LastName);
            Assert.Equal(testVendor.Email, retrievedVendor.Email);
            Assert.Equal(testVendor.MobilePhone, retrievedVendor.MobilePhone);
            Assert.Equal(testVendor.WorkPhone, retrievedVendor.WorkPhone);
            Assert.Equal(testVendor.Address1, retrievedVendor.Address1);
            Assert.Equal(testVendor.Address2, retrievedVendor.Address2);
            Assert.Equal(testVendor.City, retrievedVendor.City);
            Assert.Equal(testVendor.State, retrievedVendor.State);
            Assert.Equal(testVendor.Zip, retrievedVendor.Zip);
            Assert.Equal(testVendor.TimeZone, retrievedVendor.TimeZone);
            Assert.Equal(testVendor.Universal, retrievedVendor.Universal);
            Assert.Equal(testVendor.Type, retrievedVendor.Type);
            Assert.Equal(testVendor.FaxNumber, retrievedVendor.FaxNumber);
            Assert.Equal(testVendor.FileName, retrievedVendor.FileName);
            Assert.Equal(testVendor.Deleted, retrievedVendor.Deleted);
            Assert.Equal(testVendor.Extension, retrievedVendor.Extension);
            Assert.Equal(testVendor.FederalTaxID, retrievedVendor.FederalTaxID);
            Assert.Equal(testVendor.LicenseNumber, retrievedVendor.LicenseNumber);
            Assert.Equal(testVendor.BarNumber, retrievedVendor.BarNumber);
            Assert.Equal(testVendor.RegistrationNumber, retrievedVendor.RegistrationNumber);

            // Now delete the vendor
            // Act
            var deleteResult = _vendorService.Delete(testVendor.ID, activeLoginId);

            // Assert
            Assert.NotNull(deleteResult);
            Assert.True(deleteResult.Success);
        }

        [Fact]
        public void ToggleSelection_TogglesVendorSelection()
        {
            // Arrange
            int loginId = 1;
            int vendorId = 1;
            int activeLoginId = 1;

            // Act
            var toggleResult = _vendorService.ToggleSelection(loginId, vendorId, activeLoginId);

            // Assert
            Assert.NotNull(toggleResult);
            Assert.True(toggleResult.Success);
        }
    }
}