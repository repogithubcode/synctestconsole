using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stripe;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Model;

namespace ProEstimator.Business.Payments.StripeCommands
{
    internal class CreateStripeChargeCommand : CommandBase
    {
        private int _loginID;
        private decimal _amount;
        private string _description;
        private string _stripeCustomerID;
        private bool _isRecurring;

        public Payment NewPaymentRecord { get; private set; }

        /// <summary>
        /// Use the saved Stripe card for the passed account to make a credit card charge.  If the charge is successful a Payment record is inserted and returned in the NewPaymentRecord property.
        /// </summary>
        /// <param name="loginID">The account to make the charge for.</param>
        /// <param name="amount">The total dollars to charge.</param>
        /// <param name="description">A description, shown in the Stripe site to identify the reason for the charge.</param>
        /// <param name="isRecurring">Not actually sure what this is for... It is sent as meta data with the charge, it is either now or at one point was used by some other process down the line.</param>
        public CreateStripeChargeCommand(int loginID, decimal amount, string description, bool isRecurring, string stripeCustomerID)
        {
            _loginID = loginID; 
            _amount = amount;
            _description = description;
            _isRecurring = isRecurring;
            _stripeCustomerID = stripeCustomerID;
        }

        public override bool Execute()
        {
            if (_amount > 10000)
            {
                return Error("Error, attempted charge is too high.");
            }

            // Load the Login Info and Stripe Info, cancel if neither are found.
            LoginInfo loginInfo = LoginInfo.GetByID(_loginID);
            if (loginInfo == null)
            {
                return Error("Account information not found for account " + _loginID);
            }

            StripeInfo stripeInfo = StripeInfo.GetForLogin(_loginID);
            if (stripeInfo == null)
            {
                return Error("Saved Stripe data not found for account " + _loginID);
            }

            if (string.IsNullOrEmpty(_stripeCustomerID))
            {
                return Error("Stripe customer ID not found for account " + _loginID);
            }

            // Make sure the current login info is the default card in stripe
            SetDefaultCardCommand setDefaultCardCommand = new SetDefaultCardCommand(_loginID, _stripeCustomerID, stripeInfo);
            if (!setDefaultCardCommand.Execute())
            {
                return Error(setDefaultCardCommand.ErrorMessage);
            }

            // Make a log of the payment we are about to make.
            ErrorLogger.LogError("Creating stripe payment for " + _amount + " (stripe customer " + _stripeCustomerID + ") for LoginID: " + _loginID, "CreateStripeCharge Log");

            try
            {
                // Create the Stripe charge.
                ChargeCreateOptions chargeOptions = GetChargeCreateOptions();
                ChargeService chargeService = new ChargeService();
                Charge stripeCharge = chargeService.Create(chargeOptions);

                if (stripeCharge.Paid)
                {
                    // The payment succeeded, insert a payment record
                    NewPaymentRecord = Payment.InsertPayment(stripeInfo.LoginID, PaymentType.Stripe, stripeCharge.Id, _amount, stripeCharge.FailureMessage);
                    return true;
                }
                else
                {
                    ErrorLogger.LogError("Payment Error " + stripeCharge.FailureCode + " " + stripeCharge.FailureMessage, "CreateStripeCharge");

                    stripeInfo.CardError = true;
                    stripeInfo.ErrorMessage = stripeCharge.FailureMessage;
                    stripeInfo.Save();

                    return Error(stripeCharge.FailureMessage);
                }
            }
            catch (Exception ex)
            {
                return Error(ex.Message);
            }
        }

        private ChargeCreateOptions GetChargeCreateOptions()
        {
            ChargeCreateOptions chargeOptions = new ChargeCreateOptions();
            chargeOptions.Amount = (int)(_amount * 100);
            chargeOptions.Currency = "usd";
            chargeOptions.Description = _description;
            chargeOptions.Customer = _stripeCustomerID;
      
            chargeOptions.Metadata = new Dictionary<string, string>();
            chargeOptions.Metadata.Add("IsRecurring", _isRecurring == true ? "1" : "0");

            return chargeOptions;
        }

    }
}
