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
    internal class GetRuleSummaryTextCommand : CommandBase
    {
        private ILinkRuleLineRepository _linkRuleLineRepository;
        private LinkRule _linkRule;

        public string SummaryText { get; private set; }

        public GetRuleSummaryTextCommand(LinkRule linkRule, ILinkRuleLineRepository linkRuleLineRepository)
        {
            _linkRule = linkRule;
            _linkRuleLineRepository = linkRuleLineRepository;
        }

        public override bool Execute()
        {
            StringBuilder builder = new StringBuilder();

            List<LinkRuleLine> lines = _linkRuleLineRepository.GetForRule(_linkRule.ID);
            foreach (LinkRuleLine line in lines)
            {
                if (lines.IndexOf(line) > 0)
                {
                    if (line.Indented)
                    {
                        builder.Append(" OR ");
                    }
                    else
                    {
                        builder.Append(" AND ");
                    }
                }

                builder.Append(line.MatchPiece.ToString() + " ");

                if (line.MatchType == MatchType.ExactMatch)
                {
                    builder.Append("=");
                }
                else
                {
                    builder.Append(line.MatchType.ToString());
                }

                builder.Append(" '" + line.MatchText + "'");
            }

            SummaryText = builder.ToString();
            return true;
        }
    }
}
