using System.Collections.Generic;

using ProEstimatorData;
using ProEstimatorData.DataModel.ProAdvisor;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public interface IProAdvisorPresetRepository
    {
        FunctionResult Save(ProAdvisorPreset preset, int activeLoginID);
        ProAdvisorPreset Get(int id);
        List<ProAdvisorPreset> GetForRateProfile(int profileID);
        List<ProAdvisorPreset> GetForRuleAndProfile(int estimateID, int ruleID, int profileID, string action);
    }
}
