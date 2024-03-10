using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.CreateReports
{
    public class CreateReportsListVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }

        public CreateReportsListVM()
        {
            EstimateID = 1;
        }
    }
}