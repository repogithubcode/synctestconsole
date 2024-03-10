using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel
{
    public class SentEstimate
    {

        public int ID { get; private set; }
        public int AdminInfoID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string RecipientsList { get; private set; }
        public List<string> Recipients { get; private set; }
        public string Message { get; private set; }
        public int ReportID { get; private set; }
        public string ReportFileName { get; private set; }
        public string ReportType { get; private set; }

        public SentEstimate(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Message = row["Message"].ToString();
            ReportID = InputHelper.GetInteger(row["ReportID"].ToString());
            Recipients = new List<string>();
            ReportFileName = row["FileName"].ToString();
            ReportType = row["ReportType"].ToString();
            RecipientsList = row["Recipients"].ToString();

            string recipients = row["Recipients"].ToString();
            if (!string.IsNullOrEmpty(recipients))
            {
                string[] pieces = recipients.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                Recipients = pieces.ToList();
            }
        }

        public static int LogSentEstimate(int adminInfoID, List<string> recipients, string message, int reportID)
        {
            System.Text.StringBuilder recipientsBuilder = new System.Text.StringBuilder();
            foreach(string recipient in recipients)
            {
                recipientsBuilder.AppendLine(recipient + ";");
            }

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AdminInfoID", adminInfoID));
            parameters.Add(new SqlParameter("Recipients", recipientsBuilder.ToString()));
            parameters.Add(new SqlParameter("Message", message ?? string.Empty));  // if message is null then send empty as SP throws an
            parameters.Add(new SqlParameter("ReportID", reportID));

            DBAccess dbAccess = new DBAccess();
            DBAccessIntResult result = dbAccess.ExecuteWithIntReturn("InsertSentEstimateRecord", parameters);
            if (result.Success)
            {
                return result.Value;
            }

            return 0;
        }

        public static List<SentEstimate> GetForAdminInfoID(int adminInfoID)
        {
            List<SentEstimate> returnList = new List<SentEstimate>();

            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("GetSentEstimateRecords", new SqlParameter("AdminInfoID", adminInfoID));

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new SentEstimate(row));
                }
            }

            return returnList;
        }

        public static SentEstimate GetByID(int id)
        {
            DBAccess dbAccess = new DBAccess();
            DBAccessTableResult tableResult = dbAccess.ExecuteWithTable("GetSentEstimate", new SqlParameter("ID", id));

            if (tableResult.Success)
            {
                return new SentEstimate(tableResult.DataTable.Rows[0]);
            }

            return null;
        }
    }
}
