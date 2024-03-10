using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using System.Linq;
using System.Configuration;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimator.Business.Payments;

using System.Net.Http;
using System.IO;
using System.Data;
using ProEstimatorData.DataAdmin;
using ProEstimator.Admin.ViewModel;
using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace ProEstimator.Admin.Controllers
{
    public class TrialReportController : AdminController
    {
        public TrialReportController()
        {

        }

        [Route("TrialReport/List")]
        public ActionResult Index()
        {
            TrialReportVM trialReportVM = new TrialReportVM();

            List<SalesRepPermissionLookup> salesReps = LookupService.GetSalesRepPermissionOptionList();
            trialReportVM.SalesRepDDL = ConvertToSelectListItem(salesReps);
            trialReportVM.SelectedSalesRep = trialReportVM.SalesRepDDL[0].Value;

            trialReportVM.TotalWeTrials = 0;
            trialReportVM.TotalActiveWeTrials = 0;
            trialReportVM.TotalWeTrialsConverted = 0;

            trialReportVM.SalesRepName = Convert.ToString(ViewBag.SalesRepName).ToUpper();

            return View(trialReportVM);
        }

        public JsonResult GetTrialsByDates([DataSourceRequest] DataSourceRequest request, DateTime fromDate, DateTime toDate, int repId)
        {
            List<TrialRecordVM> trialRecordsVMs = TempData["TrialRecordsVM"] as List<TrialRecordVM>;

            return Json(trialRecordsVMs.ToDataSourceResult(request));
        }

        private List<SelectListItem> ConvertToSelectListItem(List<SalesRepPermissionLookup> salesReps)
        {
            List<SelectListItem> salesRepListItems = new List<SelectListItem>();

            foreach (SalesRepPermissionLookup salesRepPermissionLookup in salesReps)
            {
                salesRepListItems.Add(new SelectListItem { Text = salesRepPermissionLookup.Description, Value = salesRepPermissionLookup.Type });
            }

            return salesRepListItems;
        }

        public JsonResult GetTotalTrialsAndCharts(DateTime fromDate, DateTime toDate, int repId)
        {
            List<TrialReport> trialReports = TrialReport.GetForFilter(fromDate, toDate);

            TrialReportVM trialReportVM = TrialReportVM.GetTrials(trialReports, repId);

            TempData["TrialReportVM"] = trialReportVM;
            TempData["TrialRecordsVM"] = trialReportVM.Records;

            return Json(new { Success = true, TrialReportVMObject = trialReportVM }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult DownloadData(string fromDate, string toDate, int repId)
        {
            if (AdminIsValidated())
            {
                try
                {
                    SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();
                    List<TrialReport> trialReports = TrialReport.GetForFilter(Convert.ToDateTime(fromDate), Convert.ToDateTime(toDate));
                    TrialReportVM trialReportVM = TrialReportVM.GetTrials(trialReports, repId);

                    DataTable table = TrialRecordVM.ToDataTable(trialReportVM.Records);

                    string startDateName = fromDate.Replace("/", string.Empty);
                    string endDateName = toDate.Replace("/", string.Empty);

                    string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                    string diskPath = Path.Combine(adminFolder, "TrialReport" + startDateName + "_" + endDateName + "_" + repId + ".xlsx");

                    if (!Directory.Exists(adminFolder))
                    {
                        Directory.CreateDirectory(adminFolder);
                    }

                    if (System.IO.File.Exists(diskPath))
                    {
                        System.IO.File.Delete(diskPath);
                    }

                    spreadsheetWriter.WriteSpreadshet(table, diskPath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "TrialReport_" + startDateName + "_" + endDateName + "_" + repId + ".xlsx");
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "TrialReport DownloadData");
                    return Content(ex.Message);
                }
            }

            return Content("Error generating data.");
        }

    }
}