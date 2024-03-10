using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class LoginAutoRenew
    {

        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public bool Enabled { get; private set; }
        public DateTime TimeStamp { get; private set; }
	
        public LoginAutoRenew(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Enabled = InputHelper.GetBoolean(row["Enabled"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
        }

        public static void Insert(int loginID, bool isEnabled)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("Enabled", isEnabled));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("LoginsAutoRenew_Insert", parameters);
        }

        public static List<LoginAutoRenew> GetForLogin(int loginID)
        {
            List<LoginAutoRenew> results = new List<LoginAutoRenew>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("LoginsAutoRenew_GetForLogin", new SqlParameter("LoginID", loginID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(new LoginAutoRenew(row));
            }

            return results;
        }

        public static LoginAutoRenew GetLastForLogin(int loginID)
        {
            List<LoginAutoRenew> list = GetForLogin(loginID);
            if (list.Count > 0)
            {
                return list[0];
            }

            return null;
        }

    }
}
