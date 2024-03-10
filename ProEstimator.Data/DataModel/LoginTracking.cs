using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class LoginTracking
    {

        public int ID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string IPAddress { get; private set; }
        public int LoginID { get; private set; }
        public string BrowserType { get; private set; }
        public string BrowserName { get; private set; }
        public string BrowserVersion	{ get; private set; }
        public string BrowserPlatform	{ get; private set; }
        public bool IsMobile { get; private set; }
        public string UserAgent { get; private set; }

        public LoginTracking()
        {
            ID = 0;
            TimeStamp = DateTime.Now;
            IPAddress = "";
            LoginID = 0;
            BrowserType = "";
            BrowserName = "";
            BrowserVersion = "";
            BrowserPlatform = "";
            IsMobile = false;
            UserAgent = "";
        }

        public static void LogTracking(int loginID, string ipAddress, string browserType, string browser, string browserVersion, string platform, bool isMobile, string userAgent)
        {
            LoginTracking tracking = new LoginTracking();
            tracking.IPAddress = ipAddress;
            tracking.LoginID = loginID;
            tracking.BrowserType = browserType;
            tracking.BrowserName = browser;
            tracking.BrowserVersion = browserVersion;
            tracking.BrowserPlatform = platform;
            tracking.IsMobile = isMobile;
            tracking.UserAgent = userAgent;
            tracking.Save();
        }

        public LoginTracking(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            IPAddress = row["IPAddress"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            BrowserType = row["BrowserType"].ToString();
            BrowserName = row["BrowserName"].ToString();
            BrowserVersion = row["IPAddress"].ToString();
            BrowserPlatform = row["BrowserPlatform"].ToString();
            IsMobile = InputHelper.GetBoolean(row["IsMobile"].ToString());
            UserAgent = row["UserAgent"].ToString();
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("IPAddress", IPAddress));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("BrowserType", BrowserType));
            parameters.Add(new SqlParameter("BrowserName", BrowserName));
            parameters.Add(new SqlParameter("BrowserVersion", BrowserVersion));
            parameters.Add(new SqlParameter("BrowserPlatform", BrowserPlatform));
            parameters.Add(new SqlParameter("IsMobile", IsMobile));
            parameters.Add(new SqlParameter("UserAgent", UserAgent));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateLoginTracking", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        }        

        public static List<LoginTracking> GetBySearch(int? id, DateTime? dateRangeStart, DateTime? dateRangeEnd, int? loginID, bool? isMobile)
        {
            List<LoginTracking> loginTrackingRecords = new List<LoginTracking>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", id));
            parameters.Add(new SqlParameter("StartDate", dateRangeStart));
            parameters.Add(new SqlParameter("EndDate", dateRangeEnd));
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("IsMobile", isMobile));

            DBAccessTableResult result = new DBAccessTableResult();
            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    loginTrackingRecords.Add(new LoginTracking(row));
                }
            }

            return loginTrackingRecords;
        }

    }
}
