using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    internal interface IProcessInvoice
    {
        bool ShouldExecute(ProEstimatorData.DataModel.Contracts.Invoice invoice);
        void Execute(ProEstimatorData.DataModel.Contracts.Invoice invoice);
    }
}
