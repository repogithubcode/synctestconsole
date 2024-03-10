using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData.Models;

namespace ProEstimatorData.DataModel.Admin
{
    public class DueInvoice
    {
        public int ID { get; set; }
        public int InvoiceID { get; set; }
        public string InvoiceSummary { get; set; }
        public decimal InvoiceAmount { get; set; }
        public int LoginID { get; set; }
        public string DueDate { get; set; }
        public string Paid { get; set; }
        public string AutoPay { get; set; }
        public string HasCard { get; set; }
        public string CardError { get; set; }
        public string ErrorMessage { get; set; }
        public int ContractID { get; set; }

        public DueInvoice()
        {

        }

        public DueInvoice(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            InvoiceID = InputHelper.GetInteger(row["InvoiceID"].ToString());
            InvoiceSummary = InputHelper.GetString(row["InvoiceSummary"].ToString());
            InvoiceAmount = InputHelper.GetDecimal(row["InvoiceAmount"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            DueDate = InputHelper.GetString(row["DueDate"].ToString());

            Paid = InputHelper.GetString(row["Paid"].ToString());
            AutoPay = InputHelper.GetString(row["AutoPay"].ToString());

            HasCard = InputHelper.GetString(row["HasCard"].ToString());
            CardError = InputHelper.GetString(row["CardError"].ToString());
            ErrorMessage = InputHelper.GetString(row["ErrorMessage"].ToString());
            ContractID = InputHelper.GetInteger(row["ContractID"].ToString());
        }

        public static List<DueInvoice> GetForFilter(string loginIDFilter, string rangeStartFilter, string rangeEndFilter, string hasCardErrorFilter,
                                                    string autoPayFilter, string paidFilter, string hasStripeInfoFilter)
        {
            List<DueInvoice> dueInvoices = new List<DueInvoice>();
            DataTable dataTable = GetForFilterData(loginIDFilter, rangeStartFilter, rangeEndFilter, hasCardErrorFilter, autoPayFilter, paidFilter, hasStripeInfoFilter);

            foreach (DataRow row in dataTable.Rows)
            {
                dueInvoices.Add(new DueInvoice(row));
            }

            return dueInvoices;
        }

        public static DataTable GetForFilterData(string loginIDFilter, string rangeStartFilter, string rangeEndFilter, string hasCardErrorFilter,
                                                    string autoPayFilter, string paidFilter, string hasStripeInfoFilter)
        {
            List<DueInvoice> dueInvoices = new List<DueInvoice>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@LoginID", Common.GetParameterValue(loginIDFilter)) { SqlDbType = SqlDbType.Int });
            parameters.Add(new SqlParameter("@DateStart", Common.GetParameterValue(rangeStartFilter)) { SqlDbType = SqlDbType.Date });
            parameters.Add(new SqlParameter("@DateEnd", Common.GetParameterValue(rangeEndFilter)) { SqlDbType = SqlDbType.Date });

            parameters.Add(new SqlParameter("@AutoPay", Common.GetParameterValue(autoPayFilter)) { SqlDbType = SqlDbType.Bit });

            parameters.Add(new SqlParameter("@Paid", Common.GetParameterValue(paidFilter)) { SqlDbType = SqlDbType.Bit });
            parameters.Add(new SqlParameter("@HasStripeInfo", Common.GetParameterValue(hasStripeInfoFilter)) { SqlDbType = SqlDbType.Bit });
            parameters.Add(new SqlParameter("@HasCardError", Common.GetParameterValue(hasCardErrorFilter)) { SqlDbType = SqlDbType.Bit });

            DBAccessTableResult tableResult = db.ExecuteWithTable("[dbo].[DueInvoiceReport]", parameters);

            return tableResult.DataTable;
        }
    }
}