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
    internal class MarkInvoiceUnpaidCommand : CommandBase
    {
        private ProEstimatorData.DataModel.Contracts.Invoice _invoice;
        private int _paymentID;
        private bool _success;

        public MarkInvoiceUnpaidCommand(ProEstimatorData.DataModel.Contracts.Invoice invoice, int paymentID, bool success)
        {
            _invoice = invoice;
            _paymentID = paymentID;
            _success = success;
        }

        public override bool Execute()
        {
            if (!_invoice.Paid)
            {
                return Error("Invoid already marked unpaid");
            }

            if (_success)
            {
                _invoice.Paid = false;
                _invoice.DatePaid = null;
            }

            _invoice.PaymentID = _paymentID;
            SaveResult saveResult = _invoice.Save();

            if (saveResult.Success)
            {
                return true;
            }
            else
            {
                return Error(saveResult.ErrorMessage);
            }
        }
    }
}
