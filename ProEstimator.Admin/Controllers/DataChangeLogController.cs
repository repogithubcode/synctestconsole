using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using ProEstimator.Admin.ViewModel.DataChangeLog;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.Controllers
{
    public class DataChangeLogController : AdminController
    {

        public ActionResult Index()
        {
            DataChangeLogPageVM vm = new DataChangeLogPageVM();
            return View(vm);
        }

        public ActionResult GetHistoryReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string dateStartFilter, string dateEndFilter, string itemTypeFilter, string itemIDFilter)
        {
            int loginID = InputHelper.GetInteger(loginIDFilter);
            DateTime dateStart = InputHelper.GetDateTime(dateStartFilter);
            DateTime dateEnd = InputHelper.GetDateTime(dateEndFilter);
            int itemID = InputHelper.GetInteger(itemIDFilter);

            List<ChangeLogRowSummary> changeLogRows = ChangeLogRowSummary.GetForSearch(loginID, dateStart, dateEnd, itemTypeFilter, itemID);

            return Json(changeLogRows.ToDataSourceResult(request));
        }

        public JsonResult GetChangeLogDetails(int changeLogID)
        {
            ChangeLogDetailsVM vm = new ChangeLogDetailsVM();

            ChangeLogItemDetails details = ChangeLogItemDetails.GetForID(changeLogID);
            vm.Browser = details.Browser;
            vm.ComputerKey = details.ComputerKey;
            vm.Device = details.DeviceType;
            vm.EmailAddress = details.EmailAddress;
            vm.SiteUser = details.Name;
            vm.TimeStamp = details.TimeStamp.ToShortDateString() + " " + details.TimeStamp.ToShortTimeString();

            return Json(vm, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetChangeLogDetailsGrid([DataSourceRequest] DataSourceRequest request, int changeLogID)
        {
            List<ChangeLogItem> details = ChangeLogItem.GetForChangeLog(changeLogID);

            return Json(details.ToDataSourceResult(request));
        }
    }
}