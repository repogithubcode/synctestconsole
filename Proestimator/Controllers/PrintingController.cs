using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;

using Telerik.Reporting.Processing;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Profiles;
using ProEstimatorData.Models.SubModel;

using ProEstimator.Business.Logic;

using Proestimator.ViewModelMappers.Printing;
using Proestimator.ViewModel;
using Proestimator.ViewModel.Printing;
using Proestimator.Helpers;
using System.Threading.Tasks;

namespace Proestimator.Controllers
{
    public class PrintingController : SiteController
    {

        private IEstimateService _estimateService;

        public PrintingController(IEstimateService estimateService)
        {
            _estimateService = estimateService;
        }

        #region Printing

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/printing")]
        public ActionResult Index(int userID, int estimateID)
        {
            // Make sure the estimate belongs to the login
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID != ActiveLogin.LoginID)
            {
                return Redirect("/" + userID);
            }

            PrintingVM model = new PrintingVM(ActiveLogin.LoginID, estimateID, ActiveLogin.HasEMSContract, ActiveLogin.IsTrial);
            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;
            model.DownloadPDF = (PDFDownloadSetting)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "DownloadPDF", "ReportOptions", ((int)PDFDownloadSetting.OpenNewTab).ToString()).ValueString);

            if (_estimateService.IsEstimateTotalLoss(estimateID))
                model.ReportCreator.SelectedReportHeader = "Total Loss";

            if (estimate.EstimateID > 0)
            {
                if (estimate.CustomerProfileID == 0)
                {
                    return RedirectToAction("SelectRateProfile");
                }                
            }

            ViewBag.Target = model.DownloadPDF == PDFDownloadSetting.Download ? "_self" : "_blank";

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "printing";

            ViewBag.EstimateID = estimateID;

            return View(model);
        }       

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/printing")]
        public ActionResult SavePrinting(PrintingVM model)
        {
            SavePrintingData(model, ActiveLogin.ID);
            return DoRedirect("Printing");
        }

        private void SavePrintingData(PrintingVM model, int activeLoginID)
        {
            Estimate estimate = new Estimate(model.EstimateID);
            estimate.Description = string.IsNullOrEmpty(model.ReportCreator.PrintDescription) ? "" : model.ReportCreator.PrintDescription;
            estimate.ReportTextHeader = model.ReportCreator.SelectedReportHeader;
            //estimate.ImageSize = model.ImageSize;
            estimate.Save(GetActiveLoginID(activeLoginID));
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/make-and-view")]
        public async Task<ActionResult> MakeAndViewNewReport(int userID, int estimateID)
        {
            ReportFunctionResult result = await _estimateService.MakeQuickPrintEstimateReport(userID, ActiveLogin.LoginID, estimateID, ActiveLogin.ID);
            if (result.Success)
            {
                return Redirect("/" + userID + "/estimate/" + estimateID + "/view-attachment/" + result.Report.ID + "/" + ProEstimatorData.DataModel.Report.GetReportName(estimateID, result.Report.ReportType.Text) + "." + result.Report.GetFileExtension());
            }

            return RedirectToAction("Printing", "Estimate");
        }

        public JsonResult DeleteReport(int userID, int loginID, int reportID, Boolean restoreDeletedReport)
        {
            CacheActiveLoginID(userID);

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(reportID);
                if (report.ReportBelongsToLogin(loginID))
                {
                    if (restoreDeletedReport == true)
                        report.Restore();
                    else
                        report.Delete();
                }
                else
                {
                    errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, reportID);
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet); 
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/totals-report")]
        public ActionResult EstimateTotals(int userID, int estimateID)
        {
            Estimate estimate = new Estimate(estimateID);
            if (estimate.CreatedByLoginID == ActiveLogin.LoginID)
            {
                try
                {
                    // Set up a Report Source for the report file
                    var uriReportSource = new Telerik.Reporting.UriReportSource();
                    uriReportSource.Uri = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Reports", "EstimateTotals.trdx");

                    // Pass the report parameters
                    uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("AdminInfoID", estimate.EstimateID));
                    uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("SupplementVersion", estimate.LockLevel));
                    uriReportSource.Parameters.Add(new Telerik.Reporting.Parameter("PdrOnly", false));

                    // Generate the PDF
                    ReportProcessor reportProcessor = new ReportProcessor();
                    RenderingResult result = reportProcessor.RenderReport("PDF", uriReportSource, null);

                    MemoryStream stream = new MemoryStream(result.DocumentBytes);
                    return new FileStreamResult(stream, "application/pdf");
                }
                catch (System.Exception ex)
                {
                    return View(ex.Message.Contains("Check the InnerException") ? ex.InnerException.Message : ex.Message);
                }
            }

            return View("Error: report could not be generated.");
        }

        #endregion

        /// <summary>
        /// Returns the list of saved Estimate reports, used to bind to the grid
        /// </summary>
        public ActionResult GetReportHistory([DataSourceRequest] DataSourceRequest request, int loginID, int estimateID, bool showDeletedReport, string reportType)
        {
            List<ReportHistoryRowVM> reportList = GetReportHistory(loginID, estimateID, showDeletedReport);

            if (reportType.StartsWith("Custom-"))
            {
                int templateID = InputHelper.GetInteger(reportType.Replace("Custom-", ""));
                reportList = reportList.Where(o => o.CustomTemplateID == templateID).ToList();
            }
            else if (reportType != "")
            {
                reportList = reportList.Where(o => o.ReportTypeTag == reportType).ToList();
            }

            return Json(reportList.OrderByDescending(o => o.TimeStamp).ToDataSourceResult(request));
        }

        private List<ReportHistoryRowVM> GetReportHistory(int loginID, int estimateID, bool deleted)
        {
            List<ReportHistoryRowVM> reportList = new List<ReportHistoryRowVM>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<System.Data.SqlClient.SqlParameter>();
            parameters.Add(new System.Data.SqlClient.SqlParameter("EstimateID", estimateID));
            parameters.Add(new System.Data.SqlClient.SqlParameter("ShowDeletedOnly", deleted));

            DBAccessTableResult result = db.ExecuteWithTable("Report_GetEstimateReportSummary", parameters);

            int timezoneOffset = InputHelper.GetInteger(SiteSettings.Get(loginID, "TimeZone", "ReportOptions", "0").ValueString);
            ReportHistoryRowVMMapper mapper = new ReportHistoryRowVMMapper();

            foreach (DataRow row in result.DataTable.Rows)
            {
                reportList.Add(mapper.Map(new ReportHistoryRowVMMapperConfiguration() { Row = row, TimezoneOffset = timezoneOffset }));
            }

            return reportList.OrderByDescending(o => o.TimeStamp).ToList();
        }

        public JsonResult GetReportTypeList(int userID, int loginID, int estimateID, bool deletedReports)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                List<ReportHistoryRowVM> reportList = GetReportHistory(loginID, estimateID, deletedReports);

                List<ReportType> reportTypes = ReportType.GetAll();
                List<DropDownData> dataItems = new List<DropDownData>();

                foreach (ReportHistoryRowVM reportVM in reportList.Where(o => o.ReportTypeTag != "Estimate"))
                {
                    string shortName = reportVM.ReportTypeTag;
                    string longName = "";

                    if (reportVM.ReportTypeTag == "Custom")
                    {
                        shortName = "Custom-" + reportVM.CustomTemplateID;
                        longName = reportVM.ReportType;
                    }
                    else
                    {
                        ReportType reportType = reportTypes.FirstOrDefault(o => o.Tag == reportVM.ReportTypeTag);
                        if (reportType != null)
                        {
                            shortName = reportType.Tag;
                            longName = reportType.Text;
                        }
                    }

                    if (dataItems.FirstOrDefault(o => o.Value == shortName) == null)
                    {
                        dataItems.Add(new DropDownData(shortName, longName));
                    }
                }

                // Make a list to return.  Start with All and Estimate, then add the rest in alphabetical order
                List<DropDownData> data = new List<DropDownData>();
                data.Add(new DropDownData("", "All"));

                if (reportList.FirstOrDefault(o => o.ReportTypeTag == "Estimate") != null)
                {
                    data.Add(new DropDownData("Estimate", "Estimate"));
                }

                data.AddRange(dataItems.OrderBy(o => o.Text));

                return Json(data, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Returns the list of Labor Type Technician List
        /// </summary>
        public ActionResult GetWOLaborTypeTechnicianList([DataSourceRequest] DataSourceRequest request, int userID, int estimateID)
        {
            List<WorkOrderReportVM> workOrderReportVMList = new List<WorkOrderReportVM>();

            List<TechnicianVM> technicianList = new List<TechnicianVM>();
            List<Technician> printTechnicianMappingList = new List<Technician>();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());
                List<Technician> technicians = Technician.GetByLogin(activeLogin.LoginID);
                foreach (Technician technician in technicians)
                {
                    technicianList.Add(new TechnicianVM(technician));
                }

                printTechnicianMappingList = Technician.GetPrintTechniciansMapping(estimateID);
            }

            List<SimpleListItem> laborTypeList = ProEstHelper.GetLaborTypeList(estimateID);
            foreach (SimpleListItem eachSimpleListItem in laborTypeList)
            {
                WorkOrderReportVM workOrderReportVM = new WorkOrderReportVM();
                workOrderReportVM.LaborTypeID = InputHelper.GetInteger(eachSimpleListItem.Value);
                workOrderReportVM.LaborTypeText = InputHelper.GetString(eachSimpleListItem.Text);

                Technician printTechnicianMapping = printTechnicianMappingList.Where(eachPrintTechnicianMapping =>
                                                            eachPrintTechnicianMapping.LaborTypeID == workOrderReportVM.LaborTypeID).
                                                            FirstOrDefault();

                if(printTechnicianMapping != null && printTechnicianMapping.ID > -1)
                {
                    workOrderReportVM.Select = true;
                    workOrderReportVM.SelectedTechnicianID = printTechnicianMapping.ID;
                }

                workOrderReportVM.TechnicianVMList = new List<TechnicianVM>();

                List<TechnicianVM> technicianListFiltered = technicianList.
                                                            Where(eachTechnicianVM =>
                                                            eachTechnicianVM.LaborTypeID == workOrderReportVM.LaborTypeID).ToList<TechnicianVM>(); ;
                 
                workOrderReportVM.TechnicianVMList.AddRange(technicianListFiltered);

                workOrderReportVMList.Add(workOrderReportVM);
            }

            WorkOrderReportVM bodyWOReportVM = workOrderReportVMList.Where(eachWorkOrderReportVM => eachWorkOrderReportVM.LaborTypeText == "Body").SingleOrDefault();
            WorkOrderReportVM aluminumWOReportVM = workOrderReportVMList.Where(eachWorkOrderReportVM => eachWorkOrderReportVM.LaborTypeText == "Aluminum").SingleOrDefault();

            if(bodyWOReportVM != null && aluminumWOReportVM != null)
            {
                List<TechnicianVM> technicianVMs = new List<TechnicianVM>();
                technicianVMs.AddRange(aluminumWOReportVM.TechnicianVMList);

                technicianVMs.AddRange(bodyWOReportVM.TechnicianVMList.Where(each => each.ID != 0));
                aluminumWOReportVM.TechnicianVMList = technicianVMs;
            }

            return Json(workOrderReportVMList.ToDataSourceResult(request));
        }

        public JsonResult SavePrintTechniciansMapping(int userID, int estimateID, string selectedTechnicians)
        {
            CacheActiveLoginID(userID);

            SaveResult result = new SaveResult();

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (!string.IsNullOrEmpty(selectedTechnicians) && selectedTechnicians.Length > 0)
                {
                    Technician technician = new Technician();
                    technician.EstimateID = estimateID;

                    char splitCharTild = '~';
                    char splitCharPipe = '!';
                    string[] selectedTechniciansTildArr = selectedTechnicians.Split(splitCharTild);

                    foreach (string eachSelectedTechnician in selectedTechniciansTildArr)
                    {
                        string[] selectedTechniciansPipeArr = eachSelectedTechnician.Split(splitCharPipe);

                        if(selectedTechniciansPipeArr.Length >= 2)
                        {
                            technician.LaborTypeID = InputHelper.GetInteger(selectedTechniciansPipeArr[0]); // laborTypeID
                            technician.ID = InputHelper.GetInteger(selectedTechniciansPipeArr[2]); // technicianID

                            var functionResult = technician.SavePrintTechniciansMapping(activeLogin.LoginID);

                            if (!functionResult.Success)
                            {
                                result = new SaveResult(functionResult.ErrorMessage);
                                return Json(result, JsonRequestBehavior.AllowGet);
                            }
                        }
                    }
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);

        }
    }
}