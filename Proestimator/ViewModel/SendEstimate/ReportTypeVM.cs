using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate
{
    public class ReportTypeVM
    {
        public string ReportName { get; set; }
        public string FileName { get; set; }
        public bool MultiLanguage { get; set; }

        public ReportTypeVM(string reportName, string fileName, bool multiLanguage = false)
        {
            ReportName = reportName;
            FileName = fileName;
            MultiLanguage = multiLanguage;
        }
    }
}