using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class InvoiceGridVM
    {
        public int InvoiceID { get; set; }
        public string InvoiceType { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal SalesTax { get; set; }
        public decimal InvoiceTotal { get; set; }
        public string DueDate { get; set; }
        public string Notes { get; set; }
        public string DatePaid { get; set; }
        public string Summary { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPastDue { get; set; }

        public InvoiceGridVM() { }
        public int InvoiceLinkID { get; set; }

        public InvoiceGridVM(Invoice invoice)
        {
            InvoiceID = invoice.ID;
            InvoiceType = invoice.InvoiceType.Type;
            InvoiceAmount = invoice.InvoiceAmount;
            SalesTax = invoice.SalesTax;
            InvoiceTotal = invoice.InvoiceTotal;
            DueDate = invoice.DueDate.ToShortDateString();
            Notes = invoice.Notes;
            DatePaid = invoice.DatePaid.HasValue ? invoice.DatePaid.Value.ToShortDateString() : "";
            Summary = invoice.Summary;
            IsDeleted = invoice.IsDeleted;

            IsPastDue = (!invoice.Paid && invoice.DueDate <= DateTime.Now.Date);
            InvoiceLinkID = invoice.AddOnID;
        }
    }
}