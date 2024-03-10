using ProEstimatorData;
using System.Data;
using System.Web.WebPages;

namespace ProEstimator.Business.Model.CreditCardPayment
{
    public class CreditCardPaymentMerchantInfo
    {
        public int LoginID { get; set; }
        public string IntelliPayMerchantKey { get; set; }
        public string IntelliPayAPIKey { get; set; }
        public bool IntelliPayUseCardReader { get; set; }

        public CreditCardPaymentMerchantInfo()
        {
        }

        public CreditCardPaymentMerchantInfo(DataRow row)
        {
            if (row == null)
                return;

            LoginID = InputHelper.GetInteger(row["LoginID"].ToString());
            IntelliPayMerchantKey = row["IntelliPayMerchantKey"].ToString();
            IntelliPayAPIKey = row["IntelliPayAPIKey"].ToString();
            IntelliPayUseCardReader = InputHelper.GetBoolean(row["IntelliPayUseCardReader"].ToString());
        }
    }
}