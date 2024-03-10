using System.Collections.Generic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public interface ILinkRuleRepository
    {
        FunctionResult Save(LinkRule linkRule, int activeLoginID);
        LinkRule Get(int ruleID);
        List<LinkRule> GetAll();
    }
}
