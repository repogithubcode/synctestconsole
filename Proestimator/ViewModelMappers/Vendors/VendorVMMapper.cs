using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Vendors;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModelMappers.Vendors
{
    public class VendorVMMapper : IVMMapper<VendorVM>
    {
        public VendorVM Map(MappingConfiguration mappingConfiguration)
        {
            VendorVMMapperConfiguration config = mappingConfiguration as VendorVMMapperConfiguration;

            VendorVM vm = new VendorVM();

            vm.ID = config.Vendor.ID;
            vm.LoginsID = config.Vendor.LoginsID;
            vm.FirstName = config.Vendor.FirstName;
            vm.LastName = config.Vendor.LastName;
            vm.Email = config.Vendor.Email;
            vm.CompanyName = config.Vendor.CompanyName;
            vm.MobilePhone = config.Vendor.MobilePhone;
            vm.WorkPhone = config.Vendor.WorkPhone;
            vm.Address1 = config.Vendor.Address1;
            vm.Address2 = config.Vendor.Address2;
            vm.City = config.Vendor.City;
            vm.State = config.Vendor.State;
            vm.Zip = config.Vendor.Zip;
            vm.FaxNumber = config.Vendor.FaxNumber;
            vm.Extension = config.Vendor.Extension;
            vm.IsPublic = config.Vendor.Universal;
            vm.TimeZone = config.Vendor.TimeZone;
            vm.Type = (int)config.Vendor.Type;

            vm.FederalTaxID = config.Vendor.FederalTaxID;
            vm.LicenseNumber = config.Vendor.LicenseNumber;
            vm.BarNumber = config.Vendor.BarNumber;
            vm.RegistrationNumber = config.Vendor.RegistrationNumber;

            return vm;
        }
    }

    public class VendorVMMapperConfiguration : MappingConfiguration
    {
        public Vendor Vendor { get; set; }
    } 
}