using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class MiscTracking
    {

        public int ID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public int LoginID { get; private set; }
        public int EstimateID { get; private set; }
        public string OtherData { get; private set; }
        public string Tag { get; private set; }

        public MiscTracking(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            OtherData = InputHelper.GetString(row["OtherData"].ToString());
            Tag = InputHelper.GetString(row["Tag"].ToString());
        }

        public static List<MiscTracking> Get(int loginID, int estimateID, string tag)
        {
            List<MiscTracking> results = new List<MiscTracking>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("Tag", tag));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResults = db.ExecuteWithTable("MiscTracking_Get", parameters);

            if (tableResults.Success)
            {
                foreach (DataRow row in tableResults.DataTable.Rows)
                {
                    results.Add(new MiscTracking(row));
                }
            }

            return results;
        }


        public static void Insert(int loginID, int estimateID, string tag, string otherData = "")
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("EstimateID", estimateID));
            parameters.Add(new SqlParameter("OtherData", otherData));
            parameters.Add(new SqlParameter("Tag", tag));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("MiscTracking_Insert", parameters);
        }

    }
}
