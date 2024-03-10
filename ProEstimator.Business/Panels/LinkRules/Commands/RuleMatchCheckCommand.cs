using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel.LinkRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Panels.LinkRules.Commands
{
    internal class RuleMatchCheckCommand : CommandBase
    {
        private LinkRule _linkRule;
        private string _input;
        private ILinkRuleLineRepository _linkRuleLineRepository;
        private ILinkRuleRepository _linkRuleRepository;

        public RuleMatchCheckCommand(LinkRule rule, string input, ILinkRuleLineRepository linkRuleLineRepository, ILinkRuleRepository linkRuleRepository)
        {
            _linkRule  = rule;
            _input = input;
            _linkRuleLineRepository = linkRuleLineRepository;
            _linkRuleRepository = linkRuleRepository;
        }

        public override bool Execute()
        {
            List<LinkRuleLine> lines = _linkRuleLineRepository.GetForRule(_linkRule.ID);

            LinkRuleLine currentANDLine = null;
            bool currentLineMatch = false;

            foreach (LinkRuleLine line in lines)
            {
                // If the last AND line didn't match
                if (!line.Indented && currentANDLine != null && !currentLineMatch)
                {
                    return false;
                }

                if (!line.Indented)
                {
                    currentLineMatch = false;
                }

                LineMatchCheckCommand lineMatchCheckCommand = new LineMatchCheckCommand(line, _input, _linkRuleRepository, _linkRuleLineRepository);
                bool lineMatch = lineMatchCheckCommand.Execute();

                if (lineMatch)
                {
                    currentLineMatch = true;
                }

                if (!line.Indented)
                {
                    currentANDLine = line;
                }
            }

            return currentLineMatch;
        }

    }
}
