using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class PaymentFailureEmail : IProcessInvoice
    {
        private List<InvoiceFailureLog> _invoiceFailureLogs;
        private Invoice _invoice;

        public bool ShouldExecute(Invoice invoice)
        {
            _invoice = invoice;
            _invoiceFailureLogs = InvoiceFailureLog.GetForInvoice(invoice.ID);

            // Do execute if it hasn't failed yet.
            if (_invoiceFailureLogs == null || _invoiceFailureLogs.Count == 0)
            {
                return true;
            }

            // Don't execute if we've already tried the max amount.
            if (_invoiceFailureLogs.Count >= InvoiceFailureLog.MaxChargeAttempts)
            {
                return false;
            }

            // The invoice has failed already but not the max number of times yet.
            // Figure out if enough time has passed to try again.
            DateTime lastFailure = _invoiceFailureLogs.OrderByDescending(o => o.TimeStamp).First().TimeStamp;
            TimeSpan lastFailureSpan = DateTime.Now - lastFailure;

            if (lastFailureSpan.TotalDays >= InvoiceFailureLog.GetDaysBetweenChargeAttempts(_invoiceFailureLogs.Count))
            {
                return true;
            }

            return false;
        }

        public void Execute(Invoice invoice)
        {
            int failureNumber = _invoiceFailureLogs.Count + 1;
            EmailManager.SendAutoPayFailureEmail(_invoice, failureNumber);
        }
        
    }
}
