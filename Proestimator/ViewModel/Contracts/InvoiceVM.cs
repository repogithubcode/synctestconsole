using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModel.Contracts
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
        public bool ToPayDue { get; set; }
        public bool Deletable { get; set; }
    }
}