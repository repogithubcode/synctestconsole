using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimator.Business.Logic;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal class PaymentSuccessEmail : IProcessInvoice
    {
        private Invoice _invoice;

        public bool ShouldExecute(Invoice invoice)
        {
            _invoice = invoice;
            return true;
        }

        public void Execute(Invoice invoice)
        {
            EmailManager.SendAutoPaySuccessEmail(_invoice.LoginID);
        }

    }
}
