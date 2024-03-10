using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class PanelDetailsVM
    {

        public int ID { get; set; }
        public string PanelName { get; set; }
        public int SortOrder { get; set; }
        public bool Symmetry { get; set; }

        public int SectionLinkRuleID { get; set; }
        public int PrimarySectionLinkRuleID { get; set; }
        public int PrimaryPanelLinkRuleID { get; set; }

        public string SectionLinkSummary { get; set; }
        public string PrimarySectionLinkSummary { get; set; }
        public string PrimaryPanelLinkSummary { get; set; }

    }
}