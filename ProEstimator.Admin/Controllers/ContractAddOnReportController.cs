using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel;
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
    public class ContractAddOnReportController : AdminController
    {
        [Route("Reports/ContractAddOnReport")]
        public ActionResult Index()
        {
             ContractAddOnReportVM  contractAddOnReportVM = new  ContractAddOnReportVM();

            return View(contractAddOnReportVM);
        }

        public ActionResult GetContractAddOnReport([DataSourceRequest] DataSourceRequest request, string addOnDateFilter)
        {
            List<ContractAddOnReportVM>  ContractAddOnReportVMs = new List<ContractAddOnReportVM>();
            
            List<ContractAddOnReport> contractAddOnReports = ContractAddOnReport.GetForFilter(addOnDateFilter);

            foreach (ContractAddOnReport contractAddOnReport in contractAddOnReports)
            {
                ContractAddOnReportVMs.Add(new  ContractAddOnReportVM(contractAddOnReport));
            }

            return Json( ContractAddOnReportVMs.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult DownloadData(string addOnDateFilter)
        {
            try
            {
                SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();
                DataTable table = ContractAddOnReport.GetForFilterData(addOnDateFilter);

                if(table.Columns.Contains("AddOnTypeID"))
                {
                    table.Columns.Remove("AddOnTypeID");
                }

                string dateName = addOnDateFilter.Replace("/", string.Empty);

                string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                string diskPath = Path.Combine(adminFolder, "ContractAddOnReport_" + dateName + ".xlsx");

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
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "ContractAddOnReport_" + dateName + ".xlsx");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, " ContractAddOnReport DownloadData");
                return Content(ex.Message);
            }
        }
    }
}
   