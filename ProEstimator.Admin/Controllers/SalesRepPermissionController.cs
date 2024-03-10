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
using ProEstimator.Business.Model.Admin;
using ProEstimator.Admin.ViewModel.SalesRep;
using ProEstimator.Business.Logic.Admin;

namespace ProEstimator.Admin.Controllers
{
    public class SalesRepPermissionController : AdminController
    {
        private AdminService _adminService;

        [HttpGet]
        [Route("SalesRepPermission/List/{id}")]
        public ActionResult Index(int id)
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/SalesRepPermission/List/" + id;
                return Redirect("/LogOut");
            }
            else
            {
                SalesRepPermissionsVM vm = new SalesRepPermissionsVM();

                if (ProEstimator.Business.Logic.Admin.SalesRepPermissionManager.HasPermission(GetSalesRepID(), "RepPermissions"))
                {
                    vm.SetSalesRepID(id);

                    vm.SessionSalesRepID = GetSalesRepID();

                    vm.SalesRepPermissionLookupDDL = new SelectList(GetSalesRepPermissionOptionList(), "Value", "Text");
                    vm.SelectedSalesRepPermission = id;
                }

                return View(vm);
            }
        }


        [HttpPost]
        [Route("SalesRepPermission/List/{id}")]
        public ActionResult Index(SalesRepPermissionsVM vm)
        {
            foreach(SalesRepPermissionGroupVM groupVM in vm.PermissionGroups)
            {
                foreach(SalesRepPermissionVM permissionVM in groupVM.Permissions)
                {
                    SalesRepPermissionManager.SetPermission(vm.SelectedSalesRepPermission, permissionVM.Tag, permissionVM.HasPermission);
                }
            }

            return Redirect("/SalesRepPermission/List/" + vm.SelectedSalesRepPermission);
        }

        private IEnumerable<SelectListItem> GetSalesRepPermissionOptionList()
        {
            List<SalesRepPermissionLookup> SalesRepPermissionLookup = LookupService.GetSalesRepPermissionOptionList();

            List <SelectListItem> SalesRepPermissionLookupDDL = new List<SelectListItem>();
            foreach (SalesRepPermissionLookup salesRepPermissionLookup in SalesRepPermissionLookup)
            {
                SalesRepPermissionLookupDDL.Add(new SelectListItem()
                {
                    Text = Convert.ToString(salesRepPermissionLookup.Description),
                    Value = Convert.ToString(salesRepPermissionLookup.Type)
                });
            }

            return SalesRepPermissionLookupDDL;
        }
    }
}