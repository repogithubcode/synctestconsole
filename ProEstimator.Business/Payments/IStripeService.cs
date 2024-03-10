using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments
{
    public interface IStripeService
    {
        FunctionResultObj<Payment> CreateStripeCharge(int loginID, decimal amount, string description, bool isRecurring);
        FunctionResultObj<Stripe.Customer> CreateStripeCustomer(string emailAddress, int loginID);
        FunctionResult DeleteStripeCreditCard(int loginID);
        FunctionResult SetDefaultCard(int loginID, StripeInfo defaultCard);
        FunctionResultObj<StripeInfo> ProcessStripeToken(string token, string emailAddress, int loginID);
        FunctionResultObj<Stripe.Card> SaveCreditCardToSTripe(string stripeCustomerID, string token);
        string GetStripeCustomerID(int loginID, bool createCustomerIfNotFound);
    }
}
