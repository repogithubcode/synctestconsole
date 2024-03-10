using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class UnsubscribeHistory : ProEstEntity
    {
        public int ID { get; set; }
        public int UnsubscribeID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Event { get; set; }
        public string SalesRep { get; set; }

        public UnsubscribeHistory(int unsubscribeID, string evt, string salesRep)
        {
            ID = 0;
            UnsubscribeID = unsubscribeID;
            TimeStamp = DateTime.Now;
            Event = InputHelper.GetString(evt);
            SalesRep = InputHelper.GetString(salesRep);
        }

        public UnsubscribeHistory(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            UnsubscribeID = InputHelper.GetInteger(row["UnsubscribeID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Event = row["Event"].ToString();
            SalesRep = row["SalesRep"].ToString();
        }

        public static List<UnsubscribeHistory> GetUnsubscribeHistory(int unsubscribeID)
        {
            List<UnsubscribeHistory> Unsubscribes = new List<UnsubscribeHistory>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("UnsubscribeID", unsubscribeID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("GetUnsubscribeHistory", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    Unsubscribes.Add(new UnsubscribeHistory(row));
                }
            }

            return Unsubscribes;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("UnsubscribeID", UnsubscribeID));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));
            parameters.Add(new SqlParameter("Event", Event));
            parameters.Add(new SqlParameter("SalesRep", SalesRep));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("UnsubscribeHistory_Save", parameters);
            if (result.Success)
            {
                ID = result.Value;

                ChangeLogManager.LogChange(activeLoginID, "UnsubscribeHistory", ID, 0, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }
    }
}
