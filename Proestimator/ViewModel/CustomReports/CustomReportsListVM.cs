using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.CustomReports
{
    public class CustomReportsListVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public CustomReportsListVM()
        {
            EstimateID = 1;
        }
    }
}