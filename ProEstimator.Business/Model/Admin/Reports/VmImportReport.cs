using System.Collections.Generic;

namespace ProEstimator.Business.Model.Admin.Reports
{
    public class VmImportReport
    {
        public VmImportReport()
        {
            this.Detail = new List<ImportLineItem>();
        }
        public string FromDate { get; set; }
        public string ToDate { get; set; }
        public List<ImportLineItem> Detail { get; set; }
    }
}
