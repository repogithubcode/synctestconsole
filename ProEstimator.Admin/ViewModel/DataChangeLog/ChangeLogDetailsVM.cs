using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.DataChangeLog
{
    public class ChangeLogDetailsVM
    {
        public string TimeStamp { get; set; }
        public string SiteUser { get; set; }
        public string EmailAddress { get; set; }
        public string Browser { get; set; }
        public string Device { get; set; }
        public string ComputerKey { get; set; }

        public List<ChangeLogItem> ItemDetails { get; set; }

        public ChangeLogDetailsVM()
        {
            ItemDetails = new List<ChangeLogItem>();
        }
    }
}