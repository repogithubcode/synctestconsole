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
    public class MessageAttachmentMapping
    {
        public int ReportID { get; set; }
        public string ReportType { get; set; }
        public string FileName { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public int LoginID { get; set; }
        public int AdminInfoID { get; set; }
        public bool ImagesOnlyChecked { get; set; }

        public MessageAttachmentMapping(DataRow row)
        {
            ReportID = InputHelper.GetInteger(row["ReportID"].ToString());
            ReportType = row["ReportType"].ToString();
            FileName = row["FileName"].ToString();
            Subject = row["Subject"].ToString();
            Body = row["Body"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ImagesOnlyChecked = InputHelper.GetBoolean(row["ImagesOnlyChecked"].ToString());
        }

        public static List<MessageAttachmentMapping> GetByUniqueID(string uniqueID)
        {
            List<MessageAttachmentMapping> returnList = new List<MessageAttachmentMapping>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("UniqueID", uniqueID));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("GetSMSAttachmentMapping", parameters);
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new MessageAttachmentMapping(row));
                }
            }

            return returnList;
        }

        public static void Save(int? emailID, string uniqueID)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EmailID", emailID));
            parameters.Add(new SqlParameter("UniqueID", uniqueID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("AddSMSAttachmentMapping", parameters);
        }
    }
}
