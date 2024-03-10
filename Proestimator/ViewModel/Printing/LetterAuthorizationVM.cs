using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterAuthorizationVM
    {

        public int AdminInfoID { get; set; }
        public string DateString { get; set; }
        public double GrandTotal { get; set; }
        public string TextCustomer { get; set; }
        public string TextMessage { get; set; }
        public string TextGrandTotal { get; set; }
        public string TextSignature { get; set; }
        public string TextOral { get; set; }
        public string TextOther { get; set; }
        public bool ShowEstimateNumber { get; set; }
        public bool ShowID { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }

    }
}