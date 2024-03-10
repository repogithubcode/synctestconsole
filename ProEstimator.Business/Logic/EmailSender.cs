using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

using SendGrid;
using SendGrid.Helpers.Mail;
using Newtonsoft.Json;

using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;
using ProEstimator.Business.Model;
using ProEstimator.Business.Type;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimator.Business.Resources;
using DocumentFormat.OpenXml.ExtendedProperties;

namespace ProEstimator.Business.Logic
{
    public class EmailSender
    {       
        private static bool _fromNonProd;
        private static string _apiKey;

        static EmailSender()
        {
            DBAccess db = new DBAccess();
            _fromNonProd = db.FromNonProd();
            _apiKey = ConfigurationManager.AppSettings["SendGridAPI"].ToString();
        }

        public static void SendEmail(Email email)
        {
            FunctionResult sendResult = SendEmailAsync(email.ToAddresses, email.CCAddresses, email.Subject, email.Body, email.AttachmentPaths,
                email.ID.ToString(), email.LoginID.ToString(), email.ReplyTo, email.TemplateID, email.CompanyName);
            
            if (!sendResult.Success)
            {
                email.ErrorMessage = sendResult.ErrorMessage;
                email.HasError = true;
                email.Save(0);

                EmailSendFailure.Insert(email.ID, sendResult.ErrorMessage);
            }
        }

        private static FunctionResult SendEmailAsync(List<string> emailAddresses, List<string> ccEmailAddresses, string emailSubject, string emailMessage, 
            List<string> attachmentPaths, string emailID, string loginID, string replyToAddress, int templateID, string companyName)
        {
            try
            {
                SendGridMessage message = new SendGridMessage();

                List<string> addedAddresses = new List<string>();

                if (emailAddresses != null)
                {
                    foreach (string emailAddress in emailAddresses)
                    {
                        string trimmed = emailAddress.Trim().ToLower();

                        if (!addedAddresses.Contains(trimmed))
                        {
                            message.AddTo(trimmed);
                            addedAddresses.Add(trimmed);
                        }
                    }
                }

                if (ccEmailAddresses != null)
                {
                    foreach (string emailAddress in ccEmailAddresses)
                    {
                        string trimmed = emailAddress.Trim().ToLower();

                        if (!addedAddresses.Contains(trimmed))
                        {
                            message.AddCc(trimmed);
                            addedAddresses.Add(trimmed);
                        }
                    }
                }

                if(_fromNonProd)
                {
                    emailSubject = "TEST - " + emailSubject;
                }
                message.SetSubject(emailSubject);
                message.AddCustomArg("we_email_id", emailID);
                message.AddCustomArg("we_login_id", loginID);

                if (string.IsNullOrEmpty(emailMessage))
                {
                    emailMessage = " ";
                }
                if (_fromNonProd)
                {
                    emailMessage = "This is a test email and no action is required<br /><br />" + emailMessage;
                }

                // Append the footer to this email if there is a company name (i.e. if there is an estimate associated with the email)
                if (!string.IsNullOrEmpty(companyName)) 
                {
                    emailMessage = $"{emailMessage}<p></p><p></p><p></p><p></p><p>{ProEstBusiness.EmailFooterLine1.Replace("[[COMPANY_NAME]]", companyName)}</p><p>{ProEstBusiness.EmailFooterLine2}</p>";
                }

                message.PlainTextContent = HtmlUtilities.ConvertToPlainText(emailMessage);
                message.HtmlContent = emailMessage;
                message.From = IsPrivateEmail(templateID, emailSubject) ? PrivateEmailAddress : PublicEmailAddress;
                message.SetClickTracking(false, false);

      
                if (attachmentPaths != null)
                {
                    foreach (string attachmentPath in attachmentPaths)
                    {
                        try
                        {
                            if (File.Exists(attachmentPath))
                            {
                                FileInfo fileInfo = new FileInfo(attachmentPath);

                                Byte[] bytes = File.ReadAllBytes(attachmentPath);
                                String attachmentBase64 = Convert.ToBase64String(bytes);

                                SendGrid.Helpers.Mail.Attachment attachment = new SendGrid.Helpers.Mail.Attachment();
                                attachment.Content = attachmentBase64;
                                attachment.Filename = fileInfo.Name;

                                message.AddAttachment(attachment);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorLogger.LogError(ex, 0, "SendEmail AddAttachment Error");
                        }
                    }
                }

                if (!string.IsNullOrEmpty(replyToAddress))
                {
                    replyToAddress = replyToAddress.Trim();
                    message.SetReplyTo(new EmailAddress(replyToAddress));

                    if (replyToAddress.ToLower().EndsWith("web-est.com"))
                    {
                        message.From = new EmailAddress(replyToAddress);
                    }
                }

                try
                {
                    var client = new SendGridClient(_apiKey);
                    var response = Task.Run(() => client.SendEmailAsync(message));

                    if(response == null)
                    {
                        return new FunctionResult("Null SendEmail Response");
                    }

                    if (response.Result.IsSuccessStatusCode)
                    {
                        return new FunctionResult();
                    }
                    else
                    {
                        return new FunctionResult(response.Result.StatusCode.ToString());
                    }
                }
                catch (Exception ex)
                {
                    ErrorLogger.LogError(ex, int.Parse(loginID), int.Parse(emailID), "SendGrid Error");
                    return new FunctionResult(ex.Message + (ex.InnerException != null ? " " + ex.InnerException.Message : ""));
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "SendEmail Error");
                return new FunctionResult(ex.Message);
            }
        }

        private static SendGrid.Helpers.Mail.EmailAddress PublicEmailAddress
        {
            get
            {
                if (_publicEmailAddress == null)
                {
                    string emailAddress = "estimates@web-est.com";

                    try
                    {
                        string settingValue = ConfigurationManager.AppSettings["emailFromAddressPublic"];
                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            emailAddress = settingValue;
                        }
                    }
                    catch (Exception ex)
                    { }

                    _publicEmailAddress = new SendGrid.Helpers.Mail.EmailAddress(emailAddress);
                }

                return _publicEmailAddress;
            }
        }
        private static SendGrid.Helpers.Mail.EmailAddress _publicEmailAddress;

        private static SendGrid.Helpers.Mail.EmailAddress PrivateEmailAddress
        {
            get
            {
                if (_privateEmailAddress == null)
                {
                    string emailAddress = "support@web-est.com";

                    try
                    {
                        string settingValue = ConfigurationManager.AppSettings["emailFromAddressPrivate"];
                        if (!string.IsNullOrEmpty(settingValue))
                        {
                            emailAddress = settingValue;
                        }
                    }
                    catch (Exception ex)
                    { }

                    _privateEmailAddress = new SendGrid.Helpers.Mail.EmailAddress(emailAddress);
                }

                return _privateEmailAddress;
            }
        }
        private static SendGrid.Helpers.Mail.EmailAddress _privateEmailAddress;

        private static bool IsPrivateEmail(int templateID, string subject)
        {
            if (templateID > 0)
            {
                return true;
            }

            if (subject.StartsWith("New Contract #") || subject.StartsWith("ProEstimator Signed Agreement"))
            {
                return true;
            }

            return false;
        }
    }

    public class EmailProcess
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("timestamp")]
        public double TimeStamp { get; set; }
        [JsonProperty("smtp-id")]
        public string SmtpId { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
        [JsonProperty("reason")]
        public string Reason { get; set; }
        [JsonProperty("we_email_id")]
        public string EmailID { get; set; }
        public DateTime Time
        {
            get
            {
                return InputHelper.UnixTimeStampToDateTime(TimeStamp);
            }
        }
    }

    public class UnsubscribeData
    {
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("timestamp")]
        public double TimeStamp { get; set; }
        [JsonProperty("smtp-id")]
        public string SmtpId { get; set; }
        [JsonProperty("event")]
        public string Event { get; set; }
        [JsonProperty("we_login_id")]
        public string LoginID { get; set; }
        [JsonProperty("we_email_id")]
        public string EmailID { get; set; }
        public DateTime Time
        {
            get
            {
                return InputHelper.UnixTimeStampToDateTime(TimeStamp);
            }
        }
    }
}
