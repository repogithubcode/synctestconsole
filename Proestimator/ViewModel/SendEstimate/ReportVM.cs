using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.SendEstimate
{
    public class ReportVM
    {
        public int ID { get; set; }
        public string ReportType { get; set; }
        public string ReportName { get; set; }

        public ReportVM(int id, string reportType, string reportName)
        {
            ID = id;
            ReportType = reportType;
            ReportName = reportName;
        }
    }
}