using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Admin
{
    public class StripePayment
    {
        public int LoginID { get; set; }
        public string Organization { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string Date { get; set; }
        public string Amount { get; set; }
        public int PaymentNumber { get; set; }
        public string TermDescription { get; set; }

        public StripePayment()
        {

        }

        public StripePayment(DataRow row)
        {
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            Organization = InputHelper.GetString(row["Organization"].ToString());
            Address = InputHelper.GetString(row["Address1"].ToString());
            City = InputHelper.GetString(row["City"].ToString());
            State = InputHelper.GetString(row["State"].ToString());
            Zip = InputHelper.GetString(row["Zip"].ToString());
            Date = InputHelper.GetString(row["TimeStamp"].ToString());
            Amount = Math.Round(ProEstimatorData.InputHelper.GetDouble(row["Total"].ToString()), 2).ToString("0.00");
            PaymentNumber = InputHelper.GetInteger(row["PaymentNumber"].ToString());
            TermDescription = InputHelper.GetString(row["TermDescription"].ToString());
        }

        public static List<StripePayment> GetForFilter(string startDate, string endDate)
        {
            List<StripePayment> stripePayments = new List<StripePayment>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@startDate", startDate));
            parameters.Add(new SqlParameter("@endDate", endDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("GetPayments_Stripe", parameters);

            foreach (DataRow row in tableResult.DataTable.Rows)
            {
                stripePayments.Add(new StripePayment(row));
            }

            return stripePayments;
        }

        public static DataTable GetForFilterData(string startDate, string endDate)
        {
            List<StripePayment> stripePayments = new List<StripePayment>();
            DBAccess db = new DBAccess();

            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@startDate", startDate));
            parameters.Add(new SqlParameter("@endDate", endDate));

            DBAccessTableResult tableResult = db.ExecuteWithTable("GetPayments_Stripe", parameters);

            return tableResult.DataTable;
        }
    }
}