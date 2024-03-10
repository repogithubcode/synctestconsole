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

namespace ProEstimator.Admin.Controllers
{
    public class LoginFailuresController : AdminController
    {
        [Route("LoginFailures/List")]
        public ActionResult Index()
        {
            LoginFailureVM loginFailureVM = new LoginFailureVM();

            return View(loginFailureVM);
        }

        public ActionResult GetLoginFailureReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string loginNameFilter, string passwordFilter)
        {
            List<LoginFailureVM> loginFailureVMs = new List<LoginFailureVM>();

            int loginID = InputHelper.GetInteger(loginIDFilter);

            List<LoginFailure> loginFailures = LoginFailure.GetForFilter(loginID, loginNameFilter ?? "", passwordFilter ?? "");

            foreach (LoginFailure loginFailure in loginFailures)
            {
                loginFailureVMs.Add(new LoginFailureVM(loginFailure));
            }

            return Json(loginFailureVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet);
        }
    }
}