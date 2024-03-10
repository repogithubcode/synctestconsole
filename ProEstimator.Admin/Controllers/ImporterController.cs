using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Text;
using System.Data;
using System.Data.SqlClient;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Admin.ViewModel;
using ProEstimator.Admin.ViewModel.Logins;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Admin;
using ProEstimator.Business.Logic.Admin;
using System.Net;
using ProEstimator.Admin.ViewModel.Import;

namespace ProEstimator.Admin.Controllers
{
    public class ImporterController : AdminController
    {
        private AdminService _adminService;
        private WebEstImportService _webEstImportService;

        [Route("Importer/List")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/Importer/List";
                return Redirect("/LogOut");
            }
            else
            {
                ImportVM importVM = new ImportVM();

                SalesRep salesRep = SalesRep.Get(GetSalesRepID());
                if (salesRep != null)
                {
                    importVM.SessionSalesRepID = salesRep.SalesRepID == 0 ? -1 : salesRep.SalesRepID;
                }

                return View(importVM);
            }
        }

        public JsonResult GetLoginInfo(string loginId, string message)
        {
            _adminService = new AdminService();
            var model = _adminService.GetLoginInfo(loginId, message);

            ImportVM importVM = new ImportVM(model);

            return Json(importVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImportLogin(string loginId, int salesRepId)
        {
            _adminService = new AdminService();
            var model = _adminService.ImportLogin(loginId, salesRepId);

            ImportVM importVM = new ImportVM(model);

            return Json(importVM, JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteLogin(string loginId)
        {
            _adminService = new AdminService();
            var model = _adminService.DeleteLogin(loginId);

            ImportVM importVM = new ImportVM(model);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImportContracts(string loginId, string estimateId)
        {
            _adminService = new AdminService();
            var model = _adminService.ImportContracts(loginId, estimateId);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ImportEstimate(string loginId, string estimateId)
        {
            _adminService = new AdminService();

            var model = _adminService.ImportEstimateViaQueue(loginId, estimateId);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

        public JsonResult ConversionComplete(int loginID)
        {
            _webEstImportService = WebEstImportService.Instance;

            var model = _webEstImportService.ConversionComplete(loginID);

            return Json(model, JsonRequestBehavior.AllowGet);
        }

    }
}