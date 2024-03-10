using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class ExtensionHistoryVM
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public DateTime OnDate { get; set; }
        public string ExtendedBy { get; set; }

        public ExtensionHistoryVM()
        {

        }

        public ExtensionHistoryVM(DataRow row)
        {
            FromDate = InputHelper.GetDateTime(row["extendfrom"].ToString());
            ToDate = InputHelper.GetDateTime(row["extendto"].ToString());
            OnDate = InputHelper.GetDateTime(row["extendeddate"].ToString());
            ExtendedBy = InputHelper.GetString(row["SalesRep"].ToString());
        }
    }
}