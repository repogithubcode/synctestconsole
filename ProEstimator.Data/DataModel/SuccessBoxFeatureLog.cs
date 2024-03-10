using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class SuccessBoxFeatureLog
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public DateTime TimeStamp { get; private set; }
        public SuccessBoxModule Module { get; set; }
        public string Feature { get; set; }
        public bool IsImpersonated { get; set; }

        public SuccessBoxFeatureLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Module = (SuccessBoxModule)InputHelper.GetInteger(row["ModuleID"].ToString());
            Feature = row["Feature"].ToString();
            IsImpersonated = InputHelper.GetBoolean(row["IsImpersonated"].ToString());
        }

        /// <summary>
        /// Mark in our database that this feature log has been synced to Success Box
        /// </summary>
        public void LogSync()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("SuccessBox_LogFeatureSynced", new SqlParameter("ID", ID));
        }

        public static void LogFeature(int loginID, SuccessBoxModule module, string feature, int activeLoginID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("ModuleID", (int)module));
            parameters.Add(new SqlParameter("Feature", feature));
            parameters.Add(new SqlParameter("ActiveLoginID", activeLoginID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("SuccessBox_LogFeature", parameters);
        }

        public static List<SuccessBoxFeatureLog> GetUnsyncedFeatureLogs()
        {
            List<SuccessBoxFeatureLog> logs = new List<SuccessBoxFeatureLog>();

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("SuccessBox_GetUnsynced");
            foreach(DataRow row in table.DataTable.Rows)
            {
                logs.Add(new SuccessBoxFeatureLog(row));
            }

            return logs;
        }

        /// <summary>
        /// Call this after the feature log has been synced to Success Box
        /// </summary>
        public void LogSyncedTime()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("SuccessBox_LogFeatureSynced", new SqlParameter("ID", ID));
        }

        public static List<SuccessBoxFeatureLog> GetFeatureLogs(int loginID, string feature, string sDate, string eDate, bool incImperson)
        {
            List<SuccessBoxFeatureLog> logList = new List<SuccessBoxFeatureLog>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Feature", feature));
            parameters.Add(new SqlParameter("StartDate", sDate));
            parameters.Add(new SqlParameter("EndDate", eDate));
            parameters.Add(new SqlParameter("IncludeImpersonate", incImperson));

            DBAccess db = new DBAccess();
            DBAccessTableResult table = db.ExecuteWithTable("SuccessBoxFeatureLog_GetByFeature", parameters);
            if (table.Success)
            {
                foreach (DataRow row in table.DataTable.Rows)
                {
                    logList.Add(new SuccessBoxFeatureLog(row));
                }
            }
            return logList;
        }

        public static List<FeatureGroup> GetCountByFeatureForLogin(int loginID, string sDate, string eDate, bool incImperson)
        {
            List<FeatureGroup> featureList = new List<FeatureGroup>();
            List<SuccessBoxFeatureLog> tempList = GetFeatureLogs(loginID, "", sDate, eDate, incImperson);
            if (tempList.Count > 0)
            {
                var featureGroups = tempList.GroupBy(col => col.Feature).OrderBy(o => o.Key);

                foreach (var group in featureGroups)
                {
                    featureList.Add(new FeatureGroup(group.Key, group.Count()));
                }
            }

            return featureList;
        }

    }

    public class FeatureGroup
    {
        public string FeatureTag { get; private set; }
        public int Count { get; private set; }

        public FeatureGroup(string feature, int cnt)
        {
            FeatureTag = feature;
            Count = cnt;
        }
    }

    public enum SuccessBoxModule
    {
          EstimateWriting = 1
        , RateProfile = 2
        , Report = 3
        , Search = 4
    }
}
