using ProEstimator.DataRepositories.ProAdvisor;
using ProEstimatorData.DataModel;
using ProEstimatorData;
using ProEstimatorData.DataModel.ProAdvisor;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Logic;
using System.Web.Mvc;
using DocumentFormat.OpenXml.Spreadsheet;

namespace ProEstimator.Business.ProAdvisor
{
    public class ProAdvisorProfileService : IProAdvisorProfileService
    {
        private IProAdvisorProfileRepository _proAdvisorProfileRepository;
        private IProAdvisorPresetRepository _proAdvisorPresetRepository;
        private IProAdvisorPresetShellRepository _proAdvisorPresetShellRepository;

        public ProAdvisorProfileService(IProAdvisorProfileRepository proAdvisorProfileRepository, IProAdvisorPresetRepository proAdvisorPresetRepository, IProAdvisorPresetShellRepository proAdvisorPresetShellRepository) 
        {
            _proAdvisorProfileRepository = proAdvisorProfileRepository;
            _proAdvisorPresetRepository = proAdvisorPresetRepository;
            _proAdvisorPresetShellRepository = proAdvisorPresetShellRepository;
        }

        public List<ProAdvisorPresetProfile> GetAllProfilesForLogin(int loginID, bool showDeleted)
        {
            return _proAdvisorProfileRepository.GetAllProfilesForAccount(loginID, showDeleted);
        }

        public ProAdvisorPresetProfile GetProfile(int profileID)
        {
            return _proAdvisorProfileRepository.GetProfile(profileID);
        }

        public ProAdvisorPresetProfile GetDefaultProfile(int loginID)
        {
            List<ProAdvisorPresetProfile> allProfiles = GetAllProfilesForLogin(loginID, false);
            return allProfiles.FirstOrDefault(o => o.DefaultFlag == true);
        }

        public FunctionResultInt CopyProfile(int profileID, int loginID, int activeLoginID)
        {
            ProAdvisorPresetProfile originalProfile = _proAdvisorProfileRepository.GetProfile(profileID);

            ProAdvisorPresetProfile newProfile = new ProAdvisorPresetProfile();
            newProfile.DefaultFlag = false;
            newProfile.Deleted = false;
            newProfile.LoginID = loginID;
            newProfile.Name = originalProfile == null ? "System Default" : originalProfile.Name;

            if (newProfile.Name == "System Default")
            {
                newProfile.Name = "My Profile";
            }

            FunctionResult saveResult = _proAdvisorProfileRepository.SaveProfile(activeLoginID, newProfile);

            if (saveResult.Success)
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("SourceProfileID", profileID));
                parameters.Add(new SqlParameter("TargetProfileID", newProfile.ID));

                DBAccess db = new DBAccess();
                db.ExecuteNonQuery("AddOnPreset_CopyForProfile", parameters);

                return new FunctionResultInt(newProfile.ID);
            }

            return new FunctionResultInt(saveResult.ErrorMessage);
        }

        public FunctionResult SetDefaultProfile(int loginID, int profileID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("ProfileID", profileID));

            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("AddOnPresetProfile_SetDefaultProfile", parameters);
        }

        public FunctionResult DeleteRateProfile(int activeLoginID, int loginID, int userID, int profileID)
        {
            if (profileID == 1)
            {
                return new FunctionResult("Cannot delete the system default profile.");
            }

            List<ProAdvisorPresetProfile> allProfiles = GetAllProfilesForLogin(loginID, false);

            //if (allProfiles.Count == 1)
            //{
            //    return Json(new FunctionResult(false, @Proestimator.Resources.ProStrings.CannotDeleteOnlyProfile), JsonRequestBehavior.AllowGet);
            //}

            ProAdvisorPresetProfile profile = allProfiles.FirstOrDefault(o => o.ID == profileID);
            if (profile != null && profile.LoginID == loginID)
            {
                profile.Deleted = true;
                return _proAdvisorProfileRepository.SaveProfile(activeLoginID, profile);
            }
            else
            {
                return new FunctionResult(false, "Invalid rate profile");
            }
        }

        public bool UseDefaultProfile(int loginID)
        {
            return InputHelper.GetBoolean(SiteSettings.Get(loginID, "UseDefaultAddOnProfile", "Profiles", "").ValueString);
        }

        public void SetUseDefaultProfile(int activeLoginID, int loginID, bool useDefault)
        {
            SiteSettings.SaveSetting(activeLoginID, loginID, "UseDefaultAddOnProfile", "Profiles", useDefault.ToString());
        }

        public List<ProAdvisorPreset> GetPresetsByFilters(int siteUserID, int profileID, bool hasProAdvisorContract, string name, string operationType, string laborType)
        {
            List<ProAdvisorPresetShell> shells = _proAdvisorPresetShellRepository.GetAll().Where(o => !o.Deleted).OrderBy(o => o.Name).ToList();
            List<ProAdvisorPreset> presets = _proAdvisorPresetRepository.GetForRateProfile(profileID);
            List<ProAdvisorPreset> globalPresets = _proAdvisorPresetRepository.GetForRateProfile(1); 

            List<ProAdvisorPreset> returnList = new List<ProAdvisorPreset>();

            foreach (ProAdvisorPresetShell shell in shells)
            {
                bool globalIsActive = true;

                // If not the system default, hide this preset if it's hidden on the system default
                if (profileID > 1)
                {
                    ProAdvisorPreset globalPreset = globalPresets.FirstOrDefault(o => o.PresetShell != null && o.PresetShell.ID == shell.ID);
                    if (globalPreset != null && !globalPreset.Active)
                    {
                        globalIsActive = false;
                    }
                }

                if (
                    (string.IsNullOrEmpty(name) || shell.Name.ToLower().Contains(name))         // Filter by name
                    && (operationType == "All" || shell.OperationType == operationType)         // Filter by operation type
                    && (laborType == "All" || shell.LaborType == laborType)                     // Filter by labor type
                    && (shell.AccessLevel == 0 || hasProAdvisorContract)                 // Don't show Paid presets unless the user has a contract
                    && globalIsActive                                                           // Don't show if the system default profile has this one inactive
                )
                {
                    ProAdvisorPreset preset = presets.FirstOrDefault(o => o.PresetShell.ID == shell.ID);
                    if (preset == null)
                    {
                        preset = new ProAdvisorPreset();
                        preset.PresetShell = shell;
                        preset.Active = true;
                    }

                    returnList.Add(preset);
                }
            }

            return returnList;
        }

        public FunctionResult SavePresets(int activeLoginID, int userID, int profileID, string profileName, List<PresetData> presets)
        {
            FunctionResult result = new FunctionResult();
            StringBuilder builder = new StringBuilder();

            ProAdvisorPresetProfile profile = _proAdvisorProfileRepository.GetProfile(profileID);
            profile.Name = profileName;

            FunctionResult profileSaveResult = _proAdvisorProfileRepository.SaveProfile(activeLoginID, profile);

            if (!profileSaveResult.Success)
            {
                builder.AppendLine("Error saving profile name: " + profileSaveResult.ErrorMessage);
            }

            List<ProAdvisorPreset> existingPresets = _proAdvisorPresetRepository.GetForRateProfile(profileID);

            foreach (PresetData data in presets)
            {
                try
                {
                    ProAdvisorPreset addOnPreset = null;

                    if (data.PresetID == 0)
                    {
                        addOnPreset = new ProAdvisorPreset();
                        addOnPreset.ProfileID = profileID;
                        addOnPreset.PresetShell = _proAdvisorPresetShellRepository.Get(data.ShellID);
                    }
                    else
                    {
                        addOnPreset = existingPresets.FirstOrDefault(o => o.ID == data.PresetID);
                    }

                    if (addOnPreset != null)
                    {
                        addOnPreset.Labor = InputHelper.GetDouble(data.Labor);
                        addOnPreset.Refinish = InputHelper.GetDouble(data.Refinish);
                        addOnPreset.Charge = InputHelper.GetDouble(data.Charge);
                        addOnPreset.OtherCharge = InputHelper.GetDouble(data.OtherCharge);
                        addOnPreset.Active = data.Active;
                        addOnPreset.AutoSelect = data.AutoSelect;

                        if (data.OtherTypeOverride == "Shell" || data.OtherTypeOverride == addOnPreset.PresetShell.OtherType)
                        {
                            addOnPreset.OtherTypeOverride = "";
                        }
                        else
                        {
                            addOnPreset.OtherTypeOverride = data.OtherTypeOverride;
                        }

                        FunctionResult saveResult = _proAdvisorPresetRepository.Save(addOnPreset, activeLoginID);

                        if (!saveResult.Success)
                        {
                            builder.AppendLine(saveResult.ErrorMessage);
                        }
                    }
                }
                catch (Exception ex)
                {
                    builder.AppendLine(ex.Message);
                }
            }

            string errorMessage = builder.ToString();
            if (string.IsNullOrEmpty(errorMessage))
            {
                result.Success = true;
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = errorMessage;
            }

            return result;
        }
    }
}
