using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ReminderLog
    {

        public static void Insert(int loginID, int linkedID, string tag)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("LinkedID", linkedID));
            parameters.Add(new SqlParameter("Tag", tag));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ReminderLog_Insert", parameters);
        }
    }
}
