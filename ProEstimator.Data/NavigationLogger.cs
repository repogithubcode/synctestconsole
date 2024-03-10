using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel;

namespace ProEstimatorData
{
    public static class NavigationLogger
    {

        /// <summary>
        /// Log Navigation events to the database.
        /// </summary>
        public static void LogNavigation(int userID, string controlButtonID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("UserID", userID));
            parameters.Add(new SqlParameter("ControlButton", controlButtonID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("Navigation_Log", parameters);
        }

    }
}