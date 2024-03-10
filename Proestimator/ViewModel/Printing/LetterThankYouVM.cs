using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterThankYouVM
    {

        public int AdminInfoID { get; set; }
        public string DateString { get; set; }
        public string TextCustomer { get; set; }
        public string TextMessage { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }

    }
}