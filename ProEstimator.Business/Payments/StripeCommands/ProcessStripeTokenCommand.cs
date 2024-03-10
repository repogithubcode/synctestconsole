using System;

using ProEstimator.Business.Model;
using ProEstimator.Business.Model.Account.Commands;
using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Payments.StripeCommands
{

    internal class ProcessStripeTokenCommand : CommandBase
    {

        private string _token;
        private string _emailAddress;
        private int _loginID;
        private string _stripeCustomerID;

        public StripeInfo NewStripeInfo { get; private set; }

        /// <summary>
        /// Use data returned from a stripe token creation (the stripe popup that takes CC info) and save the customer and credit card to Stripe and 
        /// to the ProE database.
        /// </summary>
        public ProcessStripeTokenCommand(string token, string emailAddress, int loginID, string stripeCustomerID)
        {
            _token = token;
            _emailAddress = emailAddress;
            _loginID = loginID;
            _stripeCustomerID = stripeCustomerID;
        }

        public override bool Execute()
        {
            System.Text.StringBuilder errorBuilder = new System.Text.StringBuilder();

            try
            {
                // Record if the existing card info is on auto pay, if so we'll turn on auto pay on the new card.
                bool autoPay = IsCurrentCardOnAutoPay();

                // Check if we already have a Stripe Customer ID for the account.  If we don't, create one.
                if (string.IsNullOrEmpty(_stripeCustomerID))
                {
                    CreateStripeCustomerCommand createCustomerCommand = new CreateStripeCustomerCommand(_emailAddress, _loginID);
                    if (!createCustomerCommand.Execute())
                    {
                        return Error(createCustomerCommand.ErrorMessage);
                    }
                    else
                    {
                        _stripeCustomerID = createCustomerCommand.NewCustomer.Id;
                    }
                }

                // Delete the existing card
                DeleteStripeCreditCardCommand deleteCardCommand = new DeleteStripeCreditCardCommand(_loginID);
                deleteCardCommand.Execute();

                // Save the credit card info to Stripe.  If this fails we return with an error and without saving anything.
                SaveCreditCardToStripeCommand saveCreditCardCommand = new SaveCreditCardToStripeCommand(_stripeCustomerID, _token);
                if (!saveCreditCardCommand.Execute())
                {
                    return Error(saveCreditCardCommand.ErrorMessage);
                }

                // Create the new Stripe Info record
                NewStripeInfo = new StripeInfo();
                NewStripeInfo.LoginID = _loginID;
                NewStripeInfo.StripeCustomerID = _stripeCustomerID;
                NewStripeInfo.StripeCardID = saveCreditCardCommand.NewCard.Id;
                NewStripeInfo.CardLast4 = saveCreditCardCommand.NewCard.Last4;
                NewStripeInfo.CardExpiration = new DateTime((int)saveCreditCardCommand.NewCard.ExpYear, (int)saveCreditCardCommand.NewCard.ExpMonth, 1);

                NewStripeInfo.Save();

                // Turn auto pay on if it was on for the deleted card.
                if (autoPay)
                {
                    new TurnOnAutoPay(_loginID, "Copied").Execute();
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, _loginID, "SaveCreditCardFromTokenCommand");
                errorBuilder.AppendLine(ex.Message);
            }

            ErrorMessage = errorBuilder.ToString();
            return string.IsNullOrEmpty(ErrorMessage);
        }

        private bool IsCurrentCardOnAutoPay()
        {
            StripeInfo stripeInfo = StripeInfo.GetForLogin(_loginID);
            if (stripeInfo != null)
            {
                return stripeInfo.AutoPay;
            }

            return false;
        }

    }
}
