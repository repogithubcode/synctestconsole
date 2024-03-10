using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Admin.ViewModel.LinkRules
{
    public class LinkRuleVM
    {
        public int ID { get; set; }
        public int ParentPanelID { get; set; }
        public int TypeID { get; set; }
        public bool Enabled { get; set; }
        public List<LinkRuleLineVM> MatchTexts { get; set; }
        public List<LinkRuleSmallVM> RulesDropDown { get; set; }
        public bool SameAsParent { get; set; }

    }

    public class LinkRuleSmallVM
    {
        public int ID { get; set; }

        public LinkRuleSmallVM(LinkRule linkRule)
        {
            ID = linkRule.ID;
        }
    }
}