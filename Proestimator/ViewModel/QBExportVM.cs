using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel
{
    public class QBExportVM
    {
        public int LoginID { get; set; }
        public int UserID { get; set; }

        public string StartDate { get; set; }
        public string EndDate { get; set; }
    }

    public class QBExportHistoryVM
    {
        public int ID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? TimeStamp { get; set; }
        public int EstimateCount { get; set; }
        public Boolean? IsDeleted { get; set; }
    }
}