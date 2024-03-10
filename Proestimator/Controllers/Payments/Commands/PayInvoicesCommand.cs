using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimator.Business.Model;
using Proestimator.ViewModel;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Payments;
using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.Controllers.Payments.Commands
{
    public class PayInvoicesCommand : CommandBase
    {
        private IPaymentService _paymentService;
        private IStripeService _stripeService;
        private string _invoiceIDs;
        private int _loginID;

        public PayInvoicesCommand(IPaymentService paymentService, IStripeService stripeService, string invoiceIDs, int loginID)
        {
            _paymentService = paymentService;
            _stripeService = stripeService;
            _invoiceIDs = invoiceIDs;
            _loginID = loginID;
        }

        public override bool Execute()
        {
            // Cancel if nothing passed
            if (string.IsNullOrEmpty(_invoiceIDs))
            {
                return Error(Proestimator.Resources.ProStrings.MustSelectInvoiceByCheckingtheBox);
            }

            List<Invoice> invoices = GetInvoicesFromString(_invoiceIDs);

            FunctionResult payResult = _paymentService.PayInvoices(invoices, _loginID, _stripeService.GetStripeCustomerID(_loginID, true));
            
            if (payResult.Success)
            {
                return true;
            }
            else
            {
                return Error(payResult.ErrorMessage);
            }
        }

        private List<Invoice> GetInvoicesFromString(string selectedInvoices)
        {
            // Get the selected invoices in a list and pay for the group
            string[] pieces = selectedInvoices.Split(',');
            List<int> invoiceIDs = new List<int>();

            foreach (string idString in pieces)
            {
                invoiceIDs.Add(InputHelper.GetInteger(idString));
            }

            return ContractManager.GetInvoicesToBePaid(_loginID).Where(o => invoiceIDs.Contains(o.ID)).ToList();
        }
    }
}