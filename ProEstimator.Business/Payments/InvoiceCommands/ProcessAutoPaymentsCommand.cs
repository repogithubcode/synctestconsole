using System.Collections.Generic;
using System.Text;
using System;
using System.Linq;

using ProEstimator.Business.Model;
using ProEstimatorData;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Contracts;
using ProEstimator.Business.Payments.StripeCommands;
using ProEstimator.Business.Payments.RetentionCommands;

namespace ProEstimator.Business.Payments.InvoiceCommands
{
    /// <summary>
    /// Gets all invoices that are due for an auto payment and attempts to charge them.
    /// </summary>
    internal class ProcessAutoPaymentsCommand : CommandBase
    {
        public string Messages { get; private set; }

        private IStripeService _stripeService;

        public ProcessAutoPaymentsCommand(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        public override bool Execute()
        {
            StringBuilder messageBuilder = new StringBuilder();

            try
            {
                List<Invoice> dueInvoices = Invoice.GetAllDueInvoices();
                //dueInvoices = dueInvoices.Where(o => o.ID == 1135280).ToList();

                // Randomize the order of the invoices in case this process is started twice at the same time, this will avoid invoices being double charged.
                // dueInvoices = dueInvoices.OrderBy(o => Guid.NewGuid().ToString()).ToList();

                if (dueInvoices != null && dueInvoices.Count > 0)
                {
                    messageBuilder.AppendLine(DateTime.Now.ToString() + "\tPaymentProcessor has " + dueInvoices.Count.ToString() + " due invoices to process...");

                    foreach (Invoice invoice in dueInvoices)
                    {
                        ProcessInvoice(invoice, messageBuilder);
                    }
                }

            }
            catch (Exception ex)
            {
                ErrorLogger.LogError(ex, 0, "PaymentProcessor ProcessPayments");

                messageBuilder.AppendLine(DateTime.Now.ToString() + "\tPaymentProcessor Error: " + ex.Message);
                messageBuilder.AppendLine("\t" + ex.StackTrace);
            }

            Messages = messageBuilder.ToString();
            ErrorLogger.LogError(messageBuilder.ToString(), "PaymentProcessor ProcessPayments");

            return true;
        }

        private void ProcessInvoice(Invoice invoice, StringBuilder messageBuilder)
        {
            ShouldAutoPayInvoiceCommand shouldProcessCommand = new ShouldAutoPayInvoiceCommand(invoice);
            if (shouldProcessCommand.Execute())
            {
                PayInvoicesCommand command = GetPayInvoicesCommand(invoice);
                if (command.Execute())
                {
                    messageBuilder.AppendLine(command.ErrorMessage);

                    HandleAutoPaymentSuccessCommand successCommand = new HandleAutoPaymentSuccessCommand(invoice);
                    successCommand.Execute();
                }
                else
                {
                    messageBuilder.AppendLine(command.ErrorMessage);

                    HandleAutoPaymentFailureCommand failureCommand = new HandleAutoPaymentFailureCommand(invoice, command.ErrorMessage, command.stripeInfo == null ? 0 : command.stripeInfo.ID);
                    failureCommand.Execute();
                }
            }
            else
            {
                messageBuilder.AppendLine("Invoice " + invoice.ID + ": " + shouldProcessCommand.ErrorMessage);
            }
        }

        private PayInvoicesCommand GetPayInvoicesCommand(Invoice invoice)
        {
            string stripeCustomerID = _stripeService.GetStripeCustomerID(invoice.LoginID, true);
            List<Invoice> invoicesList = new List<Invoice>() { invoice };

            PayInvoicesCommand command = new PayInvoicesCommand(invoicesList, invoice.LoginID, stripeCustomerID, true);
            return command;
        }
    }
}
