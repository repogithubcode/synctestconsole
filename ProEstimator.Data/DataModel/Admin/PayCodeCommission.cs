using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimatorData.DataModel.Admin
{
    public class PayCodeCommission
    {
        public int PayCodeCommissionID { get; set; }

        public bool HasEMS { get; set; }
        public bool HasFrame { get; set; }
        public bool HasWT { get; set; }

        public string ContractTermDescription { get; set; }
        public int PayCode { get; set; }
        public double Commission { get; set; }

        public bool HasMU { get; set; }
        public double MUCommission { get; set; }

        public bool HasQB { get; set; }
        public double QBCommission { get; set; }

        public string CodeListVsText { get; set; }
        public string WEText { get; set; }

        public string SalesType { get; set; }

        public PayCodeCommission(DataRow row)
        {
            PayCodeCommissionID = InputHelper.GetInteger(row["PayCodeCommissionID"].ToString());

            HasEMS = InputHelper.GetBoolean(row["HasEMS"].ToString());
            HasFrame = InputHelper.GetBoolean(row["HasFrame"].ToString());
            HasWT = InputHelper.GetBoolean(row["HasWT"].ToString());

            ContractTermDescription = InputHelper.GetString(row["ContractTermDescription"].ToString());
            PayCode = InputHelper.GetInteger(row["PayCode"].ToString());
            Commission = InputHelper.GetDouble(row["Commission"].ToString());

            HasMU = InputHelper.GetBoolean(row["HasMU"].ToString());
            MUCommission = InputHelper.GetDouble(row["MUCommission"].ToString());

            HasQB = InputHelper.GetBoolean(row["HasQB"].ToString());
            QBCommission = InputHelper.GetDouble(row["QBCommission"].ToString());

            CodeListVsText = InputHelper.GetString(row["CodeListVsText"].ToString());
            WEText = InputHelper.GetString(row["WEText"].ToString());
            SalesType = InputHelper.GetString(row["SalesType"].ToString());
        }

        public static List<PayCodeCommission> Get()
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("PayCodeCommission_GetAll");
            DataTable table = tableResult.DataTable;

            List <PayCodeCommission> PayCodeCommissions = new List<PayCodeCommission>();

            foreach (DataRow row in table.Rows)
            {
                PayCodeCommissions.Add(new PayCodeCommission(row));
            }

            return PayCodeCommissions;
        }
    }
}
