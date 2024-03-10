using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class ChangeLogItemDetails
    {

        public DateTime TimeStamp { get; set; }
        public string Name { get; set; }
        public string EmailAddress { get; set; }
        public string Browser { get; set; }
        public string DeviceType { get; set; }
        public string ComputerKey { get; set; }

        public ChangeLogItemDetails()
        {

        }

        public ChangeLogItemDetails(DataRow row)
        {
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Name = InputHelper.GetString(row["Name"].ToString());
            EmailAddress = InputHelper.GetString(row["EmailAddress"].ToString());
            Browser = InputHelper.GetString(row["Browser"].ToString());
            DeviceType = InputHelper.GetString(row["DeviceType"].ToString());
            ComputerKey = InputHelper.GetString(row["ComputerKey"].ToString());
        }

        public static ChangeLogItemDetails GetForID(int changeLogID)
        {
            DBAccess db = new DBAccess(DatabaseName.ChangeLog);

            DBAccessTableResult tableResult = db.ExecuteWithTable("ChangeLog_GetDetails", new SqlParameter("ChangeLogID", changeLogID));

            if (tableResult.Success)
            {
                return new ChangeLogItemDetails(tableResult.DataTable.Rows[0]);
            }
            else
            {
                return new ChangeLogItemDetails();
            }
        }

    }
}
