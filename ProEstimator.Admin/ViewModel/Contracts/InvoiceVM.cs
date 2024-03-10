using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class InvoiceVM
    {
        public int InvoiceID { get; set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public int FeeInvoiceID { get; set; }
        public int PaymentNumber { get; set; }
        public string InvoiceType { get; set; }
        public decimal InvoiceAmount { get; set; }
        public decimal SalesTax { get; set; }
        public decimal InvoiceTotal { get; set; }
        public string DueDate { get; set; }
        public string Notes { get; set; }
        public bool Paid { get; set; }
        public string DatePaid { get; set; }
        public int PaymentID { get; set; }
        public string Summary { get; set; }
        public bool PastDue { get; set; }
        public bool IsDeleted { get; set; }

        // These are set only when the invoice is shown in the details form, not in the grid
        public List<InvoiceLinkVM> Links { get; set; }
        public int InvoiceLinkID { get; set; }

        public string ErrorMessage { get; set; }

        public InvoiceVM() { }

        public InvoiceVM(Invoice invoice)
        {
            InvoiceID = invoice.ID;
            LoginID = invoice.LoginID;
            ContractID = invoice.ContractID;
            FeeInvoiceID = invoice.FeeInvoiceID;
            PaymentNumber = invoice.PaymentNumber;
            InvoiceType = invoice.InvoiceType.Type;
            InvoiceAmount = invoice.InvoiceAmount;
            SalesTax = invoice.SalesTax;
            InvoiceTotal = invoice.InvoiceTotal;
            DueDate = invoice.DueDate.ToShortDateString();
            Notes = invoice.Notes;
            Paid = invoice.Paid;
            DatePaid = invoice.DatePaid.HasValue ? invoice.DatePaid.Value.ToShortDateString() : "";
            PaymentID = invoice.PaymentID;
            Summary = invoice.Summary;
            PastDue = invoice.DueDate < DateTime.Now.Date;
            IsDeleted = invoice.IsDeleted;

            ErrorMessage = "";
        }

    }

    public class InvoiceLinkVM
    {
        public int ID { get; set; }
        public string Name { get; set; }

        public InvoiceLinkVM() { }

        public InvoiceLinkVM(int id, string name)
        {
            ID = id;
            Name = name;
        }
    }
}