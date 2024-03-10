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
    public class ContractPayCodeCommissionMapping
    {
        public int ID { get; set; }
        public int ContractID { get; set; }
        public int PreviousCycleContractID { get; set; }
        public int PayCode { get; set; }
        public double Commission { get; set; }
        public double MUCommission { get; set; }
        public double QBCommission { get; set; }
        public DateTime? WalkThrough { get; set; }
        public Boolean PaymentExtension { get; set; }

        public ContractPayCodeCommissionMapping()
        {

        }

        public ContractPayCodeCommissionMapping(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            PreviousCycleContractID = InputHelper.GetInteger(row["PreviousCycleContractID"].ToString());
            PayCode = InputHelper.GetInteger(row["PayCode"].ToString());
            Commission = InputHelper.GetDouble(row["Commission"].ToString());
            WalkThrough = InputHelper.GetDateTime(row["WalkThrough"].ToString());
            MUCommission = InputHelper.GetDouble(row["MUCommission"].ToString());
            QBCommission = InputHelper.GetDouble(row["QBCommission"].ToString());
            PaymentExtension = InputHelper.GetBoolean(row["PaymentExtension"].ToString());
        }

        public SaveResult Save()
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("PreviousCycleContractID", PreviousCycleContractID));
            parameters.Add(new SqlParameter("PayCode", PayCode));
            parameters.Add(new SqlParameter("Commission", Commission));
            parameters.Add(new SqlParameter("MUCommission", MUCommission));
            parameters.Add(new SqlParameter("QBCommission", QBCommission));
            parameters.Add(new SqlParameter("WalkThrough", WalkThrough));
            parameters.Add(new SqlParameter("PaymentExtension", PaymentExtension));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ContractPayCodeCommissionMapping_Save", parameters);

            if (result.Success)
            {
                ID = result.Value;
            }

            return new SaveResult(result.ErrorMessage);
        }
    }
}
