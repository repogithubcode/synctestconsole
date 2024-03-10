using System.Collections.Generic;

using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.ProAdvisor
{
    public interface IProAdvisorService
    {

        List<ProAdvisorRecommendation> GetRecommendations(Estimate estimate, int sectionKey, string sectionDescription, string addAction);
        FunctionResult AddPresetToEstimate(int estimateID, int presetID, int parentLineID);
        double GetAddOnTotalForLogin(int loginID);
        void RefreshTotalForEstimate(int estimateID);
    }
}
