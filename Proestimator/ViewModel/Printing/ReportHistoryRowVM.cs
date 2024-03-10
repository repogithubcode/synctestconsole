using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace Proestimator.ViewModel.Printing
{
    public class ReportHistoryRowVM
    {

        public int ReportID { get; set; }
        public int MailID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string FileName { get; set; }
        public string ReportTypeTag { get; set; }
        public string ReportType { get; set; }
        public int CustomTemplateID { get; set; }
        public string DeleteRestoreImgName { get; set; }
        public bool IsDeleted { get; set; }
        public string Notes { get; set; }

    }
}