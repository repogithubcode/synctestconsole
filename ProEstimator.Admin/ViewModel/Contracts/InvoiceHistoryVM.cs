using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ProEstimator.Admin.ViewModel.Contracts
{
    public class InvoiceHistoryVM
    {
        public int ID { get; set; }
        public int ContractAddOnID { get; set; }
        public int InvoiceID { get; set; }
        public DateTime TimeStamp { get; set; }
        public string Action { get; set; }
        public decimal Amount { get; set; }
        public decimal SalesTax { get; set; }
        public int Quantity { get; set; }
    }
}