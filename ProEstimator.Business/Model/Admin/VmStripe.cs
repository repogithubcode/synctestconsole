using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ProEstimator.Business.Extension;
using ProEstimator.Business.ILogic;

namespace ProEstimator.Business.Model.Admin
{
    public class VmStripe : IModelMap<VmStripe>
    {
        public int LoginID { get; set; }
        public string StripeCustomerID { get; set; }
        public string StripeCardID { get; set; }
        public string CardLast4 { get; set; }
        public string CardExpiration { get; set; }
        public bool? DeleteFlag { get; set; }
        public bool? CardError { get; set; }
        public string ErrorMessage { get; set; }
        public string PlaidAccessToken { get; set; }
        public string PlaidItemID { get; set; }
        public string StripeBankAccountToken { get; set; }
        public bool? AutoPay { get; set; }
        public VmStripe ToModel(DataRow row)
        {
            var model = new VmStripe();
            model.LoginID = (int)row["LoginID"];
            model.StripeCustomerID = row["StripeCustomerID"].SafeString();
            model.StripeCardID = row["StripeCardID"].SafeString();
            model.CardLast4 = row["CardLast4"].SafeString();
            model.CardExpiration = row["CardExpiration"].SafeDate();
            model.DeleteFlag = row["DeleteFlag"].SafeBool();
            model.CardError = row["CardError"].SafeBool();
            model.ErrorMessage = row["ErrorMessage"].SafeString();
            model.PlaidAccessToken = row["PlaidAccessToken"].SafeString();
            model.PlaidItemID = row["PlaidItemID"].SafeString();
            model.StripeBankAccountToken = row["StripeBankAccountToken"].SafeString();
            model.AutoPay = row["AutoPay"].SafeBool();

            return model;
        }
    }
}
