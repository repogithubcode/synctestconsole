using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractEndDateChangeLog
    {
        public int ID { get; set; }
        public int ContractID { get; set; }
        public int TrialID { get; set; }
        public DateTime OldDate { get; set; }
        public DateTime NewDate { get; set; }
        public int SalesRepID { get; set; }
        public DateTime TimeStamp { get; set; }

        public ContractEndDateChangeLog()
        {
            TimeStamp = DateTime.Now;
        }

        public void Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("TrialID", TrialID));
            parameters.Add(new SqlParameter("OldDate", OldDate.Date));
            parameters.Add(new SqlParameter("NewDate", NewDate.Date));
            parameters.Add(new SqlParameter("SalesRepID", SalesRepID));
            parameters.Add(new SqlParameter("TimeStamp", TimeStamp));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ContractEndDateChangeLog_Insert", parameters);
        }
    }
}
