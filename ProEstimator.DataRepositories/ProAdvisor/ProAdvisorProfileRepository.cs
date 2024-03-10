using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel.ProAdvisor;

namespace ProEstimator.DataRepositories.ProAdvisor
{
    public class ProAdvisorProfileRepository : IProAdvisorProfileRepository
    {
        public List<ProAdvisorPresetProfile> GetAllProfilesForAccount(int loginID, bool showDeleted = false)
        {
            List<ProAdvisorPresetProfile> profiles = new List<ProAdvisorPresetProfile>();

            List<SqlParameter> paramters = new List<SqlParameter>();
            paramters.Add(new SqlParameter("LoginID", loginID));
            paramters.Add(new SqlParameter("Deleted", showDeleted));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPresetProfile_GetForLogin", paramters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                profiles.Add(InitializeProfile(row));
            }

            return profiles;
        }

        public ProAdvisorPresetProfile GetProfile(int profileID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("AddOnPresetProfile_Get", new SqlParameter("ID", profileID));

            if (tableResult.Success)
            {
                return InitializeProfile(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public FunctionResult SaveProfile(int activeLoginID, ProAdvisorPresetProfile profile)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", profile.ID));
            parameters.Add(new SqlParameter("LoginID", profile.LoginID));
            parameters.Add(new SqlParameter("Name", profile.Name));
            parameters.Add(new SqlParameter("DefaultFlag", profile.DefaultFlag));
            parameters.Add(new SqlParameter("CreationStamp", profile.CreationStamp));
            parameters.Add(new SqlParameter("Deleted", profile.Deleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOnPresetProfile_Save", parameters);

            if (result.Success)
            {
                ((IIDSetter)profile).ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "AddOnPresetProfile", profile.ID, profile.LoginID, parameters, profile.RowAsLoaded);
            }

            return new FunctionResult(result.ErrorMessage);
        }

        private ProAdvisorPresetProfile InitializeProfile(DataRow row)
        {
            ProAdvisorPresetProfile profile = new ProAdvisorPresetProfile();
            ((IIDSetter)profile).ID = InputHelper.GetInteger(row["ID"].ToString());
            profile.LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            profile.Name = InputHelper.GetString(row["Name"].ToString());
            profile.DefaultFlag = InputHelper.GetBoolean(row["DefaultFlag"].ToString());
            profile.CreationStamp = InputHelper.GetDateTime(row["CreationStamp"].ToString());
            profile.Deleted = InputHelper.GetBoolean(row["Deleted"].ToString());

            profile.RowAsLoaded = row;

            return profile;
        }
    }
}
