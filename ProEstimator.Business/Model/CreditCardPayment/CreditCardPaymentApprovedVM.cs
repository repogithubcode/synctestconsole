using System;

namespace ProEstimator.Business.Model.CreditCardPayment
{
    public class CreditCardPaymentApprovedVM
    {
        public int UserID { get; set; }
        public bool IsDeclinedButWasPreviouslyApproved { get; set; }

        public string IntelliPayMerchantKey { get; set; }
        public string IntelliPayAPIKey { get; set; }
        public bool IntelliPayUseCardReader { get; set; }

        public bool IsTrial { get; set; }
    }
}
