using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractAddOn : ProEstEntity
    {
        public int ID { get; private set; }
        public int ContractID { get; set; }
        public ContractPriceLevel PriceLevel { get; set; }
        public ContractType AddOnType { get; set; }
        public DateTime StartDate { get; set; }
        public bool Active { get; set; }
        public bool IsDeleted { get; set; }
        public int Quantity { get; set; }

        /// <summary>
        /// This is not a database field, this is calculated when data is loaded.  If any Invoices attached to this contract are paid, this will be True.
        /// </summary>
        public bool HasPayment { get; private set; }

        public ContractAddOn()
        {

        }

        public ContractAddOn(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
            PriceLevel = ContractPriceLevel.Get(InputHelper.GetInteger(row["ContractPriceLevelID"].ToString()));
            AddOnType = ContractType.Get(InputHelper.GetInteger(row["AddOnType"].ToString()));
            StartDate = InputHelper.GetDateTime(row["StartDate"].ToString());
            Active = InputHelper.GetBoolean(row["Active"].ToString());
            IsDeleted = InputHelper.GetBoolean(row["IsDeleted"].ToString());
            Quantity = InputHelper.GetInteger(row["Quantity"].ToString(), 1);

            try
            {
                HasPayment = InputHelper.GetBoolean(row["HasPayment"].ToString());
            }
            catch { }

            // Kind of hacky.  For an upgrade we needed a price level with no payment.  In this case, permissions should act like a payment has been made.
            if (PriceLevel.PaymentAmount == 0)
            {
                HasPayment = true;
            }

            RowAsLoaded = row;
        }

        public override SaveResult Save(int activeLoginID = 0, int loginID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("ID", ID));
            parameters.Add(new SqlParameter("ContractID", ContractID));
            parameters.Add(new SqlParameter("ContractPriceLevelID", PriceLevel.ID));
            parameters.Add(new SqlParameter("AddOnType", AddOnType.ID));
            parameters.Add(new SqlParameter("StartDate", StartDate));
            parameters.Add(new SqlParameter("Active", Active));
            parameters.Add(new SqlParameter("IsDeleted", IsDeleted));
            parameters.Add(new SqlParameter("Quantity", Quantity));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("ContractAddOn_Update", parameters);

            if (result.Success)
            {
                ID = result.Value;

                Contract contract = Contract.Get(ContractID);
                ChangeLogManager.LogChange(activeLoginID, "ContractAddOn", ID, contract.LoginID, parameters, RowAsLoaded);
            }

            return new SaveResult(result);
        }

        public static ContractAddOn Get(int id)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOn_Get", new SqlParameter("ID", id));
            if (result.Success)
            {
                return new ContractAddOn(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<ContractAddOn> GetForContract(int contractID, bool showDeleted = false)
        {
            List<ContractAddOn> addOns = new List<ContractAddOn>();

            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("contractID", contractID));
            parameters.Add(new SqlParameter("Deleted", showDeleted));

            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOn_GetForContract", parameters);
            if (result.Success)
            {
                foreach(DataRow row in result.DataTable.Rows)
                {
                    addOns.Add(new ContractAddOn(row));
                }
            }

            return addOns;
        }

        public FunctionResult DeletePermanent()
        {
            List<Invoice> invoices = Invoice.GetForContractAddOn(ID);
            if (invoices.Where(o => o.Paid == true).Count() == 0)
            {
                ErrorLogger.LogError("Add On ID: " + ID, 0, 0, "ContractAddOn DeletePermanent");

                DBAccess db = new DBAccess();
                db.ExecuteNonQuery("ContractAddOn_DeletePermanent", new SqlParameter("id", ID));

                return new FunctionResult();
            }
            else
            {
                return new FunctionResult("Could not permanently delete contract add on, at least one invoice has already been paid.");
            }
        }
    }
}
