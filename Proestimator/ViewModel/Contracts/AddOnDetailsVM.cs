using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace Proestimator.ViewModel.Contracts
{
    public class AddOnDetailsVM
    {
        public int AddOnTypeID { get; set; }
        public bool HasAddOn { get; set; }
        public string AddOnType { get; set; }
        public string TermDescription { get; set; }
        public int AddOnID { get; set; }
        public bool NeedsPayment { get; set; }
        public bool IsBundleable { get; set; }
        public bool IsBundled { get; set; }
        public bool HasPayment { get; set; }
        public int SelectedID { get; set; }
        public bool IsMultiAdd { get; set; }
        public List<AddOnDetailsVM> AddOnDetails { get; set; }
        public int SelectedAddOnQty { get; set; }
        public SelectList AddOnQtys { get; set; }
    }
}