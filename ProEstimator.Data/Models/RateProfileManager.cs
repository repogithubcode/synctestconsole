using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Profiles;

namespace ProEstimatorData.Models
{
    /// <summary>
    /// This static class encapsulates operations regarding Rate Profiles.
    /// </summary>
    public static class RateProfileManager
    {
        public static int CopyProfile(int loginID, int profileID, int estimateID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ProfileToCopy", profileID));
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("AdminInfoID", estimateID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerProfile_Copy", parameters);

            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        public static int CreateBlankRateProfile(int loginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ProfileToCopy", 1));
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("NewProfileName", "New Rate Profile"));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerProfile_Copy", parameters);

            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        public static int CreateDefaultCopy(int loginID)
        {
            List<RateProfile> allProfiles = RateProfile.GetAllForLogin(loginID);
            RateProfile defaultProfile = allProfiles.FirstOrDefault(o => o.IsDefault);

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ProfileToCopy", defaultProfile.ID));
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("NewProfileName", "Enter new name"));
            parameters.Add(new SqlParameter("NewProfileDescription", "Enter a new description"));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("CustomerProfile_Copy", parameters);

            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }
        
        public static void DeleteProfile(int profileID)
        {
            RateProfile rateProfile = RateProfile.Get(profileID);
            rateProfile.IsDeleted = true;
            rateProfile.Save();
        }

        public static void CheckUseDefault(int loginID)
        {
            try
            {
                LoginInfo loginInfo = LoginInfo.GetByID(loginID);
                if (loginInfo.UseDefaultRateProfile)
                {
                    List<RateProfile> allProfiles = RateProfile.GetAllForLogin(loginID);
                    RateProfile defaultProfile = allProfiles.FirstOrDefault(o => o.IsDefault);
                    if (defaultProfile == null)
                    {
                        loginInfo.UseDefaultRateProfile = false;
                        loginInfo.Save();
                    }
                }
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, loginID, "Error checking use default profile.");
            }
        }
    }
}