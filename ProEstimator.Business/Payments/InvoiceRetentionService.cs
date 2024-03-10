using ProEstimatorData.DataModel.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProEstimator.Business.Payments
{
    public class InvoiceRetentionService : IInvoiceRetentionService
    {
        public bool ShouldProcessInvoice(Invoice invoice)
        {
            throw new NotImplementedException();
        }

        public void ProcessInvoiceSuccess(Invoice invoice)
        {
            throw new NotImplementedException();
        }

        public void ProcessInvoiceFailure(Invoice invoice)
        {
            throw new NotImplementedException();
        }
        
    }
}
