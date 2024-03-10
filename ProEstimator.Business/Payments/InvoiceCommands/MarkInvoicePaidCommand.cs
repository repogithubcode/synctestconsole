using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.StripeCommands;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class MarkInvoicePaidCommand : CommandBase 
    {
        private Invoice _invoice;
        private int _paymentID;
        private bool _success;

        private List<IProcessInvoice> _afterInvoicePaidHandlers = new List<IProcessInvoice>();

        public MarkInvoicePaidCommand(Invoice invoice, int paymentID, bool success)
        {
            _invoice = invoice;
            _paymentID = paymentID;
            _success = success;

            _afterInvoicePaidHandlers.Add(new AfterPaidHandlerAutoRenew());
            _afterInvoicePaidHandlers.Add(new AfterPaidHandlerEarlyRenewal());
            _afterInvoicePaidHandlers.Add(new AfterPaidHanlderFirstAddOnPaymentEmail());
            _afterInvoicePaidHandlers.Add(new AfterPaidHandlerBundledAddOns());
        }

        public override bool Execute()
        {
            if (_success)
            {
                _invoice.Paid = true;
                _invoice.DatePaid = DateTime.Now;
            }

            _invoice.PaymentID = _paymentID;
            _invoice.Save();

            if (_success)
            {
                InvoicePaidTracking.Insert(_invoice.ID, true);

                foreach(IProcessInvoice processor in _afterInvoicePaidHandlers)
                {
                    if (processor.ShouldExecute(_invoice))
                    {
                        processor.Execute(_invoice);
                    }
                }

                return true;
            }

            return false;
        }
    }
}
