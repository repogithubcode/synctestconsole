using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.IO;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimator.Business.Logic;
using Proestimator.ViewModel;

using Kendo.Mvc.UI;
using Kendo.Mvc.Extensions;

namespace Proestimator.Controllers
{
    public class QBExportController : SiteController
    {
        [HttpGet]
        [Route("{userID}/reports/qb-export")]
        public ActionResult QBExport(int userID)
        {
            //if (!ActiveLogin.HasQBExportContract)
            //{
            //    return Redirect("/" + loginID + "/reports");
            //}

            QBExportVM model = new QBExportVM();

            model.LoginID = ActiveLogin.LoginID;
            model.UserID = userID;

            model.StartDate = DateTime.Now.AddDays(-7).ToShortDateString();
            model.EndDate = DateTime.Now.ToShortDateString();

            ViewBag.NavID = "reports";
            ViewBag.settingsTopMenuID = 2;
            ViewBag.EstimateNavID = 2;

            return View(model);
        }

        public ActionResult GetData(
              [DataSourceRequest] DataSourceRequest request
            , int userID
            , string startDate
            , string endDate
        )
        {
            DateTime startDateDT = InputHelper.GetDateTime(startDate);
            DateTime endDateDT = InputHelper.GetDateTime(endDate);

            List<QBExportSummaryRow> summaryRows = new List<QBExportSummaryRow>();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    FunctionResult functionResult = QBExportSummaryRow.QBExportPreprocessing(activeLogin.LoginID, startDateDT, endDateDT);

                    if(functionResult.Success)
                    {
                        summaryRows = QBExportSummaryRow.GetDataSummary(activeLogin.LoginID, startDateDT, endDateDT);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "QB Export Controller GetData");
                }
            }

            return Json(summaryRows.ToDataSourceResult(request));
        }

        public ActionResult GetHistory(
              [DataSourceRequest] DataSourceRequest request
            , int userID, Boolean showDeletedQBExportLog
        )
        {
            List<QBExportHistoryVM> historyRecords = new List<QBExportHistoryVM>();

            if (IsUserAuthorized(userID))
            {
                try
                {
                    ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                    List<QBExportLog> exportLog = QBExportLog.GetForLogin(activeLogin.LoginID, showDeletedQBExportLog);
                    foreach(QBExportLog log in exportLog)
                    {
                        List<QBExportEstimateLog> estimateLogs = QBExportEstimateLog.GetByEstimateLog(log.ID);

                        QBExportHistoryVM historyVM = new QBExportHistoryVM();
                        historyVM.ID = log.ID;
                        historyVM.StartDate = log.StartDate;
                        historyVM.EndDate = log.EndDate;
                        historyVM.TimeStamp = log.TimeStamp;
                        historyVM.EstimateCount = estimateLogs.Count;
                        historyVM.IsDeleted = log.IsDeleted;

                        historyRecords.Add(historyVM);
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, 0, "QB Export Controller GetData");
                }
            }

            return Json(historyRecords.ToDataSourceResult(request));
        }

        public JsonResult ExportData(int userID, string startDate, string endDate, string selectedIDs)
        {
            try
            {
                DateTime startDateDT = InputHelper.GetDateTime(startDate);
                DateTime endDateDT = InputHelper.GetDateTime(endDate);

                List<int> selectedIDList = new List<int>();
                string[] pieces = selectedIDs.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                foreach(string piece in pieces)
                {
                    selectedIDList.Add(InputHelper.GetInteger(piece));
                }

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                QBExporter exporter = new QBExporter();
                QBExporterResult result = exporter.ExportXLSX(activeLogin.LoginID, startDateDT, endDateDT, selectedIDList);

                string downloadLink = "";
                if (result.Success)
                {
                    downloadLink = "/" + userID + "/reports/qb-export/" + result.ExportID.ToString();
                }

                return Json(new ExportDataResult(result.Success, result.ErrorMessage, downloadLink), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                return Json(new FunctionResult(ex.Message), JsonRequestBehavior.AllowGet);
            }
        }

        public class ExportDataResult : FunctionResult
        {
            public string DownloadLink { get; set; }

            public ExportDataResult(bool success, string errorMessage, string downloadLink)
            {
                Success = success;
                ErrorMessage = errorMessage;
                DownloadLink = downloadLink;
            }
        }

        [HttpGet]
        [Route("{userID}/reports/qb-export/{exportID}")]
        public ActionResult DownloadQBExport(int userID, int exportID)
        {
            if (exportID < 1)
            {
                return Content(@Proestimator.Resources.ProStrings.InvalidExportID);
            }

            QBExportLog exportLog = QBExportLog.Get(exportID);
            if (exportLog == null || exportLog.LoginID != ActiveLogin.LoginID)
            {
                return Content(@Proestimator.Resources.ProStrings.ExportDoesnotMatchLoginID);
            }

            string diskPath = exportLog.GetDiskPath();
            if (System.IO.File.Exists(diskPath))
            {
                return File(diskPath, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet.main+xml", "QBExport_" + ActiveLogin.LoginID.ToString() + "_" + exportLog.ID.ToString() + ".xlsx");
            }

            return Content(@Proestimator.Resources.ProStrings.CouldNotFindFindTryAgain);
        }

        public JsonResult DeleteRestoreQBExportLog(int userID, int loginID, string qbExportLogIDs, Boolean showDeletedQBExportLog)
        {
            CacheActiveLoginID(userID);
            char[] splitCharArr = new char[1];
            splitCharArr[0] = ',';
            int qbExportLogID = 0;

            string errorMessage = "";

            if (IsUserAuthorized(userID))
            {
                string[] qbExportLogIDsArr = qbExportLogIDs.Split(splitCharArr);

                foreach (string eachQBExportLogID in qbExportLogIDsArr)
                {
                    qbExportLogID = InputHelper.GetInteger(eachQBExportLogID);

                    if (qbExportLogID > 0)
                    {
                        ProEstimatorData.DataModel.QBExportLog qbExportLog = ProEstimatorData.DataModel.QBExportLog.Get(qbExportLogID);

                        if (qbExportLog != null)
                        {
                            if (qbExportLog.ID == qbExportLogID)
                            {
                                if (showDeletedQBExportLog == true)
                                    qbExportLog.Restore();
                                else
                                    qbExportLog.Delete();
                            }
                            else
                            {
                                errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, qbExportLogID);
                            }
                        }
                        else
                        {
                            errorMessage = string.Format(@Proestimator.Resources.ProStrings.ErrorDeletingReportInvalidReportID, qbExportLogID);
                        }
                    }
                }
            }

            return Json(errorMessage, JsonRequestBehavior.AllowGet);
        }
    }
}