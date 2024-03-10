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
    internal class LineMatchCheckCommand : CommandBase
    {

        private ILinkRuleRepository _linkRuleRepository;
        private ILinkRuleLineRepository _lineRuleLineRepository;
        private LinkRuleLine _line;
        private string _input;

        public LineMatchCheckCommand(LinkRuleLine linkRuleLine, string input, ILinkRuleRepository linkRuleRepository, ILinkRuleLineRepository linkRuleLineRepository)
        {
            _line = linkRuleLine;
            _input = input;
            _linkRuleRepository = linkRuleRepository;
            _lineRuleLineRepository = linkRuleLineRepository;
        }

        public override bool Execute()
        {
            string matchText = _line.MatchText.ToLower().Trim();
            string processedInput = _input.ToLower().Trim();

            string categoryPart = ProcessInputText(processedInput);
            string subCategoryPart = "";

            string[] pieces = processedInput.Split("\\".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            if (pieces.Length > 0)
            {
                categoryPart = pieces[0].ToLower().Trim();
            }
            if (pieces.Length > 1)
            {
                subCategoryPart = pieces[1].ToLower().Trim();
            }

            if (_line.MatchPiece == MatchPiece.Part)
            {
                if
                (
                    (_line.MatchType == MatchType.Include && categoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.NotInclude && !categoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.ExactMatch && categoryPart == matchText)
                )
                {
                    return true;
                }
            }
            else if (_line.MatchPiece == MatchPiece.Category)
            {
                if
                (
                    (_line.MatchType == MatchType.Include && categoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.NotInclude && !categoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.ExactMatch && categoryPart == matchText)
                )
                {
                    return true;
                }
            }
            else if (_line.MatchPiece == MatchPiece.SubCategory)
            {
                if
                (
                    (_line.MatchType == MatchType.Include && subCategoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.NotInclude && !subCategoryPart.Contains(matchText))
                    ||
                    (_line.MatchType == MatchType.ExactMatch && subCategoryPart == matchText)
                )
                {
                    return true;
                }
            }
            else if (_line.MatchPiece == MatchPiece.ChildRule)
            {
                LinkRule childRule = _linkRuleRepository.Get(_line.ChildRuleID);

                RuleMatchCheckCommand ruleMatchCommand = new RuleMatchCheckCommand(childRule, _input, _lineRuleLineRepository, _linkRuleRepository);
                return ruleMatchCommand.Execute();
            }

            return false;
        }

        private string ProcessInputText(string input)
        {
            // This is a hack to deal with a data issue.  In the Mitchel database, some part descriptions end with -M or -S to mean Mechanical or Structural.  
            // These are spaced out to be at the very end of the 40 characters of the description, so it'll look like:
            // A/C REFRIGERANT RECOVERY              -M
            // The function will remove those spaces and return
            // A/C REFRIGERANT RECOVERY -M
            if (input.Length == 40 && input[38] == '-')
            {
                string beginning = input.Substring(0, 37).TrimEnd();
                string ending = input.Substring(38, 2);

                input = beginning + " " + ending;
            }

            // Remove the l or r for left/right when doing the compare, match text ignores sides.
            if (input.StartsWith("l ") || input.StartsWith("r "))
            {
                input = input.Substring(2, input.Length - 2);
            }

            return input;
        }

    }
}
