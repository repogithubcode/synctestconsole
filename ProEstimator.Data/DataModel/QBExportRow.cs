using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel
{
    public class QBExportRow
    {
        public DateTime Date { get; private set; }
        public int EstimateID { get; private set; }
		public string CustomerName { get; private set; }
		public string CustomerAddress1 { get; private set; }
		public string CustomerAddress2 { get; private set; }
		public string CustomerCity { get; private set; }
		public string CustomerState { get; private set; }
		public string CustomerZip { get; private set; }
		public string CustomerPhone { get; private set; }
        public string CustomerEmail { get; private set; }
		public string VIN { get; private set; }
		public string ItemDescription { get; private set; }
		public double Quantity { get; private set; }
		public double Rate { get; private set; }
		public string Taxed { get; private set; }
		public double Amount { get; private set; }
		public string ItemName { get; private set; }
		public DateTime ClosedRODate { get; private set; }
		public string StateTaxID { get; private set; }
		public string InsuranceCompany { get; private set; }
        public string InsuranceClaimDetails { get; private set; }
        public string QBInvoiceID { get; private set; }

        public string Notes { get; private set; }
        public string DirectLineItemTotal { get; private set; }
        public string DiscountMarkupLineItemTotal { get; private set; }


        public QBExportRow(DataRow row)
        {
            Date = InputHelper.GetDateTime(row["Date"].ToString());
            EstimateID = InputHelper.GetInteger(row["EstimateID"].ToString());
            CustomerName = InputHelper.GetString(row["CustomerName"].ToString());
            CustomerAddress1 = InputHelper.GetString(row["CustomerAddress1"].ToString());
            CustomerAddress2 = InputHelper.GetString(row["CustomerAddress2"].ToString());
            CustomerCity = InputHelper.GetString(row["CustomerCity"].ToString());
            CustomerState = InputHelper.GetString(row["CustomerState"].ToString());
            CustomerZip = InputHelper.GetString(row["CustomerZip"].ToString());
            CustomerPhone = InputHelper.GetString(row["CustomerPhone"].ToString());
            CustomerEmail = InputHelper.GetString(row["CustomerEmail"].ToString());
		    VIN = InputHelper.GetString(row["VIN"].ToString());
		    ItemDescription = InputHelper.GetString(row["ItemDescription"].ToString());
		    Quantity  = InputHelper.GetDouble(row["Quantity"].ToString());
		    Rate = InputHelper.GetDouble(row["Rate"].ToString());
		    Taxed = InputHelper.GetString(row["Taxed"].ToString());
		    Amount = InputHelper.GetDouble(row["Amount"].ToString());
		    ItemName = InputHelper.GetString(row["ItemName"].ToString());
		    ClosedRODate = InputHelper.GetDateTime(row["ClosedRODate"].ToString());
		    StateTaxID = InputHelper.GetString(row["StateTaxID"].ToString());
		    InsuranceCompany = InputHelper.GetString(row["InsuranceCompany"].ToString());
            InsuranceClaimDetails = InputHelper.GetString(row["InsuranceClaimDetails"].ToString());
            QBInvoiceID = InputHelper.GetString(row["QBInvoiceID"].ToString());
            Notes = InputHelper.GetString(row["Notes"].ToString());
            DirectLineItemTotal = InputHelper.GetString(row["DirectLineItemTotal"].ToString());
            DiscountMarkupLineItemTotal = InputHelper.GetString(row["DiscountMarkupLineItemTotal"].ToString());
        }

        public static List<QBExportRow> GetData(int loginID, DateTime startDate, DateTime endDate)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("StartDate", startDate));
            parameters.Add(new SqlParameter("EndDate", endDate));

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("QB_Export", parameters);

            List<QBExportRow> exportRows = new List<QBExportRow>();

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                exportRows.Add(new QBExportRow(row));
            }

            return exportRows;
        }
    }
}
