using System;

namespace ProEstimator.Business.Model.CreditCardPayment
{
    public class CreditCardPaymentNotApprovedVM
    {
        public int UserID { get; set; }
        public bool IsDeclinedButWasPreviouslyApproved { get; set; }
        public bool IsTrial { get; set; }
    }
}
