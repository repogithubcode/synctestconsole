using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Business.Payments
{
    public interface IPaymentService
    {
        FunctionResult PayInvoices(List<Invoice> invoices, int loginID, string stripeCustomerID);
        FunctionResult ProcessAutoPay(IStripeService stripeService);
        FunctionResult MarkInvoicePaid(Invoice invoice, int paymentID, bool success);
        FunctionResult MarkInvoiceUnPaid(Invoice invoice, int paymentID, bool success);
    }
}
