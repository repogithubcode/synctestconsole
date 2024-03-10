using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using Proestimator.Admin.ViewModelMappers;
using ProEstimator.Admin.ViewModel.LinkRules;
using ProEstimator.Business.Panels.LinkRules;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Admin.ViewModelMappers.LinkRules
{
    public class LinkRuleVMMapper : IVMMapper<LinkRuleVM>
    {
        public LinkRuleVM Map(MappingConfiguration mappingConfiguration)
        {
            LinkRuleVMMapperConfiguration config = mappingConfiguration as LinkRuleVMMapperConfiguration;

            LinkRuleVM vm = new LinkRuleVM();

            vm.ID = config.LinkRule.ID;
            vm.TypeID = (int)config.LinkRule.RuleType;
            vm.Enabled = config.LinkRule.Enabled;

            List<LinkRuleLine> ruleLines = config.LinkRuleService.GetLinesForRule(config.LinkRule.ID);

            vm.MatchTexts = new List<LinkRuleLineVM>();
            LinkRuleLineVMMapper mapper = new LinkRuleLineVMMapper();
            ruleLines.ForEach(o => vm.MatchTexts.Add(mapper.Map(new LinkRuleLineVMMapperConfiguration() { RuleLine = o })));

            List<LinkRule> rules = config.LinkRuleService.GetActiveRules().Where(o => o.ID != config.LinkRule.ID).ToList();
            vm.RulesDropDown = new List<LinkRuleSmallVM>();
            rules.ForEach(o => vm.RulesDropDown.Add(new LinkRuleSmallVM(o)));

            vm.SameAsParent = config.SameAsParent;

            return vm;
        }
    }

    public class LinkRuleVMMapperConfiguration : MappingConfiguration
    {
        public ILinkRuleService LinkRuleService { get; set; }
        public LinkRule LinkRule { get; set; }
        public bool SameAsParent { get; set; } = false;
    }
}