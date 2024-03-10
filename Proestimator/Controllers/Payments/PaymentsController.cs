using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using Proestimator.Controllers.Payments.Commands;
using Proestimator.ViewModel;
using Proestimator.ViewModelMappers.Payments;
using ProEstimator.Business.Logic;
using ProEstimator.Business.Payments;
using ProEstimatorData;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.Controllers
{
    public class PaymentsController : SiteController
    {
        private IPaymentService _paymentService;
        private IStripeService _stripeService;

        public PaymentsController(IPaymentService paymentService, IStripeService stripeService)
        {
            _paymentService = paymentService;
            _stripeService = stripeService;
        }

        [HttpPost]
        [Route("{userID}/invoice/pay-with-saved")]
        public ActionResult PayWithSavedInfo(CustomerInvoiceVM vm)
        {
            return PayForMultipleInvoices(vm, vm.SelectedEarlyRenew);
        }

        private ActionResult PayForMultipleInvoices(CustomerInvoiceVM vm, string earlyRenewIds = "")
        {
            if (string.IsNullOrEmpty(vm.SelectedInvoices))
            {
                Session["ErrorMessage"] = Proestimator.Resources.ProStrings.MustSelectInvoiceByCheckingtheBox;
            }
            else
            {
                if (!string.IsNullOrEmpty(earlyRenewIds))
                {
                    DateTime now = DateTime.Now;
                    List<Invoice> invoices = GetInvoicesFromString(earlyRenewIds);
                    foreach (Invoice invoice in invoices)
                    {
                        invoice.EarlyRenewalStamp = now;                  
                        invoice.Save();
                    }
                }
                
                PayInvoicesCommand command = new PayInvoicesCommand(_paymentService, _stripeService, vm.SelectedInvoices, ActiveLogin.LoginID);
                if (command.Execute())
                {
                    _siteLoginManager.RefreshInvoiceInformationForAccount(ActiveLogin.LoginID);
                    return Redirect("/" + ActiveLogin.SiteUserID + "/settings/contract");
                }
                else
                {
                    ErrorLogger.LogError(command.ErrorMessage, ActiveLogin.LoginID, 0, "InvoiceController PayForMultipleInvoices Error");

                    Session["ErrorMessage"] = command.ErrorMessage;
                }
            }

            return Redirect("/" + ActiveLogin.SiteUserID + "/invoice/customer-invoice");
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

            return ContractManager.GetInvoicesToBePaid(ActiveLogin.LoginID).Where(o => invoiceIDs.Contains(o.ID)).ToList();
        }
    }
}