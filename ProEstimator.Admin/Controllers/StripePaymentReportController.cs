using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.Errors;
using ProEstimator.Admin.ViewModel.Payment;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Logic.Admin;
using ProEstimator.Business.Model.Admin.Reports;
using ProEstimatorData;
using ProEstimatorData.DataModel.Admin;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Mvc;

namespace ProEstimator.Admin.Controllers
{
    public class StripePaymentReportController : AdminController
    {
        [Route("StripePaymentReport/List")]
        public ActionResult Index()
        {
            StripePaymentReportVM stripePaymentReportVM = new StripePaymentReportVM();

            return View(stripePaymentReportVM);
        }

        public ActionResult GetStripePaymentReport([DataSourceRequest] DataSourceRequest request, string startDateFilter, string endDateFilter)
        {
            List<StripePaymentReportVM> stripePaymentReportVMs = new List<StripePaymentReportVM>();
            
            List<StripePayment> stripePayments = StripePayment.GetForFilter(startDateFilter ?? "", endDateFilter ?? "");

            foreach (StripePayment stripePayment in stripePayments)
            {
                stripePaymentReportVMs.Add(new StripePaymentReportVM(stripePayment));
            }

            return Json(stripePaymentReportVMs.ToDataSourceResult(request), JsonRequestBehavior.AllowGet); 
        }

        [HttpGet]
        public ActionResult DownloadData(string startDateFilter, string endDateFilter)
        {
            try
            {
                SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();
                DataTable table = StripePayment.GetForFilterData(startDateFilter ?? "", endDateFilter ?? "");

                string startDateName = startDateFilter.Replace("/", string.Empty);
                string endDateName = endDateFilter.Replace("/", string.Empty);

                string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                string diskPath = Path.Combine(adminFolder, "StripePaymentReport_" + startDateName + "_" + endDateName + ".xlsx");

                if (!Directory.Exists(adminFolder))
                {
                    Directory.CreateDirectory(adminFolder);
                }

                if(Path.GetFullPath(diskPath).StartsWith(adminFolder, StringComparison.OrdinalIgnoreCase))
                {
                    if (System.IO.File.Exists(diskPath))
                    {
                        System.IO.File.Delete(diskPath);
                    }

                    spreadsheetWriter.WriteSpreadshet(table, diskPath);

                    byte[] fileBytes = System.IO.File.ReadAllBytes(diskPath);
                    return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "StripePaymentReport_" + startDateName + "_" + endDateName + ".xlsx");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "StripePaymentReport DownloadData");
                return Content(ex.Message);
            }

            return Content("Error generating data.");
        }
    }
}
