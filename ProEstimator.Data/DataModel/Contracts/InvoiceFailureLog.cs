using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimatorData.DataModel.Contracts
{
    public class InvoiceFailureLog
    {

        public int ID { get; private set; }
        public int InvoiceID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public string Note { get; private set; }
        public string LastFour { get; private set; }
        public string Expiration { get; private set; }
        public string StripeCardID { get; private set; }

        public InvoiceFailureLog(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            InvoiceID = InputHelper.GetInteger(row["InvoiceID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            Note = InputHelper.GetString(row["Note"].ToString());
            LastFour = InputHelper.GetString(row["CardLast4"].ToString());
            Expiration = InputHelper.GetString(row["Expiration"].ToString());
            StripeCardID = InputHelper.GetString(row["StripeCardID"].ToString());
        }

        public static void Insert(int invoiceID, string note, int stripeInfoID, bool autoPay = true)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));
            parameters.Add(new SqlParameter("Note", note));
            parameters.Add(new SqlParameter("StripeInfoID", stripeInfoID));
            parameters.Add(new SqlParameter("AutoPay", autoPay));

            DBAccess db = new DBAccess();
            db.ExecuteNonQuery("InvoiceFailureLog_Insert", parameters);
        }

        public static List<InvoiceFailureLog> GetForInvoice(int invoiceID)
        {
            List<InvoiceFailureLog> list = new List<InvoiceFailureLog>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("InvoiceFailureLog_GetForInvoice", new SqlParameter("InvoiceID", invoiceID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new InvoiceFailureLog(row));
                }
            }

            return list;
        }

        public static List<InvoiceFailureLog> GetForInvoiceAdmin(int invoiceID)
        {
            List<InvoiceFailureLog> list = new List<InvoiceFailureLog>();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("InvoiceID", invoiceID));
            parameters.Add(new SqlParameter("AutoPay", DBNull.Value));

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("InvoiceFailureLog_GetForInvoice", parameters);
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new InvoiceFailureLog(row));
                }
            }

            return list;
        }

        public static int MaxChargeAttempts { get { return 3; } }
        public static int GetDaysBetweenChargeAttempts(int failureCount)
        {
            if (failureCount == 1)
            {
                return 2;
            }
            else if(failureCount == 2)
            {
                return 4;
            }

            return 1;
        }
    }
}
