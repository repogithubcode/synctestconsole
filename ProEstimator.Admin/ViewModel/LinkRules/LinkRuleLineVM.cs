using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class LinkRuleLineVM
    {
        public int ID { get; set; }
        public int ChildRuleID { get; set; }
        public int MatchType { get; set; }
        public int MatchPiece { get; set; }
        public string MatchText { get; set; }
        public bool Disabled { get; set; }
        public bool Indented { get; set; }
    }
}