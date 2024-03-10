using ProEstimator.Admin.ViewModel.LinkRules;
using Proestimator.Admin.ViewModelMappers;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Admin.ViewModelMappers.LinkRules
{
    public class LinkRuleLineVMMapper : IVMMapper<LinkRuleLineVM>
    {
        public LinkRuleLineVM Map(MappingConfiguration mappingConfiguration)
        {
            LinkRuleLineVMMapperConfiguration config = mappingConfiguration as LinkRuleLineVMMapperConfiguration;

            LinkRuleLineVM vm = new LinkRuleLineVM
            {
                ID = config.RuleLine.ID,
                ChildRuleID = config.RuleLine.ChildRuleID,
                MatchType = (int)config.RuleLine.MatchType,
                MatchPiece = (int)config.RuleLine.MatchPiece,
                MatchText = config.RuleLine.MatchText,
                Disabled = config.RuleLine.Disabled,
                Indented = config.RuleLine.Indented
            };

            return vm;
        }
    }

    public class LinkRuleLineVMMapperConfiguration : MappingConfiguration
    {
        public LinkRuleLine RuleLine { get; set; }
    }
}