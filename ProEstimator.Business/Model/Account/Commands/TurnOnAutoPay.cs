using ProEstimatorData;
using ProEstimatorData.DataModel;

namespace ProEstimator.Business.Model.Account.Commands
{
    /// <summary>
    /// Loads the saved Stripe Info for an account and sets it to Auto Pay.  Also adds a log to MiscTracking of the change.
    /// </summary>
    public class TurnOnAutoPay : CommandBase
    {

        private int _loginID;
        private string _changeNote = "";

        public TurnOnAutoPay(int loginID, string changeNote)
        {
            _loginID = loginID;
            _changeNote = changeNote;
        }

        public override bool Execute()
        {
            // Load the saved Stripe Info for the account
            StripeInfo stripeInfo = StripeInfo.GetForLogin(_loginID);
            if (stripeInfo == null)
            {
                return Error("Saves stripe info not found for account " + _loginID);
            }

            if (stripeInfo.AutoPay)
            {
                return Error("Auto pay is already on for this account.");
            }

            // Set the auto pay value and save
            stripeInfo.SetAutoPay(true);
            SaveResult saveResult = stripeInfo.Save(_loginID);

            // Return if there was an error saving
            if (saveResult.Success == false)
            {
                return Error(saveResult.ErrorMessage);
            }

            // The stripe info was saved, now save a log of the change
            MiscTracking.Insert(_loginID, 0, "AutoPayOn", _changeNote);

            return true;
        }

    }
}
