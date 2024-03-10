using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel
{
    public class SentSmsStaus : ProEstEntity
    {
        public int ID { get; set; }
        public int ReportId { get; set; }
        public string Status { get; set; }
        public string ResponseResource { get; set; }
        public string SmsId { get; set; }
        public string ErrorMessage { get; set; }
        
        public SentSmsStaus()
        {

        }

        public SentSmsStaus(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ReportId = InputHelper.GetInteger(row["ReportId"].ToString());
            Status = row["Status"].ToString();
            ResponseResource = row["SMSResponseResource"].ToString();
            SmsId = row["SMSId"].ToString();
            ErrorMessage = Convert.ToString( row["ErrorMessage"]);
        }

        public static List<SentSmsStaus> GetAllSMS(int ReportId = 0, bool onlyPending = true)
        {
            List<SentSmsStaus> returnList = new List<SentSmsStaus>();
            DBAccess db = new DBAccess();
            //var query = "SELECT * from SentSMS_Status WHERE ( @ReportID = 0 or ReportId = @ReportID)" + (onlyPending ? " and status NOT IN('Delivered', 'Failed', 'ProcessError')" : "");
            var query = "SELECT * from SentSMS_Status WHERE ( @ReportID = 0 or ReportId = @ReportID)" + (onlyPending ? " and status = 'queued'" : "");
            var tableResult = db.ExecuteWithTableForQuery(query, new SqlParameter() { Value = ReportId, ParameterName = "@ReportId" });
            if (tableResult.Success)
            {
                foreach (DataRow row in tableResult.DataTable.Rows)
                {
                    returnList.Add(new SentSmsStaus(row));
                }
            }
            return returnList;
        }

        public static List<SentSmsStaus> GetForEstimate(int estimateID)
        {
            List<SentSmsStaus> returnList = new List<SentSmsStaus>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetSentSMSStatusForEstimate", new SqlParameter("EstimateID", estimateID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    returnList.Add(new SentSmsStaus(row));
                }
            }

            return returnList;
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ReportId", ReportId));
            parameters.Add(new SqlParameter("Status", GetString(Status)));
            parameters.Add(new SqlParameter("SMSResponseResource", GetString(ResponseResource)));
            parameters.Add(new SqlParameter("ErrorMessage", GetString(ErrorMessage)));
            parameters.Add(new SqlParameter("SMSId", GetString(SmsId)));
            parameters.Add(new SqlParameter("ID", ID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("AddOrUpdateSendSMSStatus", parameters);
            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result);
        } 
    }
}

