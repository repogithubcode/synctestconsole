using ProEstimator.Business.Model.CreditCardPayment;

namespace ProEstimator.Business.ILogic
{
    public interface ICreditCardPaymentService
    {
        CreditCardPaymentMerchantInfo GetMerchantCreditCardPaymentInfo(int loginID);
        int InsertCreditCardPaymentSuccessInfo(CreditCardPaymentSuccessVM model);
    }
}
