using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;
using ProEstimator.Admin.ViewModel.EmailReport;
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
using System.Threading.Tasks;

namespace ProEstimator.Admin.Controllers
{
    public class EmailReportController : AdminController
    {
        [Route("EmailReport/List")]
        public ActionResult Index()
        {
            EmailReportVM emailiReportVM = new EmailReportVM();

            return View(emailiReportVM);
        }

        public ActionResult GetEmailReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string emailAddressFilter, string emailSubjectFilter, string emailBodyFilter, string hasErrorFilter, string errorMessageFilter)
        {
            List<EmailReportVM> emailiReportVMs = new List<EmailReportVM>();

            loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
            rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
            rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;

            emailAddressFilter = string.IsNullOrEmpty(emailAddressFilter) ? null : emailAddressFilter;
            emailSubjectFilter = string.IsNullOrEmpty(emailSubjectFilter) ? null : emailSubjectFilter;
            emailBodyFilter = string.IsNullOrEmpty(emailBodyFilter) ? null : emailBodyFilter;
            hasErrorFilter = string.Compare(hasErrorFilter, "false", true) == 0 ? null : hasErrorFilter;
            errorMessageFilter = string.IsNullOrEmpty(errorMessageFilter) ? null : errorMessageFilter;

            List<Email> emails = Email.GetForFilter(loginIDFilter, rangeStartFilter, rangeEndFilter, emailAddressFilter, emailSubjectFilter, emailBodyFilter, hasErrorFilter, errorMessageFilter);

            foreach (Email email in emails)
            {
                emailiReportVMs.Add(new EmailReportVM(email));
            }

            return Json(emailiReportVMs.ToDataSourceResult(request));
        }

        [HttpGet]
        public ActionResult DownloadData(string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string emailAddressFilter, string emailSubjectFilter, string emailBodyFilter, string hasErrorFilter, string errorMessageFilter)
        {
            try
            {
                SpreadsheetWriter spreadsheetWriter = new SpreadsheetWriter();

                loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
                rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
                rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;

                emailAddressFilter = string.IsNullOrEmpty(emailAddressFilter) ? null : emailAddressFilter;
                emailSubjectFilter = string.IsNullOrEmpty(emailSubjectFilter) ? null : emailSubjectFilter;
                emailBodyFilter = string.IsNullOrEmpty(emailBodyFilter) ? null : emailBodyFilter;
                hasErrorFilter = string.Compare(hasErrorFilter, "false", true) == 0 ? null : hasErrorFilter;
                errorMessageFilter = string.IsNullOrEmpty(errorMessageFilter) ? null : errorMessageFilter;

                string startDateName = rangeStartFilter.Replace("/", string.Empty);
                string endDateName = rangeEndFilter.Replace("/", string.Empty);

                DataTable table = Email.GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, emailAddressFilter, emailSubjectFilter, emailBodyFilter, hasErrorFilter, errorMessageFilter);

                string adminFolder = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), "Admin");
                string diskPath = Path.Combine(adminFolder, "EmailReport_" + startDateName + "_" + endDateName + ".xlsx");

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
                return File(fileBytes, System.Net.Mime.MediaTypeNames.Application.Octet, "EmailReport_" + startDateName + "_" + endDateName + ".xlsx");
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "EmailReport_ DownloadData");
                return Content(ex.Message);
            }
        }

        #region Email Error Report

        [Route("EmailErrorReport/List")]
        public ActionResult EmailErrorReport()
        {
            EmailErrorReportVM emailReportVM = new EmailErrorReportVM();

            return View(emailReportVM);
        }

        public ActionResult GetEmailErrorReport([DataSourceRequest] DataSourceRequest request, string loginIDFilter, string rangeStartFilter, string rangeEndFilter,
                                                string emailAddressFilter, string eventFilter)
        {
            loginIDFilter = string.IsNullOrEmpty(loginIDFilter) ? null : loginIDFilter;
            rangeStartFilter = string.IsNullOrEmpty(rangeStartFilter) ? null : rangeStartFilter;
            rangeEndFilter = string.IsNullOrEmpty(rangeEndFilter) ? null : rangeEndFilter;
            emailAddressFilter = string.IsNullOrEmpty(emailAddressFilter) ? null : emailAddressFilter;
            eventFilter = string.IsNullOrEmpty(eventFilter) ? null : eventFilter;

            List<EmailErrorReportVM> emailErrorReportVMs = EmailErrorReportVM.GetForFilter(loginIDFilter, rangeStartFilter, rangeEndFilter, emailAddressFilter, eventFilter);

            return Json(emailErrorReportVMs.ToDataSourceResult(request));
        }

        #endregion

        #region Unsubscribe Email Report

        [Route("UnsubscribeEmailReport/List")]
        public ActionResult UnsubscribeEmailReport()
        {
            UnsubscribeEmailReportVM vm = new UnsubscribeEmailReportVM();

            return View(vm);
        }

        public ActionResult GetUnsubscribeReport([DataSourceRequest] DataSourceRequest request, string emailAddressFilter, string statusFilter)
        {
            List<UnsubscribeEmailReportVM> emailErrorReportVMs = UnsubscribeEmailReportVM.GetForFilter(emailAddressFilter, statusFilter);

            return Json(emailErrorReportVMs.ToDataSourceResult(request));
        }

        public async Task<JsonResult> DeleteUnsubscribe(int salesRepID, string email, bool isUnsubscribe, string evt)
        {
            FunctionResult result;
            EmailService es = new EmailService();
            if (isUnsubscribe)
            {
                result = await es.Delete(email);
            }
            else
            {
                result =  await es.Add(email);
            }
            if (result.Success)
            {
                List<Unsubscribe> Unsubscribes = Unsubscribe.GetUnsubscribeList(email, "");
                if (Unsubscribes.Count > 0)
                {
                    Unsubscribes[0].TimeStamp = DateTime.Now;
                    Unsubscribes[0].Event = evt;
                    Unsubscribes[0].EmailID = 0;
                    Unsubscribes[0].Save(ActiveLogin.ID);

                    string salesRepName = "Unknown";
                    SalesRep salesRep = SalesRep.Get(salesRepID);
                    if (salesRep != null)
                    {
                        salesRepName = salesRep.FirstName + " " + salesRep.LastName;
                    }
                    UnsubscribeHistory history = new UnsubscribeHistory(Unsubscribes[0].ID, evt, salesRepName);
                    history.Save(ActiveLogin.ID);
                }
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetUnsubscribeHistory([DataSourceRequest] DataSourceRequest request, int unsubscribeID)
        {
            List<UnsubscribeEmailReportVM> unsubscribes = new List<UnsubscribeEmailReportVM>();
            List<UnsubscribeHistory> historys = UnsubscribeHistory.GetUnsubscribeHistory(unsubscribeID);
            foreach(UnsubscribeHistory history in historys)
            {
                unsubscribes.Add(new UnsubscribeEmailReportVM(history.ID, history.TimeStamp, history.Event, history.SalesRep));
            }

            return Json(unsubscribes.ToDataSourceResult(request));
        }

        #endregion
    }
}
