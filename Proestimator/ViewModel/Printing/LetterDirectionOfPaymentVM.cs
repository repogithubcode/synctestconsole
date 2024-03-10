using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterDirectionOfPaymentVM
    {

        public int AdminInfoID { get; set; }
        public string DateString { get; set; }
        public string TextCustomer { get; set; }
        public string TextMessage { get; set; }
        public string TextMessageHeader { get; set; }
        public string TextSignature { get; set; }
        public string TextDate { get; set; }
        public string TextName { get; set; }
        public bool ShowEstimateNumber { get; set; }
        public bool ShowID { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }
        public string TextClaimNumber { get; set; }
        public string TextPolicyNumber { get; set; }

    }
}