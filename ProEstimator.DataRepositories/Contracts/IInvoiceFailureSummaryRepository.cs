using System.Collections.Generic;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.DataRepositories.Contracts
{
    public interface IInvoiceFailureSummaryRepository
    {
        List<InvoiceFailureSummary> Get(int days);
        List<InvoiceFailureSummary> GetInvoiceFails(int loginID);
    }
}
