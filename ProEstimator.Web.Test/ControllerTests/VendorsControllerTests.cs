using System.Web.Mvc;

using Xunit;

using ProEstimatorData;
using ProEstimatorData.DataModel;

using ProEstimator.DataRepositories.Vendors;

using Proestimator.Controllers.Vendors;
using Proestimator.ViewModel.Vendors;

namespace ProEstimator.Web.Test.ControllerTests
{
    public class VendorsControllerTests
    {
        private IVendorRepository _vendorService;

        private readonly VendorsController _controller;
        private readonly int _loginID = 56027;  // Mike's account
        private readonly int _userID = 30519;   // Mike's first user account
        private readonly string _computerKey = "ABCDEFGHIJ";

        public VendorsControllerTests()
        {
            ProEstimatorData.DataModel.Contracts.ContractTerms.LoadAll();

            _vendorService = new VendorRepository();

            _controller = new VendorsController(_vendorService);

            _controller.TestProjectUserLogin(_userID, _computerKey);
        }

        [Fact]
        public void GetVendor_ReturnsFunctionResultWithSuccess_True()
        {
            // Arrange
            int vendorId = 9;

            // Act
            var result = _controller.GetVendor(_userID, vendorId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var functionResult = Assert.IsType<VendorFunctionResult>(jsonResult.Data);
            Assert.True(functionResult.Success);
        }

        [Fact]
        public void DeleteVendor_ReturnsFunctionResultWithSuccess_True()
        {
            // NOTE: VendorID 1 doesn't exist so the delete will fail.  This tests that we get a result.

            // Arrange
            int vendorId = 1;

            // Act
            var result = _controller.DeleteVendor(_userID, _loginID, vendorId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var functionResult = Assert.IsType<FunctionResult>(jsonResult.Data);
            Assert.False(functionResult.Success);
        }

        [Fact]
        public void ToggleVendorSelection_ReturnsFunctionResultWithSuccess_True()
        {
            // Arrange
            int vendorId = 1;

            // Act
            var result = _controller.ToggleVendorSelection(_userID, _loginID, vendorId);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var functionResult = Assert.IsType<FunctionResult>(jsonResult.Data);
            Assert.True(functionResult.Success);
        }

        [Fact]
        public void SaveVendor_ReturnsFunctionResultWithSuccess_True()
        {
            // Arrange
            var vendorVM = new VendorVM()
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
                IsPublic = true,
                Type = (int)VendorType.AfterMarket,
                FaxNumber = "111-222-3333",
                Extension = ".txt",
                FederalTaxID = "12-3456789",
                LicenseNumber = "LIC-12345",
                BarNumber = "BAR-54321",
                RegistrationNumber = "REG-11111"
            };

            // Act
            var result = _controller.SaveVendor(_userID, _loginID, vendorVM);

            // Assert
            var jsonResult = Assert.IsType<JsonResult>(result);
            var functionResult = Assert.IsType<VendorFunctionResult>(jsonResult.Data);
            Assert.True(functionResult.Success);

            // Permenantly delete the new vendor from the DB.
            if (functionResult.Success && functionResult.Vendor != null)
            {
                DBAccess db = new DBAccess();
                db.ExecuteSql("DELETE FROM Vendor WHERE ID = " + functionResult.Vendor.ID);
            }
        }
    }
}
