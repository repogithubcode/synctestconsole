using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ChangeLogItem
    {

        public string FieldName { get; private set; }
        public string Before { get; private set; }
        public string After { get; private set; }

        public ChangeLogItem(DataRow row)
        {
            FieldName = InputHelper.GetString(row["FieldName"].ToString());
            Before = InputHelper.GetString(row["Before"].ToString());
            After = InputHelper.GetString(row["After"].ToString());
        }

        public static List<ChangeLogItem> GetForChangeLog(int changeLogID)
        {
            DBAccess db = new DBAccess(DatabaseName.ChangeLog);

            DBAccessTableResult tableResult = db.ExecuteWithTable("ChangeLogItem_GetForLog", new SqlParameter("ChangeLogID", changeLogID));

            List<ChangeLogItem> returnList = new List<ChangeLogItem>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                returnList.Add(new ChangeLogItem(row));
            }

            return returnList;
        }
    }
}
