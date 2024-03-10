using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class PanelsGridRowVM
    {

        public int PanelID { get; set; }
        public string PanelName { get; set; }
        public bool Symmetry { get; set; }
        public bool Selected { get; set; }

    }
}