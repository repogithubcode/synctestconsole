using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Admin.ViewModel.EmailReport
{
    public class UnsubscribeEmailReportVM
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public bool Unsubscribed { get; set; }
        public string SalesRep { get; set; }
        public bool History { get; set; }

        public UnsubscribeEmailReportVM() { }

        public UnsubscribeEmailReportVM(int id, DateTime dt, string evt, string salesRep)
        {
            ID = id;
            TimeStamp = dt;
            Event = evt;
            SalesRep = salesRep;
        }

        public UnsubscribeEmailReportVM(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Email = InputHelper.GetString(row["Email"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = InputHelper.GetString(row["Event"].ToString());
            Unsubscribed = Event == "unsubscribe" ? true : false;
            History = UnsubscribeHistory.GetUnsubscribeHistory(ID).Count > 0 ? true : false;
        }

        public static List<UnsubscribeEmailReportVM> GetForFilter(string addressFilter, string statusFilter)
        {
            List<UnsubscribeEmailReportVM> unsubscribeEmails = new List<UnsubscribeEmailReportVM>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Email", addressFilter));
            parameters.Add(new SqlParameter("Event", statusFilter));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetUnsubscribeList", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    unsubscribeEmails.Add(new UnsubscribeEmailReportVM(row));
                }
            }
            
            return unsubscribeEmails;
        }
    }
}