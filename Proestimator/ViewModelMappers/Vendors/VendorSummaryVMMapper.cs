using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.ViewModel.Vendors;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModelMappers.Vendors
{
    public class VendorSummaryVMMapper : IVMMapper<VendorSummaryVM>
    {
        public VendorSummaryVM Map(MappingConfiguration mappingConfiguration)
        {
            VendorSummaryVMMapperConfiguration config = mappingConfiguration as VendorSummaryVMMapperConfiguration;

            VendorSummaryVM vm = new VendorSummaryVM();

            vm.IsSelected = config.Vendor.IsSelected;
            vm.ID = config.Vendor.ID;
            vm.CompanyName = config.Vendor.CompanyName;
            vm.State = config.Vendor.State;

            return vm;
        }
    }

    public class VendorSummaryVMMapperConfiguration: MappingConfiguration
    {
        public Vendor Vendor { get; set; }
    }
}