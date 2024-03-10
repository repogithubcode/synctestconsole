using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace ProEstimatorData.DataModel
{

    /// <summary>
    /// An Error is a log in the Error table in the database.  These instances are read only, to add a new error use the ErrorLogger class.
    /// </summary>
    public class Error
    {

        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public int EstimateID { get; private set; }
        public string ErrorText { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Tag { get; private set; }

        public Error(DataRow row)
        {
            ID = InputHelper.GetInteger(row["id"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            EstimateID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            ErrorText = row["ErrorText"].ToString();
            TimeStamp = InputHelper.GetDateTime(row["TimeOccurred"].ToString());
            Tag = row["FixNote"] == null ? "" : row["FixNote"].ToString();
        }
    }
}
