using System.Collections.Generic;
using System.Text;

using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.StripeCommands;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class PayInvoicesCommand : CommandBase
    {
        private List<ProEstimatorData.DataModel.Contracts.Invoice> _invoices;
        private int _loginID;

        private StringBuilder _logBuilder = new StringBuilder();

        public StripeInfo stripeInfo { get; private set; }

        private decimal _total;
        private List<int> _ids = new List<int>();
        private bool _isRecurring = false;
        private string _stripeCustomerID;
        private bool _isAutoPay;
        
        public PayInvoicesCommand(List<Invoice> invoices, int loginID, string stripeCustomerID, bool isAutoPay)
        {
            _invoices = invoices;
            _loginID = loginID;
            _stripeCustomerID = stripeCustomerID;
            _isAutoPay = isAutoPay;
        }     
        

        public override bool Execute()
        {
            _logBuilder.Append(_loginID);

            // Stop if there are any validation errors and return them.
            string validationErrors = Validate();
            if (!string.IsNullOrEmpty(validationErrors))
            {
                ErrorLogger.LogError(validationErrors, "PayInvoicesCommand Validation");
                return Error(validationErrors);
            }

            CalculateTotals();
            string invoiceIDs = string.Join<int>(", ", _ids);

            _logBuilder.Append(" Total: " + _total.ToString("C") + " Invoice ID(s): " + invoiceIDs);

            // Create the charge with stripe
            CreateStripeChargeCommand createChargeCommand = new CreateStripeChargeCommand(_loginID, _total, "Invoice #s " + invoiceIDs, _isRecurring, _stripeCustomerID);

            bool success = createChargeCommand.Execute();
            if (success)
            {
                _logBuilder.Append(" Success!");

                // Mark all the invoices as paid and link them to this new payment
                foreach (Invoice invoice in _invoices)
                {
                    MarkInvoicePaidCommand paidCommand = new MarkInvoicePaidCommand(invoice, createChargeCommand.NewPaymentRecord.ID, true);
                    paidCommand.Execute();
                }

                ErrorMessage = _logBuilder.ToString();
            }
            else
            {
                ErrorMessage = createChargeCommand.ErrorMessage;

                if (!_isAutoPay)
                {
                    foreach (Invoice invoice in _invoices)
                    {
                        InvoiceFailureLog.Insert(invoice.ID, ErrorMessage, stripeInfo.ID, false);
                    }
                }

                _logBuilder.Append(" ERROR: " + createChargeCommand.ErrorMessage);
            }

            ErrorLogger.LogError(_logBuilder.ToString(), "PayInvoicesCommand");

            return success;
        }

        /// <summary>
        /// Returns False if there are any validation issues.  All validation issues are added to the ErrorMessage.
        /// </summary>
        /// <returns>Returns errors to show to the user.  An empty result means no validation errors.</returns>
        private string Validate()
        {
            StringBuilder userErrors = new StringBuilder();

            if (_invoices == null || _invoices.Count == 0)
            {
                _logBuilder.Append(" No invoices.");
                userErrors.AppendLine("No invoices.");
            }

            stripeInfo = StripeInfo.GetForLogin(_loginID);
            if (stripeInfo == null)
            {
                _logBuilder.Append(" No Stripe info.");
                userErrors.AppendLine("No saved card found.");
            }
            else
            {
                if (string.IsNullOrEmpty(stripeInfo.StripeCardID))
                {
                    _logBuilder.Append(" No Stripe card.");
                    userErrors.AppendLine("Saved card data could not be found.");
                }

                if (stripeInfo.CardError && _isAutoPay == false)
                {
                    _logBuilder.Append(" Card error.");
                    userErrors.AppendLine("Your card currently has an error, please clear the error try try again.  Error: " + stripeInfo.ErrorMessage);
                }
            }

            return userErrors.ToString();
        }

        private void CalculateTotals()
        {
            foreach (Invoice invoice in _invoices)
            {
                _total += invoice.InvoiceTotal;
                _ids.Add(invoice.ID);

                if (invoice.PaymentNumber > 0)
                {
                    _isRecurring = true;
                }
            }
        }
    }
}
