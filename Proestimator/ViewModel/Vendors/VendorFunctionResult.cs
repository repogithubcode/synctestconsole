using ProEstimatorData;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Vendors
{
    public class VendorFunctionResult : FunctionResult
    {
        public VendorVM Vendor { get; set; }

        public VendorFunctionResult() { }

        public VendorFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }

        public VendorFunctionResult(VendorVM vendorVM)
        {
            Vendor = vendorVM;
        }
    }
}