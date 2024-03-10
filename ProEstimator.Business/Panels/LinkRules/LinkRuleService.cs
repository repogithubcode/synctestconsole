using System.Collections.Generic;
using System.Linq;

using ProEstimator.Business.Panels.LinkRules.Commands;
using ProEstimator.DataRepositories.Panels.LinkRules;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.Business.Panels.LinkRules
{
    public class LinkRuleService : ILinkRuleService
    {
        private ILinkRuleRepository _ruleRepository;
        private ILinkRuleLineRepository _lineRepository;
        private ILinkRulePresetLinkRepository _presetLinkRepository;

        public LinkRuleService(ILinkRuleRepository ruleRepository, ILinkRuleLineRepository linkRuleLineRepository, ILinkRulePresetLinkRepository presetLinkRepository)
        {
            _ruleRepository = ruleRepository;
            _lineRepository = linkRuleLineRepository;
            _presetLinkRepository = presetLinkRepository;
        }

        /// <summary>
        /// Gets a LinkRule by it's unique ID.
        /// </summary>
        public LinkRule GetRule(int id)
        {
            return _ruleRepository.Get(id);
        }

        public LinkRule CopyRule(LinkRule rule)
        {
            CopyRuleCommand copyCommand = new CopyRuleCommand(_lineRepository, _ruleRepository, rule);
            copyCommand.Execute();
            return copyCommand.NewRule;
        }

        /// <summary>
        /// Get all non-deleted, enabled rules.
        /// </summary>
        public List<LinkRule> GetActiveRules()
        {
            return _ruleRepository.GetAll().Where(o => o.Deleted == false && o.Enabled == true).ToList();
        }        


        public LinkRuleLine GetRuleLine(int id)
        {
            return _lineRepository.Get(id);
        }

        public List<LinkRuleLine> GetLinesForRule(int ruleID)
        {
            return _lineRepository.GetForRule(ruleID);
        }

        public FunctionResultInt SaveRuleLine(int activeLoginID, int ruleID, int lineID, int sortOrder, string matchText, MatchPiece matchPiece, int childRuleID, MatchType matchType, bool indented, bool removed, bool disabled)
        {
            LinkRuleLine ruleLine = new LinkRuleLine();

            if (lineID > 0)
            {
                ruleLine = GetRuleLine(lineID);
            }

            if (removed)
            {
                if (lineID > 0)
                {
                    DeleteRuleLine(lineID);
                }

                return new FunctionResultInt(0);
            }
            else
            {
                ruleLine.LinkRuleID = ruleID;
                ruleLine.SortOrder = sortOrder;
                ruleLine.MatchText = matchText.ToLower().Trim();
                ruleLine.MatchPiece = (MatchPiece)matchPiece;
                ruleLine.ChildRuleID = childRuleID;
                ruleLine.MatchType = (MatchType)matchType;
                ruleLine.Indented = indented;
                ruleLine.Disabled = disabled;

                FunctionResult saveResult = _lineRepository.Save(ruleLine, activeLoginID);
                if (saveResult.Success)
                {
                    return new FunctionResultInt(ruleLine.ID);
                }
                else
                {
                    return new FunctionResultInt(saveResult.ErrorMessage);
                }
            }
        }

        public FunctionResult DeleteRuleLine(int id)
        {
            return _lineRepository.Delete(id);
        }

        public List<LinkRulePresetLink> GetPresetLinksForRule(int ruleID, string addAction = "")
        {
            return _presetLinkRepository.GetForRule(ruleID, addAction).ToList();
        }

        public FunctionResult DeletePresetLink(LinkRulePresetLink link)
        {
            return _presetLinkRepository.Delete(link.ID);
        }

        /// <summary>
        /// Returns True if the passed input text is a match for the passed RuleLine.
        /// </summary>
        public bool LineMatchCheck(LinkRuleLine line, string input)
        {
            LineMatchCheckCommand command = new LineMatchCheckCommand(line, input, _ruleRepository, _lineRepository);
            return command.Execute();
        }

        /// <summary>
        /// Returns True if the passed input satisfies the True condition for all lines in the passed Rule
        /// </summary>
        public bool RuleMatchCheck(LinkRule rule, string input)
        {
            RuleMatchCheckCommand command = new RuleMatchCheckCommand(rule, input, _lineRepository, _ruleRepository);
            return command.Execute();
        }

        public string GetRuleSummaryText(LinkRule rule)
        {
            GetRuleSummaryTextCommand command = new GetRuleSummaryTextCommand(rule, _lineRepository);
            command.Execute();
            return command.SummaryText;
        }
        

        /// <summary>
        /// Get a list of available sub sections for a panel on the vehicle assocaited with the estimate
        /// </summary>
        public SectionDetailsResult GetSectionDetailsForEstimate(int estimateID, Panel panel)
        {
            GetSectionDetailsForEstimateCommand command = new GetSectionDetailsForEstimateCommand(_lineRepository, estimateID, panel);
            if(command.Execute())
            {
                return command.SectionDetailsResult;
            }
            else
            {
                return new SectionDetailsResult(command.ErrorMessage);
            }
        }

        /// <summary>
        /// For the Admin page, return details about how many matches a rule has and a list of vehicles with no match.
        /// </summary>
        public MatchDetails GetMatchDetailsForRule(Panel panel, LinkRuleType linkRuleType)
        {
            GetMatchDetailsForRuleCommand command = new GetMatchDetailsForRuleCommand(_lineRepository, panel, linkRuleType);
            command.Execute();
            return command.MatchDetails;
        }
        
    }
}
