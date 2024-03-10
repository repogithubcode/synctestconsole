using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Vendors
{
    public class VendorSummaryVM
    {
        public bool IsSelected { get; set; }
        public int ID { get; set; }
        public string CompanyName { get; set; }
        public string State { get; set; }
    }
}