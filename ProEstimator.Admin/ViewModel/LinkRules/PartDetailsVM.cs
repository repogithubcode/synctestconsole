using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class PartDetailsVM
    {
        public string PartNumber { get; set; }
        public string Description { get; set; }
        public string Comment { get; set; }
        public double Price { get; set; }
        public string Barcode { get; set; }
        public string PartText { get; set; }
        public string Notes { get; set; }
    }
}