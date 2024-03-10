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
using ProEstimator.Business.Model.Admin;
using System.Threading.Tasks;

namespace ProEstimator.Admin.Controllers
{
    public class SMSDispatcherController : AdminController
    {
        private AdminService _adminService;

        [Route("SMSDispatcher/List")]
        public ActionResult Index()
        {
            if (!AdminIsValidated())
            {
                Session["AfterLogin"] = "/SMSDispatcher/List";
                return Redirect("/LogOut");
            }
            else
            {
                return View();
            }
        }

        public JsonResult GetSmsHistory([DataSourceRequest] DataSourceRequest request)
        {
            List<SmsHistoryVM> smsHistoryVMs = new List<SmsHistoryVM>();
            _adminService = new AdminService();
            var model = _adminService.GetHistory();

            foreach (VmSmsHistory vmSmsHistory in model)
            {
                smsHistoryVMs.Add(new SmsHistoryVM(vmSmsHistory));
            }

            return Json(smsHistoryVMs.ToDataSourceResult(request));
        }

        // public ActionResult GetSmsHistoryByDates([DataSourceRequest] DataSourceRequest request, DateTime fromDate, DateTime toDate)
        public ActionResult GetSmsHistoryByDates(DateTime startDate, DateTime endDate)
        {
            List<SmsHistoryVM> smsHistoryVMs  = new List<SmsHistoryVM>();

            _adminService = new AdminService();
            var model = _adminService.GetHistoryByDate(startDate, endDate);

            foreach (VmSmsHistory vmSmsHistory in model)
            {
                smsHistoryVMs.Add(new SmsHistoryVM(vmSmsHistory));
            }

            return Json(smsHistoryVMs,JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public async Task<JsonResult> SendSms(string phoneNumber, string message, string salesRep, string salesRepId, string loginId)
        {
            _adminService = new AdminService();

            VmSmsHistory vmSmsHistory = new VmSmsHistory();
            vmSmsHistory.PhoneNumber = phoneNumber;
            vmSmsHistory.Message = message;
            vmSmsHistory.SalesRep = salesRep;
            vmSmsHistory.SalesRepId = Convert.ToInt32(salesRepId);
            vmSmsHistory.LoginId = Convert.ToInt32(loginId);

            await _adminService.SendAdminSms(vmSmsHistory);

            return Json(true, JsonRequestBehavior.AllowGet);
        }
    }
}