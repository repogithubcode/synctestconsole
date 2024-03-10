using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ChangeLogRowSummary
    {

        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string ItemType { get; private set; }
        public int ItemID { get; private set; }
        public string UserEmailAddress { get; private set; }
        public int ChangeCount { get; private set; }
        public string Name { get; private set; }

        public ChangeLogRowSummary(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            ItemType = InputHelper.GetString(row["ItemType"].ToString());
            ItemID = InputHelper.GetInteger(row["ItemID"].ToString());
            UserEmailAddress = InputHelper.GetString(row["UserEmailAddress"].ToString());
            ChangeCount = InputHelper.GetInteger(row["ChangeCount"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
        }

        public static List<ChangeLogRowSummary> GetForSearch(int loginID, DateTime timeSpanStart, DateTime timeSpanEnd, string itemType, int itemID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TimeSpanStart", timeSpanStart));
            parameters.Add(new SqlParameter("TimeSpanEnd", timeSpanEnd));
            parameters.Add(new SqlParameter("ItemType", itemType));
            parameters.Add(new SqlParameter("ItemID", itemID));

            DBAccess db = new DBAccess(DatabaseName.ChangeLog);

            DBAccessTableResult tableResult = db.ExecuteWithTable("ChangeLogSummary_Search", parameters);

            List<ChangeLogRowSummary> returnList = new List<ChangeLogRowSummary>();

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                returnList.Add(new ChangeLogRowSummary(row));
            }

            return returnList;
        }

        public static List<ChangeLogRowSummary> GetByItem(string itemType, int itemID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ItemType", itemType));
            parameters.Add(new SqlParameter("ItemID", itemID));

            DBAccess db = new DBAccess(DatabaseName.ChangeLog);

            DBAccessTableResult tableResult = db.ExecuteWithTable("ChangeLog_ByItem", parameters);

            List<ChangeLogRowSummary> returnList = new List<ChangeLogRowSummary>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                returnList.Add(new ChangeLogRowSummary(row));
            }

            return returnList;
        }

    }
}
