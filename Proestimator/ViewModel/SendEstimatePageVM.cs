using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

using Proestimator.DataAttributes;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class SendEstimatePageVM
    {
        public int EstimateID { get; set; }

        public string EmailAddressOwner { get; set; }
        public string EmailAddressClaimant { get; set; }
        public string EmailAddressInsuranceAgent { get; set; }
        public string EmailAddressInsuranceAdjuster { get; set; }
        public string EmailClaimRepresentative { get; set; }

        public bool SendToOwner { get; set; }
        public bool SendToClaimant { get; set; }
        public bool SendToInsuranceAgent { get; set; }
        public bool SendToInsuranceAdjuster { get; set; }
        public bool SendToClaimRepresentative { get; set; }

        public string InfoOwner { get; set; }
        public string InfoClaimant { get; set; }
        public string InfoInsuranceAgent { get; set; }
        public string InfoInsuranceAdjuster { get; set; }
        public string InfoClaimRepresentative { get; set; }

        public string OtherTarget1 { get; set; }
        public string OtherTarget2 { get; set; }
        public string OtherTarget3 { get; set; }

        public string Message { get; set; }
        public bool NoData { get; set; }

        public string EMSExportMessage { get; set; }

        public List<SentEstimateVM> SentList { get; set; }
        public string Subject { get; set; }
        public string CustomMessage { get; set; }

        public string OwnerPhoneNumber { get; set; }
        public string ClaimantPhoneNumber { get; set; }
        public string InsuranceAgentPhoneNumber { get; set; }
        public string InsuranceAdjusterPhoneNumber { get; set; }
        public string ClaimRepresentativePhoneNumber { get; set; }

        public string SMSBody { get; set; }
        public bool AttachPdfInSMS { get; set; }

        public bool AllowSMS { get; set; }
        public bool AllowEMS { get; set; }
        public string SMSIncludeMessage { get; set; }
    }

    public class SentEstimateVM
    {
        public int MessageQueueID { get; set; }
        public string Recipients { get; set; }
        public string Status { get; set; }
        public string ErrorMessage { get; set; }
        public string ReportType { get; set; }
        public string TimeStampString { get; set; }
        public string Link { get; set; }

        public SentEstimateVM()
        {
            MessageQueueID = 0;
            Recipients = "";
            Status = "";
            ErrorMessage = "";
            ReportType = "";
            TimeStampString = "";
            Link = "";
        }

        public SentEstimateVM(SentEstimate sentEstimate)
        {
            Initialize(sentEstimate, null);
            ReportType = "Email";
        }

        public SentEstimateVM(SentEstimate sentEstimate, SentSmsStaus smsStatus)
        {
            Initialize(sentEstimate, smsStatus);

            if (smsStatus == null)
            {
                ReportType = "Email";
            }
            else
            {
                ReportType = "SMS";
            }
        }

        private void Initialize(SentEstimate sentEstimate, SentSmsStaus smsStatus)
        {
            TimeStampString = sentEstimate.TimeStamp.ToString();
            Link = "/Estimate/EstimateReport/" + sentEstimate.ReportFileName;

            StringBuilder builder = new StringBuilder();
            foreach (string recipient in sentEstimate.Recipients)
            {
                if (sentEstimate.Recipients.IndexOf(recipient) > 0)
                {
                    builder.AppendLine(recipient);
                }
                else
                {
                    builder.Append(recipient);
                }
            }

            Recipients = builder.ToString();

            if (smsStatus != null)
            {
                ErrorMessage = smsStatus.ErrorMessage;
                Status = smsStatus.Status;
            }
        }
    }
}
