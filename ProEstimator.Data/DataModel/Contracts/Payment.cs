using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.SqlClient;

namespace ProEstimatorData.DataModel.Contracts
{
    public class Payment
    {
        public int ID { get; private set; }
        public int LoginID { get; private set; }
        public DateTime TimeStamp { get; private set; }
        public PaymentType PaymentType { get; private set; }
        public string PaymentID { get; private set; }
        public decimal Total { get; private set; }

        public static Payment InsertPayment(int loginID, PaymentType paymentType, string paymentID, decimal total, string errorMessage)
        {
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("LoginID", loginID));
            parameters.Add(new SqlParameter("TimeStamp", DateTime.Now));
            parameters.Add(new SqlParameter("PaymentType", (int)paymentType));
            parameters.Add(new SqlParameter("PaymentID", paymentID));
            parameters.Add(new SqlParameter("Total", total));

            DBAccess db = new DBAccess();
            DBAccessIntResult result = db.ExecuteWithIntReturn("PaymentInsert", parameters);

            if (result != null && result.Value > 0)
            {
                return GetByID(result.Value);
            }

            return null;
        }

        public static Payment GetByID(int paymentID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PaymentGet", new SqlParameter("id", paymentID));
            if (result.Success)
            {
                return new Payment(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static Payment GetByStripeCharge(string stripeChargeID)
        {
            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PaymentGetByChargeID", new SqlParameter("PaymentID", stripeChargeID));
            if (result.Success)
            {
                return new Payment(result.DataTable.Rows[0]);
            }

            return null;
        }

        public static List<Payment> GetAllForLogin(int loginID)
        {
            List<Payment> list = new List<Payment>();

            DBAccess db = new DBAccess();
            DBAccessTableResult result = db.ExecuteWithTable("PaymentGetByLogin", new SqlParameter("loginID", loginID));
            if (result.Success)
            {
                foreach (DataRow row in result.DataTable.Rows)
                {
                    list.Add(new Payment(row));
                }
            }

            return list;
        }

        public Payment(DataRow row)
        {
            ID = InputHelper.GetInteger(row["ID"].ToString());
            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            TimeStamp = InputHelper.GetDateTime(row["TimeStamp"].ToString());
            PaymentType = (PaymentType)Enum.Parse(typeof(PaymentType), row["PaymentType"].ToString());
            PaymentID = row["PaymentID"].ToString();
            Total = InputHelper.GetDecimal(row["Total"].ToString());
        }
    }

    public enum PaymentType
    {
        Stripe = 1
    }
}
