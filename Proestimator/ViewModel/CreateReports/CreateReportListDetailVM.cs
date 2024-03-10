using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.CreateReports
{
    public class CreateReportHistoryListVM
    {
        public int ID { get; set; }
        public int LoginID { get; set; }
        public string FileName { get; set; }
        public string ReportType { get; set; }

        private string _ReportFormat;
        public string ReportFormat {
            get
            {
                string _ReportFormat = Path.GetExtension(FileName);
                return _ReportFormat;
            }
        }

        public string CreatedTimeStamp { get; set; }
    }
}
