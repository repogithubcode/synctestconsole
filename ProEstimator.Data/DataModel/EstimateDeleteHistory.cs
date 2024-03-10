using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class EstimateDeleteHistory
    {

        public int EstimateID { get; private set; }
        public DateTime DeleteStamp { get; private set; }
        public DateTime? RestoreStamp { get; private set; }

        public bool IsRestored
        {
            get
            {
                return RestoreStamp.HasValue;
            }
        }

        public EstimateDeleteHistory(DataRow row)
        {
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            DeleteStamp = InputHelper.GetDateTime(row["DeleteStamp"].ToString());

            string restoreStamp = row["RestoreStamp"].ToString();
            if (!string.IsNullOrEmpty(restoreStamp))
            {
                RestoreStamp = InputHelper.GetDateTime(restoreStamp);
            }
        }

        public static List<EstimateDeleteHistory> GetDeleteHistory(int estimateID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("EstimateDeleteHistory_Get", new SqlParameter("AdminInfoID", estimateID));

            List<EstimateDeleteHistory> returnList = new List<EstimateDeleteHistory>();

            foreach(DataRow row in result.DataTable.Rows)
            {
                returnList.Add(new EstimateDeleteHistory(row));
            }

            return returnList;
        }

        public static void LogDeletion(int estimateID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("EstimateDeleteHistory_LogDeletion", new SqlParameter("AdminInfoID", estimateID));
        }

        public static void LogRestore(int estimateID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("EstimateDeleteHistory_RestoreLast", new SqlParameter("AdminInfoID", estimateID));
        }
    }
}
