using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.AddOns;

namespace ProEstimator.Admin.ViewModel.AddOns
{
    public class AddOnRuleVM
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public bool Active { get; set; }
        public List<MatchRuleLineVM> MatchTexts { get; set; }
        public List<AddOnRuleSmallVM> RulesDropDown { get; set; }

        public AddOnRuleVM(AddOnRule addOnRule)
        {
            ID = addOnRule.ID;
            Name = addOnRule.Name;
            Active = addOnRule.Active;

            List<AddOnRuleLine> matchTexts = AddOnRuleLine.GetForRule(addOnRule.ID);

            MatchTexts = new List<MatchRuleLineVM>();
            matchTexts.ForEach(o => MatchTexts.Add(new MatchRuleLineVM(o)));

            List<AddOnRule> rules = AddOnRule.GetAll().Where(o => o.ID != addOnRule.ID && !o.Deleted).ToList();
            RulesDropDown = new List<AddOnRuleSmallVM>();
            rules.ForEach(o => RulesDropDown.Add(new AddOnRuleSmallVM(o)));
        }
    }

    public class AddOnRuleSmallVM
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public AddOnRuleSmallVM(AddOnRule addOnRule)
        {
            ID = addOnRule.ID;
            Name = addOnRule.Name;
        }
    }

}