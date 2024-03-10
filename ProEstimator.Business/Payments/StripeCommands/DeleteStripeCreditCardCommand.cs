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
    internal class DeleteStripeCreditCardCommand : CommandBase
    {
        private int _loginID;

        public DeleteStripeCreditCardCommand(int loginID)
        {
            _loginID = loginID;
        }

        public override bool Execute()
        {
            // Get the Stripe info
            StripeInfo stripeInfo = StripeInfo.GetForLogin(_loginID);
            if (stripeInfo == null)
            {
                return Error("Saved info not found for the passed account.");
            }

            // Attempt to delete the card on Stripe's server
            DeleteStripeCC(stripeInfo);

            // Mark the card as deleted in our database
            stripeInfo.DeleteFlag = true;
            stripeInfo.Save();
        
            return true;
        }

        /// <summary>
        /// Attempt to delete the card on Stripe's server.  If this fails we log the failure, but continue to mark our record as deleted.
        /// </summary>
        /// <param name="stripeInfo"></param>
        private void DeleteStripeCC(StripeInfo stripeInfo)
        {
            try
            {
                CardService stripeCardService = new CardService();
                Card stripeDeleted = stripeCardService.Delete(stripeInfo.StripeCustomerID, stripeInfo.StripeCardID);

                if (stripeDeleted.Deleted.HasValue && !stripeDeleted.Deleted.Value)
                {
                    ErrorLogger.LogError(stripeDeleted.StripeResponse.Content, stripeInfo.LoginID, 0, "DeleteStripeCreditCardCommand");
                }
            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, stripeInfo.LoginID, "DeleteStripeCC");
            }
        }
    }
}
