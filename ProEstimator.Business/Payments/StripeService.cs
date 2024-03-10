using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stripe;

using ProEstimator.Business.Payments.StripeCommands;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.InvoiceCommands;


namespace ProEstimator.Business.Payments
{
    public class StripeService : IStripeService
    {

        public FunctionResultObj<Payment> CreateStripeCharge(int loginID, decimal amount, string description, bool isRecurring)
        {
            string stripeCustomerID = GetStripeCustomerID(loginID, true);

            CreateStripeChargeCommand command = new CreateStripeChargeCommand(loginID, amount, description, isRecurring, stripeCustomerID);
            if (command.Execute())
            {
                return new FunctionResultObj<Payment>(command.NewPaymentRecord);
            }
            else
            {
                return new FunctionResultObj<Payment>(command.ErrorMessage);
            }
        }

        public FunctionResultObj<Stripe.Customer> CreateStripeCustomer(string emailAddress, int loginID)
        {
            CreateStripeCustomerCommand command = new CreateStripeCustomerCommand(emailAddress, loginID);
            if (command.Execute())
            {
                return new FunctionResultObj<Stripe.Customer>(command.NewCustomer);
            }
            else
            {
                return new FunctionResultObj<Stripe.Customer>(command.ErrorMessage);
            }
        }

        public FunctionResult DeleteStripeCreditCard(int loginID)
        {
            DeleteStripeCreditCardCommand command = new DeleteStripeCreditCardCommand(loginID);
            if (command.Execute())
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(command.ErrorMessage);
            }
        }

        public FunctionResult SetDefaultCard(int loginID, StripeInfo defaultCard)
        {
            SetDefaultCardCommand command = new SetDefaultCardCommand(loginID, GetStripeCustomerID(loginID), defaultCard);
            if (command.Execute())
            {
                return new FunctionResult();
            }
            else
            {
                return new FunctionResult(command.ErrorMessage);
            }
        }

        public FunctionResultObj<StripeInfo> ProcessStripeToken(string token, string emailAddress, int loginID)
        {
            string stripeCustomerID = GetStripeCustomerID(loginID, true);

            ProcessStripeTokenCommand command = new ProcessStripeTokenCommand(token, emailAddress, loginID, stripeCustomerID);
            if (command.Execute())
            {
                return new FunctionResultObj<StripeInfo>(command.NewStripeInfo);
            }
            else
            {
                return new FunctionResultObj<StripeInfo>(command.ErrorMessage);
            }
        }

        public FunctionResultObj<Card> SaveCreditCardToSTripe(string stripeCustomerID, string token)
        {
            SaveCreditCardToStripeCommand command = new SaveCreditCardToStripeCommand(stripeCustomerID, token);
            if (command.Execute())
            {
                return new FunctionResultObj<Card>(command.NewCard);
            }
            else
            {
                return new FunctionResultObj<Card>(command.ErrorMessage);
            }
        }

        /// <summary>
        /// Gets the Stripe Customer ID linked to the passed Login ID.
        /// </summary>
        /// <returns></returns>
        public string GetStripeCustomerID(int loginID, bool createCustomerIfNotFound = false)
        {
            // First check the ID link, this is where the data should be.
            StripeCustomerIDLink idLink = StripeCustomerIDLink.Get(loginID);
            if (idLink != null)
            {
                return idLink.StripeCustomerID;
            }

            // Historically we stored a new CustomerID with every StripeInfo record, which was a mistake, there should be one per account.
            string lastCustomerID = StripeInfo.GetLastStripeCustomerIDForLogin(loginID);
            if (!string.IsNullOrEmpty(lastCustomerID))
            {
                StripeCustomerIDLink.Insert(loginID, lastCustomerID);
                return lastCustomerID;
            }

            if (createCustomerIfNotFound == true)
            {
                Contact contact = Contact.GetContactForLogins(loginID);
                var command = new CreateStripeCustomerCommand(contact.Email, loginID);
            }

            return "";
        }


    }
}
