using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SiteChangeLog
    {

        public int ID { get; private set; }
        public DateTime Date { get; set; }
        public string ShortNote { get; set; }
        public string LongNote { get; set; }
        public bool IsActive { get; set; }

        public SiteChangeLog()
        {
            Date = DateTime.Now.Date;
            ShortNote = "";
            LongNote = "";
            IsActive = false;
        }

        public SiteChangeLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Date = InputHelper.GetDateTime(row["Date"].ToString()).Date;
            ShortNote = InputHelper.GetString(row["ShortNote"].ToString());
            LongNote = InputHelper.GetString(row["LongNote"].ToString());
            IsActive = InputHelper.GetBoolean(row["IsActive"].ToString());
        }

        public FunctionResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Date", Date.Date));
            parameters.Add(new SqlParameter("ShortNote", ShortNote));
            parameters.Add(new SqlParameter("LongNote", LongNote));
            parameters.Add(new SqlParameter("IsActive", IsActive));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("SiteChangeLog_Update", parameters);
            if (intResult.Success)
            {
                ID = intResult.Value;
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(intResult.ErrorMessage);
            }
        }

        public FunctionResult Delete()
        {
            DBAccess db = new DBAccess();
            return db.ExecuteNonQuery("SiteChangeLog_Delete", new SqlParameter("ID", ID));
        }

        public static SiteChangeLog Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteChangeLog_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new SiteChangeLog(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<SiteChangeLog> GetAll()
        {
            List<SiteChangeLog> list = new List<SiteChangeLog>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("SiteChangeLog_GetAll");

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    list.Add(new SiteChangeLog(row));
                }
            }

            return list;
        }

        public static void LogHasSeen(int loginID)
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("SiteChangeLogSeen_Log", new SqlParameter("LoginID", loginID));
        }

        public static bool HasAccountSeenChanges(int loginID)
        {
            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("SiteChangeLogSeen_ForLogin", new SqlParameter("LoginID", loginID));
            if (intResult.Success && intResult.Value > 0)
            {
                return true;
            }

            return false;
        }

        public static void ClearAccountSeenTable()
        {
            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("SiteChangeLogSeen_Clear");
        }

    }
}
