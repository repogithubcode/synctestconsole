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

    internal class CreateStripeCustomerCommand : CommandBase
    {

        private string _emailAddress;
        private int _loginID;

        public Stripe.Customer NewCustomer { get; private set; }

        /// <summary>
        /// Creates a new Stripe customer with the passed email address. 
        /// </summary>
        public CreateStripeCustomerCommand(string emailAddress, int loginID)
        {
            _emailAddress = emailAddress;
            _loginID = loginID;
        }

        public override bool Execute()
        {
            try
            {
                // Create the customer on the Stripe server.
                var newCustomer = new CustomerCreateOptions();
                newCustomer.Email = _emailAddress;
                newCustomer.Description = _loginID.ToString();

                CustomerService customerService = new CustomerService();
                NewCustomer = customerService.Create(newCustomer);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, "SaveStripeCustomerCommand");

                return Error(ex.Message);
            }

            return true;
        }
    }
}