using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.StripeCommands;

namespace ProEstimator.Business.Payments.RetentionCommands
{
    internal class ShouldAutoPayInvoiceCommand : CommandBase
    {

        private Invoice _invoice;

        public ShouldAutoPayInvoiceCommand(Invoice invoice)
        {
            _invoice = invoice;
        }

        public override bool Execute()
        {
            //Check if invoice is valid
            Invoice invoice = Invoice.Get(_invoice.ID);
            if(invoice == null)
            {
                return Error("Invoice Not found.");
            }
            if(invoice.Paid)
            {
                return Error("Invoice Already paid.");
            }
            if (invoice.IsDeleted)
            {
                return Error("Invoice deleted.");
            }
            
            // If the invoice has failed already, don't add it again unless enough time has passed
            List<InvoiceFailureLog> failureLog = InvoiceFailureLog.GetForInvoice(_invoice.ID);
            if (failureLog.Count > 0)
            {
                // If there are too many failures on this invoice don't try again
                if (failureLog.Count >= InvoiceFailureLog.MaxChargeAttempts)
                {
                    return Error("Max attempts made.");
                }

                // Don't process if we've already failed too recently.
                DateTime lastFail = failureLog.OrderByDescending(o => o.TimeStamp).First().TimeStamp;
                TimeSpan timeSpan = DateTime.Now - lastFail;

                if(timeSpan.TotalDays < InvoiceFailureLog.GetDaysBetweenChargeAttempts(failureLog.Count))
                {
                    return Error("Waiting to try again.");
                }
            }

            // If the Stripe card has an error don't try running it again
            StripeInfo stripeInfo = StripeInfo.GetForLogin(_invoice.LoginID);
            if (stripeInfo == null)
            {
                // Don't try running if there's no saved info
                return Error("No saved stripe info.");
            }

            return true;
        }

    }
}