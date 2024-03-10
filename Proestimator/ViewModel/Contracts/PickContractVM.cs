using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData.Models;

namespace Proestimator.ViewModel.Contracts
{
    public class PickContractVM
    {
        public int LoginID { get; set; }
        public bool CanPickContract { get; set; }
        public int SelectedPaymentID { get; set; }
        public string ErrorMessage { get; set; }
        public bool InProgressContractExists { get; set; }
        public string InProgressContractSummary { get; set; }
        public int InProgressContractID { get; set; }
        public bool IsRenewal { get; set; }
        public string RenewalMessage { get; set; }
        public string RecentExpirationMessage { get; set; }
    }
    
}