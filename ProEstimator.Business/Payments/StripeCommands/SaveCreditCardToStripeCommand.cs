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

    internal class SaveCreditCardToStripeCommand : CommandBase
    {
        private string _stripeCustomerID;
        private string _token;

        /// <summary>
        /// If the Execute succedes, this is the new Stripe card.  
        /// </summary>
        public Card NewCard { get; private set; }

        /// <summary>
        /// Takes a Token returned by Stripe and creates a Card in Stripe linked to the passed CustomerID.
        /// </summary>
        /// <param name="stripeCustomerID">An existing customer ID.</param>
        /// <param name="token">A stripe token.</param>
        public SaveCreditCardToStripeCommand(string stripeCustomerID, string token)
        {
            _stripeCustomerID = stripeCustomerID;
            _token = token;
        }

        public override bool Execute()
        {
            try
            {
                // Save the card info to Stripe
                var cardOptions = new CardCreateOptions();
                cardOptions.Source = _token;

                var cardService = new CardService();
                NewCard = cardService.Create(_stripeCustomerID, cardOptions);
            }
            catch (System.Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "SaveStripeCustomerCommand");

                return Error(ex.Message);
            }

            return true;
        }
    }
}
