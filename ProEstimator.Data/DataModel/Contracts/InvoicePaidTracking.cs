using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class InvoicePaidTracking
    {

        public bool Paid { get; private set; }
        public int SalesRepID { get; private set; }
        public DateTime TimeStamp { get; private set; }

        public static void Insert(int invoiceID, bool isPaid, int salesRepID = 0)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));
            parameters.Add(new SqlParameter("Paid", isPaid));
            parameters.Add(new SqlParameter("SalesRepID", salesRepID));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("InvoicePaidTracking_Insert", parameters);
        }

        public InvoicePaidTracking(DataRow row)
        {
            Paid = InputHelper.GetBoolean(row["Paid"].ToString());
            SalesRepID = InputHelper.GetInteger(row["SalesRepID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["DateTracked"].ToString());
        }

        public static List<InvoicePaidTracking> GetForInvoice(int invoiceID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult tableResult = db.ExecuteWithTable("InvoicePaidTracking_GetForInvoice", new SqlParameter("InvoiceID", invoiceID));

            List<InvoicePaidTracking> list = new List<InvoicePaidTracking>();

            if (tableResult.Success)
            {
                foreach(DataRow row in tableResult.DataTable.Rows)
                {
                    list.Add(new InvoicePaidTracking(row));
                }
            }

            return list;
        }

    }
}