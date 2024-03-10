using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Logic
{
    /// <summary>
    /// Provides methods for sending SMS with/without PDF attached to the text message body
    /// </summary>
    public class TextMessageService
    {
        /// <summary>AccountSID is Account SID of Twilio</summary>
        private static string AccountSID;

        /// <summary>AuthToken is Auth Token of Twilio</summary>
        private static string AuthToken;

        /// <summary>Phone number from which SMS sending</summary>
        private static string From;

        /// <summary>Phone number to which SMS sending</summary>
        private string To;

        /// <summary>Text message in SMS</summary>
        private string Body;

        //List of Media urls - publicly accessible urls
        private List<Uri> MediaUrls;
        private static EstimateService _estimateService;

        /// <summary>
        /// static values from Web.config
        /// </summary>
        static TextMessageService()
        {
            AccountSID = System.Configuration.ConfigurationManager.AppSettings["TwilioAccountSID"];
            AuthToken = System.Configuration.ConfigurationManager.AppSettings["TwilioAuthToken"];
            From = System.Configuration.ConfigurationManager.AppSettings["TwilioFromPhoneNo"];
            _estimateService = new EstimateService(null);
        }

        private static SendSMSResponse SendSMS(string toNumber, string body, string reportFilePath)
        {
            string errorMessage = "";

            try
            {
                TwilioClient.Init(AccountSID, AuthToken);

                MessageResource resource = MessageResource.Create(
                    from: new PhoneNumber(From),
                    to: new PhoneNumber(toNumber),
                    body: body,
                    mediaUrl: string.IsNullOrEmpty(reportFilePath) ? null : new List<Uri>() { new Uri(reportFilePath) }
                );

                if (resource != null)
                {
                    return new SendSMSResponse(true, "", resource.Status.ToString(), resource.Sid, resource);
                }
            }
            catch (System.Exception ex)
            {
                errorMessage = ex.Message;
            }

            return new SendSMSResponse(false, errorMessage, "", "", null);
        }


        public static FunctionResult SendReport(List<string> phoneNumbers, string smsBody, int reportID, string uniqueID)
        {
            FunctionResult result = new FunctionResult();
            result.Success = false;

            // Ensure the smsBody is in Plain Text (convert from HTML). Note the follow should have no impact if already in plain text.
            var plainTextBody = HtmlUtilities.ConvertToPlainText(smsBody);

            string viewAttachmentURL = ConfigurationManager.AppSettings["BaseURL"] + "SMS/" + uniqueID;
            smsBody = $"Follow this link for details on your auto estimate.{Environment.NewLine}{Environment.NewLine}{viewAttachmentURL}{Environment.NewLine}{Environment.NewLine}{plainTextBody}";

            // Send the report image to all phone numbers
            foreach (string phoneNo in phoneNumbers)
            {
                string toNumber = phoneNo.Replace("-", "").Replace("(", "").Replace(")", "").Replace(" ", "");

                SendSMSResponse response = SendSMS(toNumber, smsBody, null);
                if (response.Success)
                {
                    SentSmsStaus smsStatus = new SentSmsStaus();
                    smsStatus.ReportId = reportID;
                    smsStatus.ErrorMessage = response.ErrorMessage;
                    smsStatus.ResponseResource = Newtonsoft.Json.JsonConvert.SerializeObject(response.MessageResource);
                    smsStatus.Status = response.Status;
                    smsStatus.SmsId = response.MessageID;
                    smsStatus.Save();

                    result.Success = true;
                }
                else
                {
                    result.Success = false;
                    result.ErrorMessage += response.ErrorMessage + Environment.NewLine;
                }
            }
           
            return result;
        }

        public static MessageResource FetchSMS(string sId)
        {
            TwilioClient.Init(AccountSID, AuthToken);
            var messageResponse = MessageResource.Fetch(sId);
            return messageResponse;
            //  return Newtonsoft.Json.JsonConvert.SerializeObject(messageResponse);
        }

        public static void UpdateAllQueuedSMSStatus(StringBuilder builder)
        {
            try
            {
                var PendingSmsList = SentSmsStaus.GetAllSMS();

                if (PendingSmsList.Count > 0)
                {
                    builder.AppendLine(DateTime.Now.ToString() + "\tTextMessageService UpdateAllQueuedSMSStatus has " + PendingSmsList.Count.ToString() + " pending SMS messages to check.");

                    foreach (var sms in PendingSmsList)
                    {
                        try
                        {
                            var response = FetchSMS(sms.SmsId);// response from Twilio
                            sms.Status = response.Status.ToString();
                            sms.ResponseResource = Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            sms.ErrorMessage = response.ErrorMessage;
                        }
                        catch (Exception ex)
                        {
                            sms.Status = "ProcessError";
                            sms.ErrorMessage = ex.Message;

                            builder.AppendLine(DateTime.Now.ToString() + "\tTextMessageService UpdateAllQueuedSMSStatus Error processing SentSmsStatus ID " + sms.ID.ToString() + ": " + ex.Message);
                        }

                        sms.Save();
                    }
                }
            }
            catch (Exception ex)
            {
                ProEstimatorData.ErrorLogger.LogError(ex, 0, "TextMessageService UpdateAllQueuedSMSStatus");

                builder.AppendLine(DateTime.Now.ToString() + "\tTextMessageService error: " + ex.Message);
                builder.AppendLine("\t" + ex.StackTrace);
            }
        }

    }

    public class SendSMSResponse
    {
        public bool Success { get; private set; }
        public string ErrorMessage { get; private set; }
        public string Status { get; private set; }
        public string MessageID { get; private set; }
        public MessageResource MessageResource { get; private set; }

        public SendSMSResponse(bool success, string errorMessage, string status, string messageID, MessageResource messageResource)
        {
            Success = success;
            ErrorMessage = errorMessage;
            Status = status;
            MessageID = messageID;
            MessageResource = messageResource;
        }
    }

    public class SentEstimateFunctionResult : FunctionResult
    {
        public SentEstimate SentEstimate { get; set; }
        public SentSmsStaus SentSMSStatus { get; set; }

        public SentEstimateFunctionResult()
            : base()
        { }
    }
}
