using System.Collections.Generic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public interface ILinkRuleLineRepository
    {
        FunctionResult Save(LinkRuleLine linkRuleLine, int activeLoginID);
        LinkRuleLine Get(int id);
        List<LinkRuleLine> GetForRule(int linkRuleID);
        List<LinkRuleLine> GetAll();
        FunctionResult Delete(int id);
        LinkRuleLine Copy(LinkRuleLine lineToCopy, int newLinkRuleID);
    }
}
