using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using ProEstimatorData;
using ProEstimatorData.Models;
using ProEstimatorData.DataModel;
using ProEstimatorData.DataModel.Settings;
using ProEstimatorData.DataModel.Contracts;

using Proestimator.ViewModel.Contracts;

namespace Proestimator.ViewModel.Contracts
{
    public class ContractTabVM
    {

        public int LoginID { get; set; }
        public bool HasActiveContract { get; set; }
        public bool HasActiveTrial { get; set; }
        public List<InvoiceVM> DueInvoices { get; set; } = new List<InvoiceVM>();
        public ContractVM CurrentContract { get; set; }
        public string TrialExpirationMessage { get; set; }
        public bool HasInProgressContract { get; set; }
        public ContractVM InProgressContract { get; set; }
        public bool HasFutureContract { get; set; }
        public ContractVM FutureContract { get; set; }
        public bool ShowContractRenewalButton { get; set; }
        public int ContractExpirationDays { get; set; }
        public bool NeedsPayment { get; set; }
        public decimal InvoiceAmount { get; set; }
        public bool HasContractHistory { get; set; }

    }
}