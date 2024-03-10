using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace ProEstimator.Admin.ViewModel.ContractReport
{
    public class ContractReportVM
    {
        public SelectList SalesRepDDL { get; set; }
        public string SelectedSalesRep { get; set; }
    }
}