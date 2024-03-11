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
    public class PdrReportController : AdminController
    {
        [Route("PdrReport/List")]
        public ActionResult Index()
        {
            PdrReportVM pdrReportVM = new PdrReportVM();

            return View(pdrReportVM);
        }

        public ActionResult GetPdrReport([DataSourceRequest] DataSourceRequest request, string startDateFilter, string endDateFilter)
        {
            List<PdrReportVM> pdrReportVMs = new List<PdrReportVM>();
            
            List<PdrReport> pdrReports = PdrReport.GetForFilter();

            pdrReports = pdrReports.Where(x => DateTime.Parse(x.Date) >= Convert.ToDateTime(startDateFilter) 
                                                && DateTime.Parse(x.Date) <= Convert.ToDateTime(endDateFilter).AddDays(1)).ToList()
                                                .OrderBy(pdrReport => pdrReport.Date).ToList<PdrReport>();

            foreach (PdrReport pdrReport in pdrReports)
            {
                pdrReportVMs.Add(new PdrReportVM(pdrReport));
            }

            return Json(pdrReportVMs.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult DownloadData(string startDateFilter, string endDateFilter)
        {
            if (AdminIsValidated())
            {
                try
                {
                    SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();
                    DataTable table = PdrReport.GetForFilterData();

                    table = table.AsEnumerable().Where(row => (row.Field<DateTime>("Date") >= Convert.ToDateTime(startDateFilter))
                                                && (row.Field<DateTime>("Date") <= Convert.ToDateTime(endDateFilter).AddDays(1))).CopyToDataTable();

                    DataView dataView = new DataView(table);
                    dataView.Sort = "Date";
                    table = dataView.ToTable();

                    string startDateName = startDateFilter.Replace("/", string.Empty);
                    string endDateName = endDateFilter.Replace("/", string.Empty);

                    string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                    string diskPath = Path.Combine(adminFolder, "PdrReport_" + startDateName + "_" + endDateName + ".xlsx");

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
                        return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "PdrReport_" + startDateName + "_" + endDateName + ".xlsx");
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "PdrReport DownloadData");
                    return Content(ex.Message);
                }
            }

            return Content("Error generating data.");
        }
    }
}
