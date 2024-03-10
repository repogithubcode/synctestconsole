using Elmah;
using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.Errors;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using ErrorLog = ProEstimatorData.DataModel.Admin.ErrorLog;

namespace ProEstimator.Admin.Controllers
{
    public class ErrorLogReportController : AdminController
    {
        [Route("ErrorLogReport/List")]
        public ActionResult Index()
        {
            ErrorLogReportVM errorLogReportVM = new ErrorLogReportVM();

            errorLogReportVM.ErrorTagDDL = new SelectList(GetErrorTagList(), "Value", "Text");
            errorLogReportVM.SelectedErrorTag = "";

            return View(errorLogReportVM);
        }

        public ActionResult GetErrorLogReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string adminInfoIDFilter,
                                                string rangeStartFilter, string rangeEndFilter, string errorTagFilter, string errorTextFilter)
        {
            // LoginID
            int loginID = 0;
            Boolean loginIdTryParseVal = Int32.TryParse(loginIDFilter, out loginID);

            // AdminInfoID
            int adminInfoID = 0;
            Boolean adminInfoIdTryParseVal = Int32.TryParse(adminInfoIDFilter, out adminInfoID);

            List<ErrorLogReportVM> errorLogReportVms = new List<ErrorLogReportVM>();

            List<ErrorLog> errorLogs = ErrorLog.GetForFilter(loginID, adminInfoID , rangeStartFilter ?? "", rangeEndFilter ?? "", 
                                                                                            errorTagFilter ?? "", errorTextFilter ?? "");

            foreach (ErrorLog errorLog in errorLogs)
            {
                errorLogReportVms.Add(new ErrorLogReportVM(errorLog));
            }

            return Json(errorLogReportVms.ToDataSourceResult(request));
        }

        private IEnumerable<SelectListItem> GetErrorTagList()
        {
            List<ErrorTag> errorTags = ErrorLog.GetDistinctErrorTagList();
            errorTags.Insert(0,new ErrorTag("",-1));

            var GetErrorTagData = errorTags.Select(errorTag => (errorTag.ErrorTagText != "") ? new SelectListItem()
            {
                Text = errorTag.ErrorTagText + " (" + errorTag.ErrorTagCount.ToString() + ")",
                Value = errorTag.ErrorTagText
            } : new SelectListItem()
            {
                Selected = true,
                Text = "",
                Value = errorTag.ErrorTagText
            });

            return GetErrorTagData;
        }
    }
}
