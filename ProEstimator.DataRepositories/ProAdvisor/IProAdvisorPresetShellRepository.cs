using System.Collections.Generic;

using ProEstimatorData;
using ProEstimatorData.DataModel.ProAdvisor;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public interface IProAdvisorPresetShellRepository
    {
        FunctionResult Save(ProAdvisorPresetShell presetShell, int activeLoginID);
        ProAdvisorPresetShell Get(int id);
        List<ProAdvisorPresetShell> GetAll();
    }
}
