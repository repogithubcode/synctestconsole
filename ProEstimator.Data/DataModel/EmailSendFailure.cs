using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class EmailSendFailure
    {

        public int ID { get; private set; }
        public int EmailID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Message { get; private set; }
	
        public EmailSendFailure(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            EmailID = InputHelper.GetInteger(row["EmailID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Message = InputHelper.GetString(row["Message"].ToString());
        }

        public static SaveResult Insert(int emailID, string message)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("EmailID", emailID));
            parameters.Add(new SqlParameter("Message", message));
                
            DBAccess db = new DBAccess();
            return new SaveResult(db.ExecuteNonQuery("EmailFailures_Insert", parameters));
        }

        public static List<EmailSendFailure> GetForEmail(int emailID)
        {
            List<EmailSendFailure> results = new List<EmailSendFailure>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("EmailFailures_GetForEmail", new SqlParameter("EmailID", emailID));

            foreach(DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(new EmailSendFailure(row));
            }

            return results;
        }

    }
}
