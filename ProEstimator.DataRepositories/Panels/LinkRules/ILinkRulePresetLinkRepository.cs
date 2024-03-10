using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.LinkRules;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.Panels.LinkRules
{
    public interface ILinkRulePresetLinkRepository
    {
        FunctionResult Save(LinkRulePresetLink linkRulePresetLink, int activeLoginID);
        FunctionResult Delete(int id);
        List<LinkRulePresetLink> GetForRule(int ruleID, string addAction = "");
    }

}
