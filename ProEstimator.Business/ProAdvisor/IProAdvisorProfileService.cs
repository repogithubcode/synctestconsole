using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DocumentFormat.OpenXml.Drawing;
using ProEstimator.Business.Logic;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.ProAdvisor;

namespace ProEstimator.Business.ProAdvisor
{
    public interface IProAdvisorProfileService
    {

        List<ProAdvisorPresetProfile> GetAllProfilesForLogin(int loginID, bool showDeleted);
        ProAdvisorPresetProfile GetProfile(int profileID);
        ProAdvisorPresetProfile GetDefaultProfile(int loginID);
        FunctionResultInt CopyProfile(int profileID, int loginID, int activeLoginID);
        FunctionResult SetDefaultProfile(int loginID, int profileID);
        FunctionResult DeleteRateProfile(int activeLoginID, int loginID, int userID,  int profileID);
        bool UseDefaultProfile(int loginID);
        void SetUseDefaultProfile(int activeLoginID, int loginID, bool useDefault);
        List<ProAdvisorPreset> GetPresetsByFilters(int siteUserID, int profileID, bool hasProAdvisorContract, string name, string operationType, string laborType);
        FunctionResult SavePresets(int activeLoginID, int userID, int profileID, string profileName, List<PresetData> presets);
    }
}
