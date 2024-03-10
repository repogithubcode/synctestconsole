using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class SendGridInfo : ProEstEntity
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public string Reason { get; set; }
        public int EmailID { get; set; }

        public SendGridInfo(string email, DateTime time, string evt, string reason, string emailID)
        {
            ID = 0;
            Email = InputHelper.GetString(email);
            TimeStamp = time;
            Event = InputHelper.GetString(evt);
            Reason = InputHelper.GetString(reason);
            EmailID = InputHelper.GetInteger(emailID);
        }

        public SendGridInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Email = row["Email"].ToString();
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = row["Event"].ToString();
            Reason = row["Reason"].ToString();
            EmailID = InputHelper.GetInteger(row["EmailID"].ToString());

            RowAsLoaded = row;
        }

        public static List<SendGridInfo> GetByEmailID(int emailID)
        {
            List<SendGridInfo> info = new List<SendGridInfo>();

            if (emailID > 0)
            {
                DBAccess db = new DBAccess();
                DBAccessTableResult result = db.ExecuteWithTable("GetSendGridInfoByEmailID", new SqlParameter("EmailID", emailID));

                if (result.Success)
                {
                    foreach (DataRow row in result.DataTable.Rows)
                    {
                        info.Add(new SendGridInfo(row));
                    }
                }
            }

            return info;
        }

        public static bool GetBounce(string email)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetBounce", new SqlParameter("Email", email));
            return result.Success;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Email", Email));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Event", Event));
            parameters.Add(new SqlParameter("Reason", Reason));
            parameters.Add(new SqlParameter("EmailID", EmailID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("SendGridInfo_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "SendGridInfo", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }
    }
}
