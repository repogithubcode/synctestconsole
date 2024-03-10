using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class WebEstAccountInfo
    {
        public int LoginID { get; private set; }
        public string LoginName { get; private set; }
        public string Organization { get; private set; }
        public int SalesRepID { get; private set; }
        public string SalesRepFirstName { get; private set; }
        public string SalesRepLastName { get; private set; }

        public WebEstAccountInfo()
        {

        }

        public WebEstAccountInfo(DataRow row)
        {
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            LoginName = InputHelper.GetString(row["LoginName"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            SalesRepFirstName = InputHelper.GetString(row["SalesRep_FirstName"].ToString());
            SalesRepLastName = InputHelper.GetString(row["SalesRep_LastName"].ToString());
        }

        public static WebEstAccountInfo GetForFilter(int loginID)
        {
            WebEstAccountInfo webEstAccountInfo = null;
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));

            DBAccessTableResult tableResult = db.ExecuteWithTable("GetWebEstLoginInfo", parameters);

            if(tableResult.DataTable.Rows.Count == 1)
            {
                webEstAccountInfo = new WebEstAccountInfo(tableResult.DataTable.Rows[0]);
            }

            return webEstAccountInfo;
        }
    }
}