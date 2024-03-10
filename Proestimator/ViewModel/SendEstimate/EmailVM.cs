using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate
{
    public class EmailVM
    {

        public string ToAddresses { get; set; }
        public string CCAddresses { get; set; }
        public string PhoneNumbers { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public List<ReportVM> ReportAttachments { get; set; }
        public string SentTimeMessage { get; set; }
        public string ErrorMessage { get; set; }
        public List<StatusMessageVM> StatusMessage { get; set; }

        public EmailVM()
        {
            ReportAttachments = new List<ReportVM>();
            StatusMessage = new List<StatusMessageVM>();
        }

    }
}