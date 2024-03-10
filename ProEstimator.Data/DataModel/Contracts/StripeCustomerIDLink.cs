using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel.Contracts
{
    public class StripeCustomerIDLink
    {
        public int LoginID { get; private set; }
        public string StripeCustomerID { get; private set; }
        public DateTime TimeStamp { get; private set; }

        public StripeCustomerIDLink() { }

        public StripeCustomerIDLink(DataRow row)
        {
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            StripeCustomerID = InputHelper.GetString(row["StripeCustomerID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
        }

        public static StripeCustomerIDLink Get(int loginID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("StripeCustomerIDs_Get", new SqlParameter("LoginID", loginID));
            if (result.Success)
            {
                return new StripeCustomerIDLink(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static void Insert(int loginID, string stripeCustomerID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StripeCustomerID", stripeCustomerID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("StripeCustomerIDs_Insert", parameters);
        }
    }
}
