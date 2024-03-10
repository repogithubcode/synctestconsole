using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.PDR;

namespace ProEstimator.Business.Logic
{
    public class PDR_Manager
    {

        #region Rate Profiles

        /// <summary>
        /// Create a copy of the passed PDR Rate profile ID and assign it to the passed login ID.  Return the new profile ID
        /// </summary>
        public PDR_RateProfileFunctionResult DuplicateRateProfile(int rateProfileID, int targetLoginID, int activeLoginID, string newProfileName = "")
        {
            // Cancel if we can't get the source profile
            PDR_RateProfile sourceProfile = PDR_RateProfile.GetByID(rateProfileID);
            if (sourceProfile == null)
            {
                return new PDR_RateProfileFunctionResult("Source PDR rate profile not found.");
            }

            // Create a copy of the source profile
            PDR_RateProfile newProfile = new PDR_RateProfile();
            newProfile.GridType = sourceProfile.GridType;
            newProfile.HideDentCounts = sourceProfile.HideDentCounts;
            newProfile.IsDefault = false;
            newProfile.LoginID = targetLoginID;
            newProfile.ProfileName = string.IsNullOrEmpty(newProfileName) ? (sourceProfile.ProfileName + " copy") : newProfileName;
            newProfile.ProfileType = sourceProfile.ProfileType;
            newProfile.TieredOversizedDents = sourceProfile.TieredOversizedDents;
            newProfile.OriginalID = rateProfileID;
            newProfile.Taxable = sourceProfile.Taxable;

            SaveResult saveResult = newProfile.Save(activeLoginID);
            if (!saveResult.Success)
            {
                return new PDR_RateProfileFunctionResult(saveResult.ErrorMessage);
            }

            // The base rate profile copy was created, now copy the Rate records from the source to the new profile
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SourceProfileID", sourceProfile.ID));
            parameters.Add(new SqlParameter("TargetProfileID", newProfile.ID));

            DBAccess db = new DBAccess();
            FunctionResult functionResult = db.ExecuteNonQuery("PDR_Rate_CopyToNewProfile", parameters);

            if (!functionResult.Success)
            {
                return new PDR_RateProfileFunctionResult(functionResult.ErrorMessage);
            }

            return new PDR_RateProfileFunctionResult(newProfile);
        }

        /// <summary>
        /// Delete a rate profile.  If a LoginID is passed, the profile will only be deleted if it is assigned to the passed ID.
        /// </summary>
        public FunctionResult DeleteRateProfile(int profileID, int loginID, int activeLoginID)
        {
            PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);
            if (profile == null)
            {
                return new FunctionResult("Rate profile not found.");
            }

            if (loginID > 0 && profile.LoginID != loginID)
            {
                return new FunctionResult("Permission denied.");
            }

            profile.Delete(activeLoginID);

            return new FunctionResult();
        }

        /// <summary>
        /// Restore a rate profile.  If a LoginID is passed, the profile will only be restored if it is assigned to the passed ID.
        /// </summary>
        public FunctionResult RestoreRateProfile(int profileID, int loginID, int activeLoginID)
        {
            PDR_RateProfile profile = PDR_RateProfile.GetByID(profileID);
            if (profile == null)
            {
                return new FunctionResult("Rate profile not found.");
            }

            if (loginID > 0 && profile.LoginID != loginID)
            {
                return new FunctionResult("Permission denied.");
            }

            profile.Deleted = false;
            profile.Save(activeLoginID);

            return new FunctionResult();
        }

        /// <summary>
        /// Make a copy of the rate profile marked as Default for the passed LoginID
        /// </summary>
        public PDR_RateProfileFunctionResult CopyFromDefault(int loginID, int activeLoginID)
        {
            List<PDR_RateProfile> profiles = PDR_RateProfile.GetByLogin(loginID);
            if (profiles == null || profiles.Count == 0)
            {
                return new PDR_RateProfileFunctionResult("No rate profiles found for user.");
            }

            PDR_RateProfile defaultProfile = profiles.FirstOrDefault(o => o.IsDefault);
            if (defaultProfile == null)
            {
                return new PDR_RateProfileFunctionResult("No default profile found for user.");
            }

            return DuplicateRateProfile(defaultProfile.ID, loginID, activeLoginID);
        }

        public FunctionResult CopyPanel(int copyFromPanelID, int copyToPanelID, int rateProfileID, int activeLoginID)
        {
            PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(rateProfileID);
            if (rateProfile == null)
            {
                return new FunctionResult("Rate profile not found.");
            }

            List<PDR_Rate> allRates = rateProfile.GetAllRates();

            List<PDR_Rate> copyFromRates = allRates.Where(o => o.Panel != null && o.Panel.ID == copyFromPanelID).ToList();
            List<PDR_Rate> copyToRates = allRates.Where(o => o.Panel != null && o.Panel.ID == copyToPanelID).ToList();

            foreach(PDR_Rate sourceRate in copyFromRates)
            {
                PDR_Rate targetRate = null;
                
                if (sourceRate.Size == PDR_Size.Oversized || (int)sourceRate.Size < 6)
                {
                    targetRate = copyToRates.FirstOrDefault(o => o.Size == sourceRate.Size && o.Depth == sourceRate.Depth);
                }
                else
                {
                    targetRate = copyToRates.FirstOrDefault(o => o.Size == sourceRate.Size && o.Quantity.ID == sourceRate.Quantity.ID);
                }
                
                if (targetRate != null && targetRate.Amount != sourceRate.Amount)
                {
                    targetRate.Amount = sourceRate.Amount;
                    targetRate.Save(activeLoginID, rateProfile.LoginID);
                }
            }

            return new FunctionResult();
        }

        public void MakeSureDefaultExists(int loginID, int activeLoginID)
        {
            // If there are no rate profiles create a default one.  This will happen the first time the user uses PDR
            List<PDR_RateProfile> profiles = PDR_RateProfile.GetByLogin(loginID).Where(o => o.AdminInfoID == 0).ToList();

            if (profiles == null || profiles.Count == 0)
            {
                PDR_RateProfileFunctionResult result = DuplicateRateProfile(1, loginID, activeLoginID, "Rate Profile");
                result.RateProfile.IsDefault = true;
                result.RateProfile.Save(activeLoginID);
                profiles.Add(result.RateProfile);

                // This happens when the user sets up PDR, make a copy of the global Description Presets for this account
                List<PDR_DescriptionPreset> globalPresets = PDR_DescriptionPreset.GetAllByLoginID(1);
                foreach(PDR_DescriptionPreset preset in globalPresets)
                {
                    PDR_DescriptionPreset presetCopy = new PDR_DescriptionPreset();
                    presetCopy.LoginID = loginID;
                    presetCopy.Text = preset.Text;
                    presetCopy.Save(activeLoginID);
                }
            }        
        }

        #endregion

    }

    public class PDR_RateProfileFunctionResult : FunctionResult
    {
        public PDR_RateProfile RateProfile { get; private set; }

        public PDR_RateProfileFunctionResult(PDR_RateProfile newRateProfile)
            : base()
        {
            RateProfile = newRateProfile;
        }

        public PDR_RateProfileFunctionResult(string errorMessage)
            : base(errorMessage)
        {

        }
    }
}