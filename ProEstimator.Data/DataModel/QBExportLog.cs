using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Configuration;

namespace ProEstimatorData.DataModel
{
    public class QBExportLog
    {

        public int ID { get; private set; }
        public int LoginID { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public DateTime? TimeStamp { get; set; }
        public Boolean? IsDeleted { get; set; }

        public QBExportLog()
        {

        }

        public QBExportLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString());
            EndDate = InputHelper.GetDateTime(row["EndDate"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
        }

        public string GetDiskPath()
        {
            return Path.Combine(ConfigurationManager.AppSettings.Get("UserContentPath").ToString(), LoginID.ToString(), "Reports", "QBExport" + ID.ToString() + ".xlsx");
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("StartDate", StartDate));
            parameters.Add(new SqlParameter("EndDate", EndDate));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult intResult = db.ExecuteWithIntReturn("QBExportLog_Update", parameters);
            if (intResult.Success)
            {
                ID = intResult.Value;
                return new SaveResult();
            }
            else
            {
                return new SaveResult(intResult.ErrorMessage);
            }
        }

        public static QBExportLog Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("QBExportLog_Get", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new QBExportLog(tableResult.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<QBExportLog> GetForLogin(int loginID, Boolean? showDeletedQBExportLog = null)
        {
            List<QBExportLog> results = new List<QBExportLog>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("IsDeleted", showDeletedQBExportLog));

            DBAccessTableResult tableResult = db.ExecuteWithTable("QBExportLog_GetByLogin", parameters);

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(new QBExportLog(row));
            }

            return results; 
        }

        public void Delete(int activeLoginID = 0)
        {
            IsDeleted = true;
            Save();
        }

        public void Restore(int activeLoginID = 0)
        {
            IsDeleted = false;
            Save();
        }
    }
}

