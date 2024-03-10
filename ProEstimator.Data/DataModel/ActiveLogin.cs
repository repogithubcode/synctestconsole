using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimatorData.DataModel
{
    public enum ActiveLoginDeleteKey
    {
        Active = 0,
        LoggedOut = 1,
        MultiLoggedOut = 2,
        KickedOut = 3,
        TimeOut = 4
    }

    public enum DeviceType
    {
        Desktop,
        Tablet,
        Mobile
    }

    public class ActiveLogin
    {
        // These are saved in the database
        public int ID { get; private set; }
        public int SiteUserID { get; set; }
        public int SalesRepID { get; set; }
        public int LoginID { get; set; }
        public string ComputerKey { get; set; }
        public string IPAddress { get; set; }
        public string ComputerName { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime LastActivity { get; set; }
        public bool IsImpersonated { get; set; }
        public ActiveLoginDeleteKey DeleteKey { get; set; }
        public DeviceType DeviceType { get; set; }
        public string Browser { get; set; }

        public int NoOfLogins { get; set; }

        public bool AdminIsImpersonating { get; set; }

        public bool AutoSaveTurnedOnTechSupport { get; set; } = true;

        public bool AutoSaveTurnedOnSiteUser { get; set; } = true;

        public ActiveLogin()
        {
            DeviceType = DeviceType.Desktop;
        }

        public ActiveLogin(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            SiteUserID = InputHelper.GetInteger(row["SiteUserID"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            ComputerKey = InputHelper.GetString(row["ComputerKey"].ToString());
            IPAddress = InputHelper.GetString(row["IPAddress"].ToString());
            ComputerName = InputHelper.GetString(row["ComputerName"].ToString());
            StartTime = InputHelper.GetDateTime(row["StartTime"].ToString());
            LastActivity = InputHelper.GetDateTime(row["LastActivity"].ToString());
            IsImpersonated = InputHelper.GetBoolean(row["IsImpersonated"].ToString());
            DeleteKey = (ActiveLoginDeleteKey)Enum.Parse(typeof(ActiveLoginDeleteKey), row["DeleteKey"].ToString());

            string browserType = InputHelper.GetString(row["DeviceType"].ToString());
            if (browserType == "M")
            {
                DeviceType = DeviceType.Mobile;
            }
            else if (browserType == "T")
            {
                DeviceType = DeviceType.Tablet;
            }
            else
            {
                DeviceType = DeviceType.Desktop;
            }

            Browser = InputHelper.GetString(row["Browser"].ToString());
        }

        public SaveResult MarkActivity()
        {
            LastActivity = DateTime.Now;
            return Save();
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("SiteUserID", SiteUserID));
            parameters.Add(new SqlParameter("SalesRepID", SalesRepID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("ComputerKey", ComputerKey == null ? "" : ComputerKey));
            parameters.Add(new SqlParameter("IPAddress", IPAddress));
            parameters.Add(new SqlParameter("ComputerName", ComputerName));
            parameters.Add(new SqlParameter("StartTime", StartTime));
            parameters.Add(new SqlParameter("LastActivity", LastActivity));
            parameters.Add(new SqlParameter("IsImpersonated", IsImpersonated));
            parameters.Add(new SqlParameter("DeleteKey", DeleteKey));
            parameters.Add(new SqlParameter("DeviceType", DeviceType.ToString().Substring(0, 1)));
            parameters.Add(new SqlParameter("Browser", Browser));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ActiveLogin_Update", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }

        public void Delete(ActiveLoginDeleteKey deleteReason)
        {
            DeleteKey = deleteReason;
            Save();
        }


        public static ActiveLogin Get(int activeLoginID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ActiveLogin_Get", new SqlParameter("ID", activeLoginID));
            if (result.Success)
            {
                return new ActiveLogin(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<ActiveLogin> GetUserLoginActivity(int loginID, int userID)
        {
            List<ActiveLogin> logList = new List<ActiveLogin>();

            if (loginID > 0 || userID > 0)
            {
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("IncludeImpersonated", false));
                parameters.Add(new SqlParameter("LoginID", loginID));
                parameters.Add(new SqlParameter("UserID", userID));

                DBAccess db = new DBAccess();
                DBAccessTableResult table = db.ExecuteWithTable("ActiveLogin_GetActivities", parameters);
                if (table.Success)
                {
                    foreach (DataRow row in table.DataTable.Rows)
                    {
                        logList.Add(new ActiveLogin(row));
                    }
                }
            }
           
            return logList;
        }

        public static ActiveLogin GetLastLoginActivity(int loginID)
        {
            List<ActiveLogin> tempList = GetUserLoginActivity(loginID, 0);
            if (tempList.Count > 0)
            {
                return tempList[0];
            }
            return null;
        }
       
        public static ActiveLogin GetLastLoginActivityByUser(int userID)
        {
            List<ActiveLogin> tempList = GetUserLoginActivity(0, userID);
            if (tempList.Count > 0)
            {
                return tempList[0];
            }
            return null;
        }

    }
}
