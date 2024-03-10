using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Proestimator.ViewModel.Contracts
{
    public class ContractTermsVM
    {
        public string TermDescription { get; set; }
        public decimal PaymentAmount { get; set; }
        public decimal TermTotal { get; set; }
        public decimal DepositAmount { get; set; }
        public int NumberOfPayments { get; set; }
        public bool ForceAutoPay { get; set; }
        public int ContractPriceLevelID { get; set; }
        public string PaymentDescription { get; set; }
        public string Summary { get; set; }
    }

}