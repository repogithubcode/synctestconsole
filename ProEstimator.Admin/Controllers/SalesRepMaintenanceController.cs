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
using ProEstimatorData.DataModel.Admin;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.DataModel;
using ProEstimator.Business.Logic;

namespace ProEstimator.Admin.Controllers
{
    public class SalesRepMaintenanceController : AdminController
    {
        private AdminService _adminService;

        [Route("SalesRepMaintenance/List")]
        public ActionResult Index()
        {
            SalesRepVM salesRepVM = new SalesRepVM();
            salesRepVM.SessionSalesRepID = GetSalesRepID();

            return View(salesRepVM);
        }

        public ActionResult GetSalesReps([DataSourceRequest] DataSourceRequest request)
        {
            List<SalesRepVM> salesRepVMs = new List<SalesRepVM>();

            List<SalesRep> salesReps = SalesRep.GetAll();

            foreach (SalesRep salesRep in salesReps)
            {
                salesRepVMs.Add(new SalesRepVM(salesRep));
            }

            return Json(salesRepVMs.ToDataSourceResult(request));
        }

        [HttpPost]
        public ActionResult InsertSalesRep(SalesRepVM salesRepVM)
        {
            _adminService = new AdminService();

            SalesRep salesRep = new SalesRep();
            salesRep.SalesNumber = salesRepVM.Number;
            salesRep.FirstName = salesRepVM.FirstName;
            salesRep.LastName = salesRepVM.LastName;
            salesRep.Email = salesRepVM.Email;

            _adminService.InsertSalesRep(salesRep);

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult UpdateSalesRep([DataSourceRequest] DataSourceRequest request, SalesRepVM salesRepVM)
        {
            _adminService = new AdminService();

            SalesRep salesRep = new SalesRep();
            salesRep.SalesNumber = salesRepVM.Number;
            salesRep.FirstName = salesRepVM.FirstName;
            salesRep.LastName = salesRepVM.LastName;
            salesRep.Email = salesRepVM.Email;
            salesRep.UserName = salesRepVM.UserName;
            salesRep.SalesRepID = salesRepVM.SalesRepID;
            salesRep.Password = salesRepVM.Password;

            _adminService.UpdateSalesRep(salesRep);

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public JsonResult DeleteRep(int id)
        {
            _adminService = new AdminService();
            _adminService.DeleteRep(id);

            return Json(new { Success = true }, JsonRequestBehavior.AllowGet);
        }
    }
}