using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class HandleAutoPaymentFailureCommand : CommandBase
    {

        private Invoice _invoice;
        private string _errorMessage;
        private int _stripeInfoID;

        private List<IProcessInvoice> _processors = new List<IProcessInvoice>();

        public HandleAutoPaymentFailureCommand(Invoice invoice, string errorMessage, int stripeInfoID)
        {
            _invoice = invoice;
            _errorMessage = errorMessage;
            _stripeInfoID = stripeInfoID;

            _processors.Add(new PaymentFailureEmail());
        }

        public override bool Execute()
        {
            foreach (IProcessInvoice processor in _processors)
            {
                if (processor.ShouldExecute(_invoice))
                {
                    processor.Execute(_invoice);
                }
            }

            InvoiceFailureLog.Insert(_invoice.ID, _errorMessage, _stripeInfoID);

            return true;
        }

    }
}
