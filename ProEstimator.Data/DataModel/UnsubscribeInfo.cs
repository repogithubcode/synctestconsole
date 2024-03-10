using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class UnsubscribeInfo : ProEstEntity
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public int LoginID { get; set; }
        public int EmailID { get; set; }

        public UnsubscribeInfo(string email, DateTime time, string evt, string loginID, string emailID)
        {
            ID = 0;
            Email = InputHelper.GetString(email);
            TimeStamp = time;
            Event = InputHelper.GetString(evt);
            LoginID = InputHelper.GetInteger(loginID);
            EmailID = InputHelper.GetInteger(emailID);
        }

        public UnsubscribeInfo(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Email = row["Email"].ToString();
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = row["Event"].ToString();
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            EmailID = InputHelper.GetInteger(row["EmailID"].ToString());
        }

        public static bool GetUnsubscribe(string email)
        {
            List<UnsubscribeInfo> info = new List<UnsubscribeInfo>();

            if (email != "")
            {
                DBAccess db = new DBAccess();
                List<SqlParameter> parameters = new List<SqlParameter>();
                parameters.Add(new SqlParameter("Email", email));

                DBAccessTableResult result = db.ExecuteWithTable("GetUnsubscribe", parameters);

                if (result.Success)
                {
                    foreach (DataRow row in result.DataTable.Rows)
                    {
                        info.Add(new UnsubscribeInfo(row));
                    }
                    if(info.Count > 0 && info[0].Event == "unsubscribe")
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("LoginID", LoginID));
            parameters.Add(new SqlParameter("Email", Email));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Event", Event));
            parameters.Add(new SqlParameter("EmailID", EmailID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("UnsubscribeInfo_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "UnsubscribeInfo", ID, LoginID, parameters, RowAsLoaded);
                Unsubscribe unsubscribe = new Unsubscribe();
                List<Unsubscribe> Unsubscribes = Unsubscribe.GetUnsubscribeList(Email, "");
                if (Unsubscribes.Count > 0)
                {
                    unsubscribe = Unsubscribes[0];
                }
                
                unsubscribe.Email = Email;
                unsubscribe.TimeStamp = TimeStamp;
                unsubscribe.Event = Event;
                unsubscribe.EmailID = EmailID;
                
                unsubscribe.Save();
            }

            return new SaveResult(result);
        }
    }
}
