using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class RuleGridRowVM
    {
        public int ID { get; set; }
        public bool Active { get; set; }
        public string Name { get; set; }
        public string SectionMatch { get; set; }
        public string PartMatch { get; set; }

        public RuleGridRowVM(AddOnRule rule, List<AddOnRuleLine> matches)
        {
            ID = rule.ID;
            Name = rule.Name;
            Active = rule.Active;

            string sectionMatchSeperator = "";
            string partMatchSeperator = "";

            SectionMatch = "";
            PartMatch = "";

            foreach(AddOnRuleLine match in matches)
            {
                if (match.MatchPiece == MatchPiece.Section)
                {
                    SectionMatch += sectionMatchSeperator + (match.MatchType == MatchType.Include ? match.MatchText : "<span class='not-include'>" + match.MatchText + "</span>");
                    sectionMatchSeperator = ", ";
                }
                else
                {
                    PartMatch += partMatchSeperator + (match.MatchType == MatchType.Include || match.MatchType == MatchType.ExactMatch ? match.MatchText : "<span class='not-include'>" + match.MatchText + "</span>");
                    partMatchSeperator = ", ";
                }
            }
        }
    }
}