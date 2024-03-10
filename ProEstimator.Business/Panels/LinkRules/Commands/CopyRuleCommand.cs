using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Panels.LinkRules.Commands
{
    internal class CopyRuleCommand : CommandBase
    {
        private ILinkRuleLineRepository _linkRuleLineRepository;
        private ILinkRuleRepository _linkRuleRepository;
        private LinkRule _linkRule;

        public LinkRule NewRule { get; private set; }

        public CopyRuleCommand(ILinkRuleLineRepository repository, ILinkRuleRepository linkRuleRepository, LinkRule rule)
        {
            _linkRuleLineRepository = repository;
            _linkRuleRepository = linkRuleRepository;
            _linkRule = rule;
        }

        public override bool Execute()
        {
            NewRule = new LinkRule();
            NewRule.RuleType = _linkRule.RuleType;
            NewRule.Deleted = _linkRule.Deleted;
            NewRule.Enabled = _linkRule.Enabled;
            
            FunctionResult saveResult = _linkRuleRepository.Save(NewRule, 0);
            if (saveResult.Success)
            {
                List<LinkRuleLine> lines = _linkRuleLineRepository.GetForRule(_linkRule.ID);
                foreach (LinkRuleLine line in lines)
                {
                    _linkRuleLineRepository.Copy(line, NewRule.ID);
                }

                return true;
            }
            else
            {
                return Error(saveResult.ErrorMessage);
            }
        }

    }
}
