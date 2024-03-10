using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments
{
    public interface IInvoiceRetentionService
    {

        bool ShouldProcessInvoice(Invoice invoice);
        void ProcessInvoiceFailure(Invoice invoice);
        void ProcessInvoiceSuccess(Invoice invoice);

    }
}
