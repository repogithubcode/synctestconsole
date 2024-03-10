using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stripe;

using ProEstimator.Business.Model;
using ProEstimatorData.DataModel;
using ProEstimatorData;

namespace ProEstimator.Business.Payments.StripeCommands
{
    internal class SetDefaultCardCommand : CommandBase
    {
        private int _loginID;
        private string _stripeCustomerID;
        private StripeInfo _defaultCard;

        public SetDefaultCardCommand(int loginID, string stripeCustomerID, StripeInfo defaultCard)
        {
            _loginID = loginID;
            _stripeCustomerID = stripeCustomerID;
            _defaultCard = defaultCard;
        }

        public override bool Execute()
        {
            if (string.IsNullOrEmpty(_stripeCustomerID))
            {
                return Error("Stripe Customer ID not found.");
            }

            if (_defaultCard == null)
            {
                return Error("Default card not passed.");
            }

            if (String.IsNullOrEmpty(_defaultCard.StripeCardID))
            {
                return Error("Invalid card ID.");
            }

            try
            {
                // Create the customer on the Stripe server.
                var customerUpdate = new CustomerUpdateOptions();
                customerUpdate.DefaultSource = _defaultCard.StripeCardID;

                CustomerService customerService = new CustomerService();
                customerService.Update(_stripeCustomerID, customerUpdate);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, "UpdateDefaultCardCommand");

                return Error(ex.Message);
            }

            return true;
        }
    }
}
