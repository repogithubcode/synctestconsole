using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Stripe;

using ProEstimator.Business.Payments.StripeCommands;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.InvoiceCommands;

namespace ProEstimator.Business.Payments
{
    public class PaymentService : IPaymentService
    {

        public FunctionResult PayInvoices(List<ProEstimatorData.DataModel.Contracts.Invoice> invoices, int loginID, string stripeCustomerID)
        {
            var command = new PayInvoicesCommand(invoices, loginID, stripeCustomerID, false);
            return new FunctionResult(command.Execute(), command.ErrorMessage);
        }

        public FunctionResult ProcessAutoPay(IStripeService stripeService)
        {
            var command = new ProcessAutoPaymentsCommand(stripeService);
            return new FunctionResult(command.Execute(), command.ErrorMessage);
        }

        public FunctionResult MarkInvoicePaid(ProEstimatorData.DataModel.Contracts.Invoice invoice, int paymentID, bool success)
        {
            var command = new MarkInvoicePaidCommand(invoice, paymentID, success);
            return new FunctionResult(command.Execute(), command.ErrorMessage);
        }

        public FunctionResult MarkInvoiceUnPaid(ProEstimatorData.DataModel.Contracts.Invoice invoice, int paymentID, bool success)
        {
            var command = new MarkInvoiceUnpaidCommand(invoice, paymentID, success);
            return new FunctionResult(command.Execute(), command.ErrorMessage);
        }
        
    }
}
