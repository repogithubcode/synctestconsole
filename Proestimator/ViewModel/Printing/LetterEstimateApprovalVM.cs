using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Configuration;

namespace Proestimator.ViewModel.Printing
{
    public class LetterEstimateApprovalVM
    {
        public int EstimateID { get; set; }
        public string ReportName { get; set; }
        public bool ShowEstimateNumber { get; set; }
        public bool ShowID { get; set; }
        public string UserContentPath { get { return ConfigurationManager.AppSettings.Get("UserContentPath").ToString().Replace(@"\", "/"); } }

    }
}