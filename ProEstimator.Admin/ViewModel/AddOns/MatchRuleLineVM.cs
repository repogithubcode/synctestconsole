using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class MatchRuleLineVM
    {
        public int ID { get; set; }
        public int ChildRuleID { get; set; }
        public int MatchType { get; set; }
        public int MatchPiece { get; set; }
        public string MatchText { get; set; }
        public bool Disabled { get; set; }
        public bool Indented { get; set; }

        public MatchRuleLineVM(AddOnRuleLine ruleLine)
        {
            ID = ruleLine.ID;
            ChildRuleID = ruleLine.ChildRuleID;
            MatchType = (int)ruleLine.MatchType;
            MatchPiece = (int)ruleLine.MatchPiece;
            MatchText = ruleLine.MatchText;
            Disabled = ruleLine.Disabled;
            Indented = ruleLine.Indented;
        }
    }
}