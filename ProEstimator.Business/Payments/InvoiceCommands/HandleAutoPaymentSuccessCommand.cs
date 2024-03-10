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
    internal class HandleAutoPaymentSuccessCommand : CommandBase
    {

        private Invoice _invoice;

        private List<IProcessInvoice> _processors = new List<IProcessInvoice>();

        public HandleAutoPaymentSuccessCommand(Invoice invoice)
        {
            _invoice = invoice;

           // _processors.Add(new PaymentSuccessEmail());
        }

        public override bool Execute()
        {
            //foreach (IProcessInvoice processor in _processors)
            //{
            //    if (processor.ShouldExecute(_invoice))
            //    {
            //        processor.Execute(_invoice);
            //    }
            //}

            return true;
        }

    }
}
