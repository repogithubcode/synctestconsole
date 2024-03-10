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
    public class PDR_RateProfileManager
    {



        #region Rate Profiles

        /// <summary>
        /// Create a copy of the passed PDR Rate profile ID and assign it to the passed login ID.  Return the new profile ID
        /// </summary>
        public PDR_RateProfileFunctionResult DuplicateRateProfile(int rateProfileID, int targetLoginID, string newProfileName = "")
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

            SaveResult saveResult = newProfile.Save();
            if (!saveResult.Success)
            {
                return new PDR_RateProfileFunctionResult(saveResult.ErrorMessage);
            }

            // The base rate profile copy was created, now copy the Rate records from the source to the new profile
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("SourceProfileID", sourceProfile.ID));
            parameters.Add(new SqlParameter("TargetProfileID", newProfile.ID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("PDR_Rate_CopyToNewProfile", parameters);

            return new PDR_RateProfileFunctionResult(newProfile);
        }

        /// <summary>
        /// Delete a rate profile.  If a LoginID is passed, the profile will only be deleted if it is assigned to the passed ID.
        /// </summary>
        public FunctionResult DeleteRateProfile(int profileID, int loginID = 0)
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

            profile.Delete();

            return new FunctionResult();
        }

        /// <summary>
        /// Make a copy of the rate profile marked as Default for the passed LoginID
        /// </summary>
        public PDR_RateProfileFunctionResult CopyFromDefault(int loginID)
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

            return DuplicateRateProfile(defaultProfile.ID, loginID);
        }

        public FunctionResult CopyPanel(int copyFromPanelID, int copyToPanelID, int rateProfileID)
        {
            PDR_RateProfile rateProfile = PDR_RateProfile.GetByID(rateProfileID);
            if (rateProfileID == null)
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
                    targetRate.Save();
                }
            }

            return new FunctionResult();
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