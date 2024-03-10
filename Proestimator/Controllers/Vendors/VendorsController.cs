using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using System.Data;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimatorData;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel;

using ProEstimator.DataRepositories.Vendors;

using Proestimator.ViewModel;
using Proestimator.ViewModel.Vendors;
using Proestimator.ViewModelMappers.Vendors;
using Proestimator.Controllers.Vendors.Commands;

namespace Proestimator.Controllers.Vendors
{

    public class VendorsController : SiteController, IVendorsController
    {
        private IVendorRepository _vendorService;

        public VendorsController(IVendorRepository vendorService)
        {
            _vendorService = vendorService;
        }

        [HttpGet]
        [Route("{userID}/vendors")]
        public ActionResult Index(int userID)
        {
            VendorIndexVM model = new VendorIndexVM();
            model.LoginID = ActiveLogin.LoginID;

            LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);

            model.ShowRepairFacility = loginInfo.ShowRepairShopProfiles;
            model.ShowAlternateIdentity = loginInfo.AllowAlternateIdentities;

            model.VendorTypes = GetVendorTypes(loginInfo);

            ViewBag.NavID = "vendors";

            return View(model);
        }

        private List<SimpleListItem> GetVendorTypes(LoginInfo loginInfo)
        {
            List<SimpleListItem> types = new List<SimpleListItem>();
            types.Add(new SimpleListItem("After", ((int)VendorType.AfterMarket).ToString()));
            types.Add(new SimpleListItem("OEM", ((int)VendorType.OEM).ToString()));
            types.Add(new SimpleListItem("LKQ", ((int)VendorType.LKQ).ToString()));
            types.Add(new SimpleListItem("Reman", ((int)VendorType.Reman).ToString()));

            return types;
        }

        public ActionResult GetVendorList([DataSourceRequest] DataSourceRequest request, int loginID, int typeID, bool publicVendors, string filterText)
        {
            List<Vendor> vendors;
            
            if (publicVendors)
            {
                vendors = _vendorService.GetUniversalList(loginID);
            }
            else
            {
                vendors = _vendorService.GetPrivateList(loginID, (VendorType)typeID);
            }

            if (!string.IsNullOrEmpty(filterText))
            {
                vendors = vendors.Where(o => o.CompanyName.ToLower().Contains(filterText.ToLower())).ToList();
            }

            vendors = vendors.OrderBy(o => o.State).ToList();

            VendorSummaryVMMapper mapper = new VendorSummaryVMMapper();

            List<VendorSummaryVM> summaryList = new List<VendorSummaryVM>();
            foreach (Vendor vendor in vendors)
            {
                summaryList.Add(mapper.Map(new VendorSummaryVMMapperConfiguration() { Vendor = vendor }));
            }

            return Json(summaryList.ToDataSourceResult(request));
        }

        /// <summary>Returns a VendorFunctionResult.</summary>
        public JsonResult GetVendor(int userID, int vendorID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                Vendor vendor = _vendorService.Get(vendorID);
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (vendor != null && (vendor.LoginsID == activeLogin.LoginID || vendor.Universal == true))
                {
                    return Json(new VendorFunctionResult(VendorToVM(vendor)), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new VendorFunctionResult("User not authorized."), JsonRequestBehavior.AllowGet);
        }

        private VendorVM VendorToVM(Vendor vendor)
        {
            VendorVMMapper mapper = new VendorVMMapper();
            VendorVM vm = mapper.Map(new VendorVMMapperConfiguration() { Vendor = vendor });
            return vm;
        }

        /// <summary>Returns a FunctionResult</summary>
        public JsonResult DeleteVendor(int userID, int loginID, int vendorID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SaveResult saveResult = _vendorService.Delete(vendorID, GetActiveLoginID(userID));

                if (saveResult.Success)
                {
                    return Json(new FunctionResult(), JsonRequestBehavior.AllowGet);
                }
                else
                {
                    return Json(new FunctionResult(saveResult.ErrorMessage), JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new FunctionResult("Not authorized."), JsonRequestBehavior.AllowGet);
        }

        /// <summary>Returns a FunctionResult.</summary>
        public JsonResult ToggleVendorSelection(int userID, int loginID, int vendorID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SiteActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                SaveResult saveResult = _vendorService.ToggleSelection(loginID, vendorID, activeLogin.ID);
                  
                return Json(new FunctionResult(saveResult.Success, saveResult.ErrorMessage), JsonRequestBehavior.AllowGet);
            }

            return Json(new FunctionResult("Not authorized"), JsonRequestBehavior.AllowGet);
        }

        /// <summary>Returns a VendorFunctionResult</summary>
        public JsonResult SaveVendor(int userID, int loginID, VendorVM vendorVM)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                SaveVendorCommand command = new SaveVendorCommand(_vendorService, GetActiveLoginID(userID), loginID, vendorVM);
                command.Execute();
                return Json(command.VendorFunctionResult, JsonRequestBehavior.AllowGet);
            }

            return Json(new VendorFunctionResult("Not authorized."), JsonRequestBehavior.AllowGet);
        }

    }
}