using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class Unsubscribe : ProEstEntity
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public int EmailID { get; set; }

        public Unsubscribe()
        {
        }

        public Unsubscribe(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            Email = row["Email"].ToString();
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = row["Event"].ToString();
            EmailID = InputHelper.GetInteger(row["EmailID"].ToString());
        }

        public static List<Unsubscribe> GetUnsubscribeList(string email, string evt)
        {
            List<Unsubscribe> Unsubscribes = new List<Unsubscribe>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("Email", email));
            parameters.Add(new SqlParameter("Event", evt));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetUnsubscribeList", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    Unsubscribes.Add(new Unsubscribe(row));
                }
            }

            return Unsubscribes;
        }

        public static bool GetUnsubscribe(string email)
        {
            if (email != "")
            {
                List<Unsubscribe> info = GetUnsubscribeList(email, "");
                if (info.Count > 0 && info[0].Event == "unsubscribe")
                {
                    return true;
                }
            }
            return false;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("Email", Email));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Event", Event));
            parameters.Add(new SqlParameter("EmailID", EmailID));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("Unsubscribe_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "Unsubscribe", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }
    }
}
