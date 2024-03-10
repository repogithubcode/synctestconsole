using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractAddOnTrial : ProEstEntity
    {
        public int ID { get; private set; }
        public int ContractID { get; set; }
        public ContractType AddOnType { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsDeleted { get; set; }

        public ContractAddOnTrial()
        {

        }

        public ContractAddOnTrial(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            AddOnType = ContractType.Get(InputHelper.GetInteger(row["AddOnType"].ToString()));
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString());
            EndDate = InputHelper.GetDateTime(row["EndDate"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("AddOnType", AddOnType.ID));
            parameters.Add(new SqlParameter("StartDate", StartDate));
            parameters.Add(new SqlParameter("EndDate", EndDate));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ContractAddOnTrial_Update", parameters);

            if (result.Success)
            {
                ID = result.Value;

                Contract contract = Contract.Get(ContractID);
                ChangeLogManager.LogChange(activeLoginID, "ContractAddOnTrial", ID, contract.LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static ContractAddOnTrial Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOnTrial_Get", new SqlParameter("ID", id));
            if (result.Success)
            {
                return new ContractAddOnTrial(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<ContractAddOnTrial> GetForContract(int contractID, bool showDeleted = false)
        {
            List<ContractAddOnTrial> addOns = new List<ContractAddOnTrial>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("contractID", contractID));
            parameters.Add(new SqlParameter("Deleted", showDeleted));

            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOnTrial_GetForContract", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    addOns.Add(new ContractAddOnTrial(row));
                }
            }

            return addOns;
        }

    }
}
