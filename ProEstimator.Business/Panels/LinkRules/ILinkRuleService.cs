using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using System.Collections.Generic;

namespace ProEstimator.Business.Panels.LinkRules
{
    public interface ILinkRuleService
    {
        LinkRule GetRule(int id);
        LinkRule CopyRule(LinkRule rule);
        List<LinkRule> GetActiveRules();
        

        LinkRuleLine GetRuleLine(int id);
        List<LinkRuleLine> GetLinesForRule(int ruleID);
        FunctionResultInt SaveRuleLine(int activeLogin, int ruleID, int lineID, int sortOrder, string matchText, MatchPiece matchPiece, int childRuleID, MatchType matchType, bool indented, bool removed, bool disabled);
        FunctionResult DeleteRuleLine(int id);

        List<LinkRulePresetLink> GetPresetLinksForRule(int ruleID, string addAction = "");
        FunctionResult DeletePresetLink(LinkRulePresetLink link);


        bool LineMatchCheck(LinkRuleLine line, string input);
        bool RuleMatchCheck(LinkRule rule, string input);
        string GetRuleSummaryText(LinkRule rule);
        SectionDetailsResult GetSectionDetailsForEstimate(int estimateID, Panel panel);
        MatchDetails GetMatchDetailsForRule(Panel panel, LinkRuleType linkRuleType);
    }
}
