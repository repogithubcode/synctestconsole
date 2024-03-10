using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Text;
using System.Data;
using System.Data.SqlClient;
using System.Configuration;

using Kendo.Mvc.Extensions;
using Kendo.Mvc.UI;

using Proestimator.Helpers;
using Proestimator.Resources;
using Proestimator.ViewModel.SendEstimate;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Logic;
using Proestimator.ViewModel;
using Proestimator.ViewModel.SendEstimate.EMSExport;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimatorData.Models;
using ProEstimatorData.Models.EditorTemplateModel;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.DataModel.PDR;
using ProEstimatorData.DataModel.Profiles;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Proestimator.Controllers
{
    public class SendEstimateController : SiteController
    {

        #region Email / SMS send

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/send-estimate/{emailID?}")]
        public ActionResult Index(int userID, int estimateID, int emailID = 0)
        {
            SendEstimateVM model = new SendEstimateVM(ActiveLogin.LoginID, estimateID, ActiveLogin.HasEMSContract, ActiveLogin.IsTrial);

            // Make sure the account has an email address set up to send messages from
            ProEstimatorData.DataModel.Contact contact = ProEstimatorData.DataModel.Contact.GetContactForLogins(ActiveLogin.LoginID);
            if (contact != null && !string.IsNullOrEmpty(contact.Email))
            {
                model.HasAccountEmailAddress = true;

                if (estimateID > 0)
                {
                    try
                    {
                        Estimate estimate = new Estimate(estimateID);
                        if (estimate == null || estimate.CreatedByLoginID != ActiveLogin.LoginID)
                        {
                            return Redirect("/");
                        }

                        if (estimate.CustomerProfileID == 0)
                        {
                            return RedirectToAction("SelectRateProfile", "Estimate");
                        }

                        model.SelectedEmailID = emailID;
                        model.LoginID = ActiveLogin.LoginID;
                        model.EstimateID = estimateID;

                        // Set the default subject line
                        Customer customer = Customer.Get(estimate.CustomerID);

                        if (!string.IsNullOrEmpty(estimate.ClaimNumber))
                        {
                            model.DefaultSubject = estimate.ClaimNumber + " - " + customer.Contact.FirstName + " " + customer.Contact.LastName;
                        }
                        else
                        {
                            Vehicle vehicleInfo = Vehicle.GetByEstimate(estimateID);
                            model.DefaultSubject = customer.Contact.FirstName + " " + customer.Contact.LastName;  // TODO - vehicle name
                        }

                        // Fill the list of available email addresses to add
                        FillEmailAndPhonesForContact(model, customer.Contact, "Customer");

                        FillEmailAndPhones(estimateID, model, ContactSubType.Claimant, "Claimant");
                        FillEmailAndPhones(estimateID, model, ContactSubType.Adjuster, "Adjuster");
                        FillEmailAndPhones(estimateID, model, ContactSubType.InsuranceAgent, "Insurance Agent");
                        FillEmailAndPhones(estimateID, model, ContactSubType.ClaimRepresentative, "Claim Representative");

                        if (contact != null)
                        {
                            FillEmailAndPhonesForContact(model, contact, "Shop");
                        }


                        model.HasEMSContract = ActiveLogin.HasEMSContract;
                        if (!ActiveLogin.HasEMSContract)
                        {
                            // There might be created but un paid for EMS contract
                            Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
                            List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                            ContractAddOn emsAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 5);
                            if (emsAddOn != null)
                            {
                                if (!emsAddOn.Active)
                                {
                                    model.HasEMSContract = true;    // Still go to the EMS page where we will show a message about the contract being deactivated
                                }
                                else
                                {
                                    model.EmsRedirectPage = "customer-invoice";
                                }
                            }
                            else
                            {
                                model.EmsRedirectPage = "pick-addon/" + activeContract.ID;
                            }
                        }

                        FillEmailTemplates(model);
                    }
                    catch (Exception ex)
                    {
                        ErrorLogger.LogError(ex, ActiveLogin.LoginID, "SendEstimate IndexLoad");
                    }
                }
                else
                {
                    return RedirectToAction("OpenEstimate", "Home");
                }
            }
            else
            {
                LoginInfo loginInfo = LoginInfo.GetByID(ActiveLogin.LoginID);
                model.AccountIsLocked = loginInfo.ProfileLocked;
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "sendestimate";

            ViewBag.EstimateID = estimateID;

            // Determine if the Financing functionality should be included
            if (ViewBag.ShowFinancing == true)
            {
                var merchantInfo = FinancingService.Instance.GetWisetackMerchantData(ActiveLogin.LoginID);
                model.IsFinancingMerchantApproved = !string.IsNullOrEmpty(merchantInfo?.MerchantID) && merchantInfo?.Status == "APPLICATION_APPROVED";
            }

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/send-estimate")]
        public ActionResult Index(SendEstimateVM vm)
        {
            var redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            return View(vm);
        }

        private void FillEmailAndPhones(int estimateID, SendEstimateVM model, ContactSubType contactSubType, string contactLabel)
        {
            ProEstimatorData.DataModel.Contact contact = ProEstimatorData.DataModel.Contact.GetContact(estimateID, contactSubType);
            if (contact != null)
            {
                FillEmailAndPhonesForContact(model, contact, contactLabel);
            }
        }

        private void FillEmailAndPhonesForContact(SendEstimateVM model, ProEstimatorData.DataModel.Contact contact, string contactLabel)
        {
            if (!string.IsNullOrEmpty(contact.Email))
            {
                model.EmailAddresses.Add(new EmailAddressVM(contactLabel, contact.Email));
            }

            if (!string.IsNullOrEmpty(contact.SecondaryEmail))
            {
                model.EmailAddresses.Add(new EmailAddressVM(contactLabel + " Secondary", contact.SecondaryEmail));
            }

            if (!string.IsNullOrEmpty(contact.Phone))
            {
                PhoneType phoneType = PhoneType.GetByCode(contact.PhoneNumberType1);
                model.PhoneNumbers.Add(new PhoneNumberVM(contactLabel + (phoneType == null ? "" : " " + phoneType.ScreenDisplay), InputHelper.FormatPhone(contact.Phone)));
            }

            if (!string.IsNullOrEmpty(contact.Phone2))
            {
                PhoneType phoneType = PhoneType.GetByCode(contact.PhoneNumberType2);
                model.PhoneNumbers.Add(new PhoneNumberVM(contactLabel + (phoneType == null ? "" : " " + phoneType.ScreenDisplay), InputHelper.FormatPhone(contact.Phone2)));
            }

            if (!string.IsNullOrEmpty(contact.Phone))
            {
                PhoneType phoneType = PhoneType.GetByCode(contact.PhoneNumberType3);
                model.PhoneNumbers.Add(new PhoneNumberVM(contactLabel + (phoneType == null ? "" : " " + phoneType.ScreenDisplay), InputHelper.FormatPhone(contact.Phone3)));
            }
        }

        private void FillEmailTemplates(SendEstimateVM model)
        {
            model.BodyTemplates = new List<TemplateVM>();

            var templates = EmailCustomTemplate.GetForLogin(model.LoginID, false);

            foreach (var template in templates)
            {
                model.BodyTemplates.Add(new TemplateVM(template.ID, template.Name, template.Description));
            }

            var defaultBody = templates.FirstOrDefault(o => o.IsDefault == true);
            if (defaultBody != null)
            {
                model.DefaultBodyTemplateID = defaultBody.ID;
            }
        }

        /// <summary>
        /// Returns the list of sent emails for the active estimate, used to bind to the grid
        /// </summary>
        public ActionResult GetSentList([DataSourceRequest] DataSourceRequest request, int estimateID, bool email, bool sms)
        {
            List<SentEstimateSummaryVM> summaryList = new List<SentEstimateSummaryVM>();

            List<ReportEmail> emails = ReportEmail.GetForEstimate(estimateID);
            foreach (ReportEmail emailMessage in emails)
            {
                summaryList.Add(new SentEstimateSummaryVM(emailMessage));
            }

            return Json(summaryList.ToDataSourceResult(request));
        }

        public JsonResult GetSentEmail(int userID, int estimateID, int id)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ReportEmail email = ReportEmail.Get(id);

                if (email != null && email.EstimateID == estimateID)
                {
                    EmailVM vm = new EmailVM();
                    vm.ToAddresses = email.ToAddresses;
                    vm.CCAddresses = email.CCAddresses;
                    vm.PhoneNumbers = email.PhoneNumbers;
                    vm.Subject = email.Subject;
                    vm.Body = email.Body;
                    vm.SentTimeMessage = email.SentStamp.ToShortDateString() + " at " + email.SentStamp.ToShortTimeString();
                    vm.ErrorMessage = email.Errors;

                    List<ProEstimatorData.DataModel.Report> reports = ProEstimatorData.DataModel.Report.GetReportsForEmail(email.ID);
                    foreach (ProEstimatorData.DataModel.Report report in reports)
                    {
                        string reportName = ProEstimatorData.DataModel.Report.GetReportName(estimateID, report.ReportType.Text) + "." + report.GetFileExtension();
                        vm.ReportAttachments.Add(new ReportVM(report.ID, report.ReportType.Tag, reportName));
                    }

                    List<SendGridInfo> sendGridInfo = SendGridInfo.GetByEmailID(email.EmailID);
                    foreach(SendGridInfo info in sendGridInfo)
                    {
                        StatusMessageVM messageVM = new StatusMessageVM();
                        messageVM.Email = info.Email;
                        messageVM.ID = info.ID;
                        messageVM.TimeStamp = info.TimeStamp;
                        messageVM.Event = info.Event;
                        messageVM.Reason = info.Reason;
                        messageVM.EmailID = info.EmailID;

                        vm.StatusMessage.Add(messageVM);
                    }
                    
                    return Json(vm, JsonRequestBehavior.AllowGet);
                }
            }

            return Json("", JsonRequestBehavior.AllowGet);
        }

        public async Task<JsonResult> CreateReportAttachment(int userID, int estimateID, string reportType, string language = "en", string extraParam = "", string version = "", string customReportType = null)
        {
            CacheActiveLoginID(userID);
            string activeLoginID = Convert.ToString(HttpContext.Items["ActiveLoginID"]);

            CreateReportJson reportJson = new CreateReportJson();

            try
            {
                Estimate estimate = new Estimate(estimateID);
                if (IsUserAuthorized(userID))
                {
                    ReportFunctionResult result;

                    if (reportType == "CustomReport")
                    {
                        int templateID = InputHelper.GetInteger(customReportType);

                        string[] extraParams = extraParam.Split(":".ToCharArray());
                        bool includeImages = InputHelper.GetBoolean(extraParams[4]);

                        ReportFunctionResult estimateImagesReportResult = null;
                        Report estimateImagesReport = null;
                        string[] pdfDocumentsToMerge = null;
                        if (includeImages)
                        {
                            pdfDocumentsToMerge = new string[2];

                            ReportGenerator reportGenerator = new ReportGenerator();
                            estimateImagesReportResult = await reportGenerator.GenerateReport(GetActiveLoginID(userID), estimate, "Estimate_Images", language, extraParam);
                            estimateImagesReport = estimateImagesReportResult.Report;

                            pdfDocumentsToMerge[1] = estimateImagesReportResult.Report.GetDiskPath();
                        }

                        CustomReportGenerator generator = new CustomReportGenerator();
                        result = generator.RenderCustomReport(templateID, estimateID, estimate.CreatedByLoginID, estimateImagesReport);
                        string mergedFileName = ProEstimatorData.DataModel.Report.GetReportName(estimateID, reportType) + ".pdf";

                        if(includeImages)
                        {
                            pdfDocumentsToMerge[0] = result.Report.GetDiskPath();

                            // Get template and estimate and validate
                            CustomReportTemplate template = CustomReportTemplate.Get(templateID);
                            result.Report.FileName = Path.GetFileNameWithoutExtension(result.Report.GetDiskPath()) + "_" + Guid.NewGuid().ToString();
                            SaveResult reportRecordSave = result.Report.Save();

                            string mergedFileNameWithDiscPath = result.Report.GetDiskPath();
                            generator.MergePDFfiles(pdfDocumentsToMerge, mergedFileNameWithDiscPath);
                        }

                        result.ReportFullName = mergedFileName;
                    }
                    else if (reportType == "EMS")
                    {
                        EMSExportManager exportManager = new EMSExportManager();
                        result = exportManager.ExportEMS(estimateID, extraParam, version);
                        result.ReportFullName = ProEstimatorData.DataModel.Report.GetReportName(estimateID, reportType) + ".zip";
                    }
                    else
                    {
                        ReportGenerator generator = new ReportGenerator();
                        result = await generator.GenerateReport(GetActiveLoginID(userID), estimate, reportType, language, extraParam);
                        result.ReportFullName = ProEstimatorData.DataModel.Report.GetReportName(estimateID, reportType) + ".pdf";

                        // If we are adding a PartsOrder report, we want to return all of the vendor email addresses to the email popup list on the send page.
                        if (reportType == "PartsOrder")
                        {
                            string[] extraParams = extraParam.Split(":".ToCharArray());

                            List<SqlParameter> parameters = new List<SqlParameter>();
                            parameters.Add(new SqlParameter("AdminInfoID", estimateID));
                            parameters.Add(new SqlParameter("PartSourceFilter", extraParams[0]));
                            parameters.Add(new SqlParameter("SupplementVersion", InputHelper.GetInteger(extraParams[1])));

                            DBAccess db = new DBAccess();
                            DBAccessTableResult vendors = db.ExecuteWithTable("Report_GetPartOrdersVendors", parameters);
                            foreach (DataRow row in vendors.DataTable.Rows)
                            {
                                string vendorName = InputHelper.GetString(row["CompanyName"].ToString());
                                string emailAddress = InputHelper.GetString(row["Email"].ToString());

                                if (!string.IsNullOrEmpty(vendorName) && !string.IsNullOrEmpty(emailAddress))
                                {
                                    reportJson.NewEmailAddresses.Add(new EmailAddressVM("Vendor: " + vendorName, emailAddress));
                                }
                            }
                        }
                    }

                    if (result != null)
                    {
                        if (result.Success)
                        {
                            reportJson.Report = new ReportVM(result.Report.ID, result.Report.ReportType == null ? "Custom" : result.Report.ReportType.Tag, result.ReportFullName);
                            reportJson.ErrorMessage = result.ErrorMessage;      // There could be extra message even if the report was successful
                        }
                        else
                        {
                            reportJson.Success = false;
                            reportJson.ErrorMessage = result.ErrorMessage;
                        }
                    }
                    else
                    {
                        reportJson.Success = false;
                        reportJson.ErrorMessage = @Proestimator.Resources.ProStrings.NoResultFromReportGenerator;
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, InputHelper.GetInteger(activeLoginID), "CreateReportAttachment GenerateReport");
            }

            return Json(reportJson, JsonRequestBehavior.AllowGet);
        }

        public JsonResult CreatePreviewReport(int userID, string editorText, int templateID)
        {
            CacheActiveLoginID(userID);

            string editorProcessedText = "";

            if (IsUserAuthorized(userID))
            {
                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                CustomReportGenerator generator = new CustomReportGenerator();
                editorProcessedText = generator.RenderPreviewCustomReport(templateID, 1, editorText, activeLogin.LoginID);
            }

            var reportJson = new { Success = true, EditorProcessedText = editorProcessedText };
            return Json(reportJson, JsonRequestBehavior.AllowGet);
        }

        public class CreateReportJson
        {
            public bool Success { get; set; }
            public string ErrorMessage { get; set; }
            public ReportVM Report { get; set; }
            public List<EmailAddressVM> NewEmailAddresses { get; set; }

            public CreateReportJson()
            {
                Success = true;
                ErrorMessage = "";
                NewEmailAddresses = new List<EmailAddressVM>();
            }
        }

        public JsonResult SendEmail(int userID, int loginID, int estimateID, string toAddresses, string ccAddresses, string phoneNumbers, string subject, string body, List<int> reportIDs)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StringBuilder errorBuilder = new StringBuilder();

                // Turn the e-mail address inputs into lists of addresses and validate them
                List<string> toList = GetEmailAddressList(toAddresses);
                List<string> ccList = GetEmailAddressList(ccAddresses);
                List<string> phoneList = GetEmailAddressList(phoneNumbers);

                // Do validation
                ValidateEmailAddresses(toList, errorBuilder);
                ValidateEmailAddresses(ccList, errorBuilder);
                ValidateBounceEmailAddresses(toList, errorBuilder);
                ValidateBounceEmailAddresses(ccList, errorBuilder);
                ValidateUnsubscribeEmailAddresses(toList, errorBuilder);
                ValidateUnsubscribeEmailAddresses(ccList, errorBuilder);
                ValidatePhoneNumbers(phoneList, errorBuilder);

                if (toList.Count == 0 && phoneList.Count == 0)
                {
                    errorBuilder.AppendLine("Please enter at least one e-mail address or phone number to send to.");
                }

                if (errorBuilder.Length > 0)
                {
                    return Json(errorBuilder.ToString(), JsonRequestBehavior.AllowGet);
                }

                // Create the database records
                ReportEmail email = new ReportEmail();
                email.SentStamp = DateTime.Now;
                email.Subject = subject;
                email.Body = body;
                email.ToAddresses = toAddresses;
                email.CCAddresses = ccAddresses;
                email.PhoneNumbers = phoneNumbers;
                email.EstimateID = estimateID;
                SaveResult emailSaveResult = email.Save();

                string uniqueID = GenerateUniqueID();

                if (emailSaveResult.Success)
                {
                    // Get a list of disk paths to the files to attach from the passed report IDs
                    List<string> attachmentPaths = new List<string>();
                    if (reportIDs != null)
                    {
                        foreach (int reportID in reportIDs)
                        {
                            ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(reportID);
                            if (report != null)
                            {
                                attachmentPaths.Add(report.GetDiskPath());
                                email.InsertReportAttachment(reportID);
                            }
                        }
                    }

                    List<string> attachmentPathsColl = ImageAttachmentHelper.GetAttachmentPaths(loginID, reportIDs);

                    foreach (string eachAttachmentPaths in attachmentPathsColl)
                    {
                        attachmentPaths.Add(eachAttachmentPaths);
                    }

                    // Send the text message
                    if (phoneList.Count > 0)
                    {
                        MessageAttachmentMapping.Save(email.ID, uniqueID);

                        FunctionResult sendSMSresult = TextMessageService.SendReport(phoneList, body, email.ID, uniqueID);
                        if (!sendSMSresult.Success)
                        {
                            errorBuilder.AppendLine(sendSMSresult.ErrorMessage);
                        }
                    }

                    // Send the email
                    if (toList.Count > 0 || ccList.Count > 0 || phoneList.Count > 0)
                    {
                        string shopAddress = "";

                        // Get the shop email address to use as the reply to address.
                        ProEstimatorData.DataModel.Contact contact = ProEstimatorData.DataModel.Contact.GetContactForLogins(loginID);
                        if (contact != null && (!string.IsNullOrEmpty(contact.Email) || !string.IsNullOrEmpty(contact.Phone)))
                        {
                            shopAddress = contact.Email;
                        }

                        AccountPreferencesVM model = new AccountPreferencesVM();
                        Boolean isEmailAutoCcOn = InputHelper.GetBoolean(SiteSettings.Get(loginID, "AutoCcOn", "EmailPreferences", "1").ValueString);

                        if (isEmailAutoCcOn)
                        {
                            toList.Add(shopAddress);
                        }

                        Email emailMessage = new Email();
                        emailMessage.LoginID = loginID;
                        toList.ForEach(o => emailMessage.AddToAddress(o));
                        ccList.ForEach(o => emailMessage.AddCCAddress(o));
                        emailMessage.Subject = subject;
                        emailMessage.Body = body;
                        attachmentPaths.ForEach(o => emailMessage.AddAttachmentPath(o));
                        emailMessage.ReplyTo = shopAddress;
                        emailMessage.Save(0);
                        EmailSender.SendEmail(emailMessage);
                        email.EmailID = emailMessage.ID;

                        SuccessBoxFeatureLog.LogFeature(loginID, SuccessBoxModule.EstimateWriting, "User sent an estimate", GetActiveLoginID(userID));
                    }

                    email.Errors = errorBuilder.ToString();
                    if (!string.IsNullOrEmpty(email.Errors) || email.EmailID > 0)
                    {
                        email.Save();
                    }
                }
                else
                {
                    errorBuilder.AppendLine("Error saving email database record: " + emailSaveResult.ErrorMessage);
                }

                string returnString = errorBuilder.ToString();
                if (string.IsNullOrEmpty(returnString))
                {
                    returnString = "success";
                }


                return Json(returnString, JsonRequestBehavior.AllowGet);
            }

            return Json("User not authorized.", JsonRequestBehavior.AllowGet);
        }

        private string GenerateUniqueID()
        {
            return Guid.NewGuid().ToString().Replace("-", "").Substring(0, 10);
        }

        private List<string> GetEmailAddressList(string input)
        {
            List<string> result = new List<string>();

            if (!string.IsNullOrEmpty(input))
            {
                input = input.Replace(",", ";");

                string[] pieces = input.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

                foreach (string piece in pieces)
                {
                    result.Add(piece.Trim());
                }
            }

            return result;
        }

        /// <summary>
        /// If any email addresses are invalid, return a string with the error message.  Returns an empty string if the addresses are OK
        /// </summary>
        private void ValidateEmailAddresses(List<string> emailAddresses, StringBuilder errorBuilder)
        {
            foreach (string emailAddress in emailAddresses)
            {
                if (!InputHelper.IsValidEmail(emailAddress))
                {
                    errorBuilder.AppendLine(emailAddress + " is not a valid e-mail address<br>");
                }
            }
        }

        private void ValidateBounceEmailAddresses(List<string> emailAddresses, StringBuilder errorBuilder)
        {
            foreach (string emailAddress in emailAddresses)
            {
                if (SendGridInfo.GetBounce(emailAddress))
                {
                    errorBuilder.AppendLine(emailAddress + " is a bounce e-mail address<br>");
                }
            }
        }

        private void ValidateUnsubscribeEmailAddresses(List<string> emailAddresses, StringBuilder errorBuilder)
        {
            foreach (string emailAddress in emailAddresses)
            {
                if (Unsubscribe.GetUnsubscribe(emailAddress))
                {
                    errorBuilder.AppendLine(emailAddress + " is a unsubscribe e-mail address<br>");
                }
            }
        }

        private void ValidatePhoneNumbers(List<string> phoneNumbers, StringBuilder errorBuilder)
        {
            foreach (string phoneNumber in phoneNumbers)
            {
                if (!InputHelper.IsValidPhoneNumber(phoneNumber))
                {
                    errorBuilder.AppendLine(phoneNumber + " is not a valid phone number");
                }
            }
        }

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/view-attachment/{attachment}/{filename}")]
        public ActionResult ViewAttachment(int userID, int estimateID, string attachment, string filename = "")
        {
            Estimate estimate = new Estimate(estimateID);
            if (estimate != null && estimate.CreatedByLoginID == ActiveLogin.LoginID)
            {
                int idInt = InputHelper.GetInteger(attachment);
                if (idInt > 0)
                {
                    ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(idInt);

                    if (report != null && report.ReportBelongsToLogin(ActiveLogin.LoginID))
                    {
                        string diskPath = report.GetDiskPath();
                        if (!string.IsNullOrEmpty(diskPath) && System.IO.File.Exists(diskPath))
                        {
                            var fileStream = new FileStream(diskPath, FileMode.Open, FileAccess.Read);
                            if (report.GetFileExtension() == "pdf")
                            {
                                PDFDownloadSetting download = (PDFDownloadSetting)InputHelper.GetInteger(SiteSettings.Get(ActiveLogin.LoginID, "DownloadPDF", "ReportOptions", ((int)PDFDownloadSetting.OpenNewTab).ToString()).ValueString);

                                if (download == PDFDownloadSetting.Download)
                                {
                                    Response.Headers.Add("Content-Disposition", "attachment");
                                }

                                //return new FileStreamResult(fileStream, "application/pdf") { FileDownloadName = report.FileName + "." + report.GetFileExtension() };
                                return new FileStreamResult(fileStream, "application/pdf");
                                //return File(fileStream, "application/pdf", report.FileName);
                            }
                            else
                            {
                                return File(diskPath, report.GetContentType(), report.FileName + "." + report.GetFileExtension());
                            }
                        }
                    }
                }
            }

            //filename from querystring or estimate id from session is not valid...show the error message instead of blank screen.
            return Content("Error: report not found.");
        }

        public JsonResult CheckEmails(int userID, string toAddresses)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                StringBuilder errorBuilder = new StringBuilder();

                // Turn the e-mail address inputs into lists of addresses and validate them
                List<string> toList = GetEmailAddressList(toAddresses);

                // Do validation
                ValidateEmailAddresses(toList, errorBuilder);
                ValidateBounceEmailAddresses(toList, errorBuilder);
                ValidateUnsubscribeEmailAddresses(toList, errorBuilder);

                return Json(errorBuilder.ToString(), JsonRequestBehavior.AllowGet);

            }
            return Json("", JsonRequestBehavior.AllowGet);
        }

        public JsonResult DeleteAttachment(int userID, int reportID)
        {
            CacheActiveLoginID(userID);

            if (IsUserAuthorized(userID))
            {
                ProEstimatorData.DataModel.Report report = ProEstimatorData.DataModel.Report.Get(reportID);

                ActiveLogin activeLogin = _siteLoginManager.GetActiveLogin(userID, GetComputerKey());

                if (report != null && report.ReportBelongsToLogin(activeLogin.LoginID))
                {
                    report.Delete();

                    // Delete Email's image attachment
                    ImageAttachment imageAttachment = new ImageAttachment(reportID);
                    imageAttachment.DeleteForReport();

                    return Json("", JsonRequestBehavior.AllowGet);
                }
            }

            return Json(@Proestimator.Resources.ProStrings.ErrorReportNotFoundToDelete, JsonRequestBehavior.AllowGet);
        }

        #endregion


        #region EMS Export

        [HttpGet]
        [Route("{userID}/estimate/{estimateID}/ems-export")]
        public ActionResult EMS(int userID, int estimateID)
        {
            EMSExportPage model = new EMSExportPage();

            // If the active session has no frame data contract, there might actually be a contract but it is deactivated.  
            if (!ActiveLogin.HasEMSContract)
            {
                Contract activeContract = Contract.GetActive(ActiveLogin.LoginID);
                List<ContractAddOn> addOns = ContractAddOn.GetForContract(activeContract.ID);
                ContractAddOn emsAddOn = addOns.FirstOrDefault(o => o.AddOnType.ID == 5);
                if (emsAddOn != null)
                {
                    if (!emsAddOn.Active)
                    {
                        model.ContractDeactivated = true;
                    }
                    else
                    {
                        return Redirect("/" + userID + "/invoice/customer-invoice");
                    }
                }
                else
                {
                    return Redirect("/" + userID + "/invoice/pick-addon/" + activeContract.ID);
                }
            }

            model.LoginID = ActiveLogin.LoginID;
            model.EstimateID = estimateID;

            ViewBag.EstimateID = estimateID;
            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "sendestimate";

            return View(model);
        }

        [HttpPost]
        [Route("{userID}/estimate/{estimateID}/ems-export")]
        public ActionResult EMS(EMSExportPage vm)
        {
            var redirectResult = DoRedirect("");
            if (redirectResult != null)
            {
                return redirectResult;
            }

            ViewBag.NavID = "estimate";
            ViewBag.EstimateNavID = "sendestimate";

            return View(vm);
        }

        /// <summary>
        /// Returns the list of EMS exports, used to bind to the grid
        /// </summary>
        public ActionResult GetEMSList([DataSourceRequest] DataSourceRequest request, int estimateID)
        {
            List<SentEmsVM> summaryList = new List<SentEmsVM>();

            // Add EMS exports into the list
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Function", "GetExportListEMS2.6"));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("App_ImportExport", parameters);

            if (result.Success)
            {
                string baseUrl = ConfigurationManager.AppSettings["BaseURL"];

                foreach (DataRow row in result.DataTable.Rows)
                {
                    SentEmsVM emsVM = new SentEmsVM();
                    emsVM.MessageQueueID = InputHelper.GetInteger(row[0].ToString());
                    emsVM.Status = row["Status"].ToString();
                    emsVM.TimeStamp = InputHelper.GetDateTime(row["CreationDateTime"].ToString());

                    summaryList.Add(emsVM);
                }

                summaryList = summaryList.OrderByDescending(o => o.TimeStamp).ToList();
            }

            return Json(summaryList.ToDataSourceResult(request));
        }

        public JsonResult SendEMS(int loginID, int estimateID)
        {
            SentEmsVM result = new SentEmsVM();

            //try
            //{
            //    EMSExportManager exportManager = new EMSExportManager();
            //    EMSExportResult emsExportResult = exportManager.ExportEMS(estimateID, loginID);

            //    result.Success = emsExportResult.Success;
            //    result.ErrorMessage = string.IsNullOrEmpty(emsExportResult.ErrorMessage) ? Resources.ProStrings.SendEMS_Success : emsExportResult.ErrorMessage;

            //    result.MessageQueueID = emsExportResult.MessageQueueID;
            //    result.Status = "Queued";
            //    result.TimeStamp = DateTime.Now;
            //}
            //catch (System.Exception ex)
            //{
            //    ErrorLogger.LogError(ex, loginID, "SendEMS");
            //    result.Success = false;
            //    result.ErrorMessage = ex.Message;
            //}

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        /// Called on a timer after an ems record is queued until the record is complete or has an error.  Returns the current status.
        /// </summary>
        public JsonResult GetEmsUpdates(int emsID)
        {
            EMSCheckAjaxResult result = new EMSCheckAjaxResult();

            result.IsCheckDone = false;

            try
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult tableResult = db.ExecuteWithTable("GetMessageQueueItemStatus", new SqlParameter("id", emsID));
                if (tableResult.Success)
                {
                    int statusNumber = InputHelper.GetInteger(tableResult.DataTable.Rows[0]["msgStatus"].ToString());
                    result.CurrentStatus = tableResult.DataTable.Rows[0]["msgStatusLong"].ToString();

                    result.Success = true;

                    if (statusNumber > 2)
                    {
                        result.IsCheckDone = true;
                    }
                }
                else
                {
                    result.ErrorMessage = @Proestimator.Resources.ProStrings.EMSQueueRecordNotFound;
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "GetEMSUpdates");
                result.Success = false;
                result.ErrorMessage = ex.Message;
            }

            return Json(result, JsonRequestBehavior.AllowGet);
        }

        public class EMSCheckAjaxResult : AjaxResult
        {
            public bool IsCheckDone { get; set; }
            public string CurrentStatus { get; set; }
        }

        public ActionResult EmsDownload(int loginID, int estimateID, int emsID)
        {
            if (emsID > 0)
            {
                ProEstimatorData.DataModel.Estimate estimate = new Estimate(estimateID);
                if (estimate == null || estimate.CreatedByLoginID != loginID)
                {
                    return Content(@Proestimator.Resources.ProStrings.InvalidEstimateID);
                }

                // Get the path to the zip file.
                // NOTE: on 7/10/2018 the EMS creation process was redone.  When we moved the database to a new sql server the old SSIS process that did the update didn't work anymore.
                // Before the EMS files were all saved in an EMS folder in the root of the user content, now they are saved in the Reports folder along with the PDFs.
                string filePath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), estimate.CreatedByLoginID.ToString(), estimate.EstimateID.ToString(), "Reports", emsID.ToString() + ".zip");
                if (!System.IO.File.Exists(filePath))
                {
                    filePath = Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath"), "EMS", estimate.EstimateID.ToString() + "_" + emsID.ToString() + ".zip").Replace("//", "/");
                }

                if (System.IO.File.Exists(filePath))
                {
                    var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
                    //var fsResult = new FileStreamResult(fileStream, "application/zip");
                    //return fsResult;
                    return File(fileStream, "application/zip", "EMS_" + emsID.ToString() + ".zip");
                }
                else
                {
                    return Content(@Proestimator.Resources.ProStrings.ZipFileNotFound);
                }
            }

            return Content(@Proestimator.Resources.ProStrings.CouldNotFindEMSDownloadTryAgain);
        }

        #endregion
    }
}