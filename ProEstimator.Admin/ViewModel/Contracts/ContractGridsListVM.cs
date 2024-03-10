using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class ContractGridsListVM
    {
        public List<AddOnVM> AddOns { get; set; }
        public List<AddOnTrialVM> AddOnTrials { get; set; }
        public List<InvoiceGridVM> MainInvoices { get; set; }
        public List<InvoiceGridVM> FrameInvoices { get; set; }
        public List<InvoiceGridVM> EmsInvoices { get; set; }
        public List<InvoiceGridVM> MultiUserInvoices { get; set; }
        public List<InvoiceGridVM> QbInvoices { get; set; }
        public List<InvoiceGridVM> AdvisorInvoices { get; set; }
        public List<InvoiceGridVM> ImageInvoices { get; set; }
        public List<InvoiceGridVM> ReportsInvoices { get; set; }
        public List<InvoiceGridVM> BundleInvoices { get; set; }
        public List<InvoiceGridVM> CustomInvoices { get; set; }
    }
}