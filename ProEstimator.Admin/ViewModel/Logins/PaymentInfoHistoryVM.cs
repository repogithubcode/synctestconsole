using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Data;

using ProEstimatorData;

namespace ProEstimator.Admin.ViewModel.Logins
{
    public class PaymentInfoHistoryVM
    {
        public int StripeInfoID { get; set; }
        public string StripeCustomerID { get; set; }
        public string StripeCardID { get; set; }
        public string CardLast4 { get; set; }
        public DateTime CardExpiration { get; set; }

        public bool DeleteFlag { get; set; }
        public bool CardError { get; set; }

        public string ErrorMessage { get; set; }
        public bool AutoPay { get; set; }
        public List<DeletePaymentInfoStatusVM> DeleteStatus { get; set; }

        public PaymentInfoHistoryVM()
        {

        }

        public PaymentInfoHistoryVM(DataRow row)
        {
            StripeInfoID = InputHelper.GetInteger(row["ID"].ToString());
            StripeCustomerID = InputHelper.GetString(row["StripeCustomerID"].ToString());
            StripeCardID = InputHelper.GetString(row["StripeCardID"].ToString());
            CardLast4 = InputHelper.GetString(row["CardLast4"].ToString());

            CardExpiration = InputHelper.GetDateTime(row["CardExpiration"].ToString());
            DeleteFlag = InputHelper.GetBoolean(row["DeleteFlag"].ToString());
            CardError = InputHelper.GetBoolean(row["CardError"].ToString());

            ErrorMessage = InputHelper.GetString(row["ErrorMessage"].ToString());
            AutoPay = InputHelper.GetBoolean(row["AutoPay"].ToString());
        }
    }

    public class DeletePaymentInfoStatusVM
    {
        public string When { get; private set; }
        public string Action { get; private set; }
        public string SalesRepName { get; private set; }
        public DeletePaymentInfoStatusVM(DateTime time, string what, string who)
        {
            When = InputHelper.GetString(time.ToString());
            Action = InputHelper.GetString(what.ToString()).Replace("CCInfo", "");
            SalesRepName = InputHelper.GetString(who.ToString());
        }
    }
}