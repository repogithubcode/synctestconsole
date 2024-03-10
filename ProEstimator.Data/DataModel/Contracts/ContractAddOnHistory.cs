using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class ContractAddOnHistory
    {
        public int ID { get; private set; }
        public int ContractAddOnID { get; private set; }
        public int InvoiceID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public decimal Amount { get; private set; }
        public decimal SalesTax { get; private set; }
        public int StartQuantity { get; private set; }
        public int EndQuantity { get; private set; }
        public int Number { get; private set; }
        public string Notes { get; private set; }

        public ContractAddOnHistory(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            ContractAddOnID = InputHelper.GetInteger(row["ContractAddOnID"].ToString());
            InvoiceID = InputHelper.GetInteger(row["InvoiceID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Amount = InputHelper.GetDecimal(row["Amount"].ToString());
            SalesTax = InputHelper.GetDecimal(row["SalesTax"].ToString());
            StartQuantity = InputHelper.GetInteger(row["StartQuantity"].ToString());
            EndQuantity = InputHelper.GetInteger(row["EndQuantity"].ToString());
            Number = InputHelper.GetInteger(row["Number"].ToString());
            Notes = row["Notes"].ToString();
        }

        public static void Insert(int addOnID, int invoiceID, decimal amount, decimal salesTax, int startQuantity, int endQuantity, int num, string notes)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AddOnID", addOnID));
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));
            parameters.Add(new SqlParameter("Amount", amount));
            parameters.Add(new SqlParameter("SalesTax", salesTax));
            parameters.Add(new SqlParameter("StartQuantity", startQuantity));
            parameters.Add(new SqlParameter("EndQuantity", endQuantity));
            parameters.Add(new SqlParameter("Number", num));
            parameters.Add(new SqlParameter("Notes", notes));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("ContractAddOnHistory_Insert", parameters);
        }

        public static List<ContractAddOnHistory> GetForAddOn(ContractAddOn addOn)
        {
            List<ContractAddOnHistory> list = new List<ContractAddOnHistory>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AddOnID", addOn.ID));
            parameters.Add(new SqlParameter("EndQuantity", addOn.Quantity));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOnHistory_GetForAddOn", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new ContractAddOnHistory(row));
                }
            }

            return list;
        }

        public static List<ContractAddOnHistory> GetForInvoice(int invoiceID)
        {
            List<ContractAddOnHistory> list = new List<ContractAddOnHistory>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOnHistory_GetForInvoice", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new ContractAddOnHistory(row));
                }
            }

            return list;
        }

        public static int CountForAddOn(ContractAddOn addOn)
        {
            List<ContractAddOnHistory> list = new List<ContractAddOnHistory>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("AddOnID", addOn.ID));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("ContractAddOnHistory_GetForAddOn", parameters);
            if (result.Success)
            {
                return result.DataTable.Rows.Count;
            }

            return 0;
        }
    }
}
