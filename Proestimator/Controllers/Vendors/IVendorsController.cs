using Kendo.Mvc.UI;
using Proestimator.ViewModel.Vendors;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.Controllers.Vendors
{
    public interface IVendorsController 
    {
        ActionResult GetVendorList([DataSourceRequest] DataSourceRequest request, int loginID, int typeID, bool publicVendors, string filterText);

        /// <summary>Returns a VendorFunctionResult.</summary>
        JsonResult GetVendor(int userID, int vendorID);

        /// <summary>Returns a FunctionResult</summary>
        JsonResult DeleteVendor(int userID, int loginID, int vendorID);

        /// <summary>Returns a FunctionResult.</summary>
        JsonResult SaveVendor(int userID, int loginID, VendorVM vendor);

        /// <summary>Returns a VendorFunctionResult.</summary>
        JsonResult ToggleVendorSelection(int userID, int loginID, int vendorID);
    }
}