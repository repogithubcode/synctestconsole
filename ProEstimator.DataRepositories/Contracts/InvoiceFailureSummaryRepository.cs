using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.DataRepositories.Contracts
{
    public class InvoiceFailureSummaryRepository : IInvoiceFailureSummaryRepository
    {
        private InvoiceFailureSummary Instantiate(DataRow row)
        {
            InvoiceFailureSummary invoiceFailureSummary = new InvoiceFailureSummary();

            invoiceFailureSummary.InvoiceID = InputHelper.GetInteger(row["InvoiceID"].ToString());
            invoiceFailureSummary.LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            invoiceFailureSummary.InvoiceTotal = InputHelper.GetDecimal(row["InvoiceTotal"].ToString());
            invoiceFailureSummary.DueDate = InputHelper.GetDateTime(row["DueDate"].ToString());
            invoiceFailureSummary.InvoiceNotes = InputHelper.GetString(row["InvoiceNotes"].ToString());
            invoiceFailureSummary.InvoiceSummary = InputHelper.GetString(row["InvoiceSummary"].ToString());
            invoiceFailureSummary.LastFailStamp = InputHelper.GetDateTime(row["LastFailStamp"].ToString());
            invoiceFailureSummary.FailNote = InputHelper.GetString(row["FailNote"].ToString());
            invoiceFailureSummary.LastFour = InputHelper.GetString(row["CardLast4"].ToString());
            invoiceFailureSummary.Expiration = InputHelper.GetString(row["Expiration"].ToString());
            invoiceFailureSummary.StripeCardID = InputHelper.GetString(row["StripeCardID"].ToString());

            return invoiceFailureSummary;
        }

        public List<InvoiceFailureSummary> Get(int days)
        {
            List<InvoiceFailureSummary> results = new List<InvoiceFailureSummary>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InvoiceFailureLog_GetSummary", new SqlParameter("DayWindow", days));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                results.Add(Instantiate(row));
            }

            return results;
        }

        public List<InvoiceFailureSummary> GetInvoiceFails(int loginID)
        {
            List<InvoiceFailureSummary> returnList = new List<InvoiceFailureSummary>();

            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InvoiceFailureLog_GetSummary", new SqlParameter("DayWindow", DBNull.Value));

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                if (InputHelper.GetInteger(row["LoginID"].ToString()) == loginID)
                {
                    returnList.Add(Instantiate(row));
                }
            }

            return returnList;
        }
    }
}
