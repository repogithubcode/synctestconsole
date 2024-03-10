using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.DueInvoiceReport;
using ProEstimator.Business.Logic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class DueInvoiceReportController : AdminController
    {
        [Route("DueInvoiceReport/List")]
        public ActionResult Index()
        {
            DueInvoiceReportVM dueInvoiceReportVM = new DueInvoiceReportVM();

            return View(dueInvoiceReportVM);
        }

        public ActionResult GetDueInvoiceReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string rangeStartFilter,
                                                string rangeEndFilter, string hasCardErrorFilter, string autoPayFilter, string paidFilter, string hasStripeInfoFilter)
        {
            List<DueInvoiceReportVM> dueInvoiceReportVMs = new List<DueInvoiceReportVM>();

            loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
            rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
            rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;

            autoPayFilter = string.Compare(autoPayFilter, "false", true) == 0 ? null : autoPayFilter;
            paidFilter = string.Compare(paidFilter, "false", true) == 0 ? null : paidFilter;
            hasStripeInfoFilter = string.Compare(hasStripeInfoFilter, "false", true) == 0 ? null : hasStripeInfoFilter;
            hasCardErrorFilter = string.Compare(hasCardErrorFilter, "false", true) == 0 ? null : hasCardErrorFilter;

            List<DueInvoice> dueInvoices = DueInvoice.GetForFilter(loginIDFilter, rangeStartFilter, rangeEndFilter, hasCardErrorFilter, autoPayFilter, paidFilter, hasStripeInfoFilter);

            foreach (DueInvoice dueInvoice in dueInvoices)
            {
                dueInvoiceReportVMs.Add(new DueInvoiceReportVM(dueInvoice));
            }

            return Json(dueInvoiceReportVMs.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult DownloadData(string loginIDFilter, string rangeStartFilter,
                                                string rangeEndFilter, string hasCardErrorFilter, string autoPayFilter, string paidFilter, string hasStripeInfoFilter)
        {
            try
            {
                SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();

                loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
                rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
                rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;

                autoPayFilter = string.Compare(autoPayFilter, "false", true) == 0 ? null : autoPayFilter;
                paidFilter = string.Compare(paidFilter, "false", true) == 0 ? null : paidFilter;
                hasStripeInfoFilter = string.Compare(hasStripeInfoFilter, "false", true) == 0 ? null : hasStripeInfoFilter;
                hasCardErrorFilter = string.Compare(hasCardErrorFilter, "false", true) == 0 ? null : hasCardErrorFilter;

                string startDateName = rangeStartFilter.Replace("/", string.Empty);
                string endDateName = rangeEndFilter.Replace("/", string.Empty);

                DataTable table = DueInvoice.GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, hasCardErrorFilter, autoPayFilter, paidFilter, hasStripeInfoFilter);

                table.Columns.Remove("ContractID");
                table.Columns.Remove("StripeCardID");

                string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                string diskPath = Path.Combine(adminFolder, "DueInvoiceReport_" + startDateName + "_" + endDateName + ".xlsx");

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
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "DueInvoiceReport_" + startDateName + "_" + endDateName + ".xlsx");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "DueInvoiceReport DownloadData");
                return Content(ex.Message);
            }
        }

        public JsonResult BatchStripeInfoCardErrorRemove(string ids)
        {
            string[] pieces = ids.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            List<string> stripeInfoIDCollection = new List<string>(pieces);

            HashSet<string> uniqueStripeInfoIDColl = new HashSet<string>();
            foreach (string eachStripeInfoID in stripeInfoIDCollection)
            {
                if(!string.IsNullOrEmpty(eachStripeInfoID))
                {
                    uniqueStripeInfoIDColl.Add(eachStripeInfoID.Trim());
                }
            }

            int counter = 0;

            foreach (string eachStripeInfoID in uniqueStripeInfoIDColl)
            {
                int stripeInfoID = InputHelper.GetInteger(eachStripeInfoID);
                if (stripeInfoID > 0)
                {
                    StripeInfo stripeInfo = StripeInfo.GetStripeInfo(stripeInfoID);
                    if (stripeInfo != null)
                    {
                        stripeInfo.CardError = false;
                        stripeInfo.ErrorMessage = string.Empty;
                        stripeInfo.Save(ActiveLogin.ID);
                        counter++;
                    }
                }
            }

            return Json(counter + " stripe card error(s) deleted", JsonRequestBehavior.AllowGet);
        }

    }
}
