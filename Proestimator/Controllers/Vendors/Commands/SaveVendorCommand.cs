using ProEstimator.DataRepositories.Vendors;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using Proestimator.ViewModel.Vendors;
using Proestimator.ViewModelMappers.Vendors;

namespace Proestimator.Controllers.Vendors.Commands
{
    public class SaveVendorCommand : CommandBase
    {
        private IVendorRepository _vendorRepository;
        private VendorVM _vendorVM;
        private int _activeLoginID;
        private int _loginID;

        public VendorFunctionResult VendorFunctionResult { get; set; }

        public SaveVendorCommand(IVendorRepository vendorRepository, int activeLogin, int loginID, VendorVM vendorVM)
        {
            _vendorRepository = vendorRepository;
            _activeLoginID = activeLogin;
            _loginID = loginID;
            _vendorVM = vendorVM;

            VendorFunctionResult = new VendorFunctionResult();
        }

        public override bool Execute()
        {
            Vendor vendor = new Vendor();
            if (_vendorVM.ID > 0)
            {
                vendor = _vendorRepository.Get(_vendorVM.ID);
            }

            vendor.LoginsID = _loginID;
            vendor.Address1 = _vendorVM.Address1;
            vendor.Address2 = _vendorVM.Address2;
            vendor.City = _vendorVM.City;
            vendor.CompanyName = _vendorVM.CompanyName;
            vendor.Email = _vendorVM.Email;
            vendor.FaxNumber = InputHelper.GetNumbersOnly(_vendorVM.FaxNumber);
            vendor.FirstName = _vendorVM.FirstName;
            vendor.LastName = _vendorVM.LastName;
            vendor.MobilePhone = InputHelper.GetNumbersOnly(_vendorVM.MobilePhone);
            vendor.State = _vendorVM.State;
            vendor.TimeZone = _vendorVM.TimeZone;
            vendor.WorkPhone = InputHelper.GetNumbersOnly(_vendorVM.WorkPhone);
            vendor.Zip = _vendorVM.Zip;
            vendor.Extension = _vendorVM.Extension;
            vendor.Type = (VendorType)_vendorVM.Type;
            vendor.FederalTaxID = _vendorVM.FederalTaxID;
            vendor.LicenseNumber = _vendorVM.LicenseNumber;
            vendor.BarNumber = _vendorVM.BarNumber;
            vendor.RegistrationNumber = _vendorVM.RegistrationNumber;

            SaveResult saveResult = _vendorRepository.Save(vendor, _activeLoginID);
            if (saveResult.Success)
            {
                VendorVMMapper mapper = new VendorVMMapper();
                VendorFunctionResult.Vendor = mapper.Map(new VendorVMMapperConfiguration() { Vendor = vendor });
            }
            else
            {
                VendorFunctionResult.Success = false;
                VendorFunctionResult.ErrorMessage = saveResult.ErrorMessage;
            }

            return true;
        }

    }
}