using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Web.Mvc;
using ProEstimatorData.Models.SubModel;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;

namespace Proestimator.ViewModel
{
    public class VendorIndexVM
    {
        public List<SimpleListItem> VendorTypes { get; set; }
        public string SearchText { get; set; }
        public int LoginID { get; set; }
        public bool IsAdmin { get { return false; }} // TODO Refactor return SiteSession.Current.IsAdmin; } }
        public bool ShowRepairFacility { get; set; }
        public bool ShowAlternateIdentity { get; set; }

        public VendorIndexVM()
        {
            SearchText = "";
            VendorTypes = new List<SimpleListItem>();
        }
    }
   
}