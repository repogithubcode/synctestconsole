using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Text;

using ProEstimatorData.DataModel.Contracts;

using Proestimator.ViewModel.Contracts;
using Proestimator.ViewModelMappers.Contracts;

namespace Proestimator.ViewModel
{
    public class CustomerInvoiceVM
    {
        public int LoginID { get; set; }
        public List<InvoiceVM> Invoices { get; set; }

        /// <summary>A comma seperated list of the checked invoices</summary>
        public string SelectedInvoices { get; set; }
        public string SelectedEarlyRenew { get; set; }

        public List<InvoiceWithTotal> TotalsArray
        {
            get
            {
                var invoices = new List<InvoiceWithTotal>();

                foreach (Invoice invoice in _invoices)
                {
                    invoices.Add(new InvoiceWithTotal(invoice.ID, invoice.InvoiceTotal, invoice.Notes, invoice.DueDate, invoice.EarlyRenewal && invoice.PaymentNumber <= 1));
                }

                return invoices;
            }
        }

        public void LoadInvoices(List<Invoice> invoices)
        {
            _invoices = invoices;
            Invoices = new List<InvoiceVM>();

            InvoiceVMMapper invoiceMapper = new InvoiceVMMapper();

            foreach (Invoice invoice in invoices)
            {
                Invoices.Add(invoiceMapper.Map(new InvoiceVMMapperConfiguration() { Invoice = invoice, EarlyRenew = invoice.EarlyRenewal && invoice.PaymentNumber <= 1 }));
            }
        }

        private List<Invoice> _invoices;

        public bool UserCanChangeContract { get; set; }

        public string PlaidToken { get; set; }
        public string PlaidAccountID { get; set; }

        public bool HasSavedPaymentInfo { get; set; }
        public string Last4 { get; set; }
        public DateTime CardExpiration { get; set; }

        public bool AllowAutoPay { get; set; }
        public bool AutoPaySelected { get; set; }
        public string AutoPaySelectedString { get; set; }
        public bool ForceAutoPay { get; set; }

        public string StripeKey { get; set; }
        public string PlaidKey { get; set; }
        public string PlaidEnvironment { get; set; }
        public string CheckPaymentNote { get; set; }

        public string PromoCode { get; set; }
        public double PromoAmount { get; set; }
        public int PromoID { get; set; }

        public string Message { get; set; }

        public bool CardHasError { get; set; }
        public string CardErrorMessage { get; set; }
    }

    public class InvoiceWithTotal
    {
        public int InvoiceID { get; set; }
        public decimal InvoiceTotal { get; set; }
        public bool InvoiceEarlyRenew { get; set; }
        public InvoiceWithTotal(int invoiceID, decimal invoiceTotal, string notes, DateTime due, bool earlyRenewal)
        {
            InvoiceID = invoiceID;
            InvoiceTotal = invoiceTotal;
            InvoiceEarlyRenew = earlyRenewal;
        }
    }
}