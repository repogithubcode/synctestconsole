using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.ProAdvisor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public interface IProAdvisorProfileRepository
    {
        ProAdvisorPresetProfile GetProfile(int profileID);
        List<ProAdvisorPresetProfile> GetAllProfilesForAccount(int loginID, bool showDeleted = false);
        FunctionResult SaveProfile(int activeLoginID, ProAdvisorPresetProfile profile);
    }
}
