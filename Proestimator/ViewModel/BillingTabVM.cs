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

namespace Proestimator.ViewModel
{
    public class BillingTabVM
    {
        public int LoginID { get; set; }

        public List<ContractVM> Contracts { get; set; }

        public List<InvoiceVM> DueInvoices { get; set; }

        public bool HasActiveWebEstContract { get; set; }

        public string TrialExpirationMessage { get; set; }

        public bool ShowChangeContractButton { get; set; }

        public bool ShowContractRenewalButton { get; set; }
        public int ContractExpirationDays { get; set; }

        public bool NeedsPayment { get; set; }
        public decimal InvoiceAmount { get; set; }

        public bool CurrentContractIsEarlyRenewal { get; set; }
        public DateTime EarlyRenewalStamp { get; set; }

        public bool ShowAutoPay { get; set; }
        public bool HasSavedPaymentInfo { get; set; }
        public string Last4 { get; set; }
        public DateTime CardExpiration { get; set; }
        public bool AllowAutoPay { get; set; }
        public bool AutoPaySelected { get; set; }
        public bool ForceAutoPay { get; set; }

        public string StripeKey { get; set; }

        public string Message { get; set; }
        public string ErrorMessage { get; set; }

        public bool CardHasError { get; set; }
        public string CardErrorMessage { get; set; }

        public List<AutoRenewDetailVM> AutoRenewDetails { get; set; }
        public bool AutoRenewOn { get; set; }

        public BillingTabVM()
        {
            Contracts = new List<ContractVM>();
            DueInvoices = new List<InvoiceVM>();
            ShowAutoPay = true;
            ErrorMessage = "";
            AutoRenewDetails = new List<AutoRenewDetailVM>();
        }

    }

   

    

    public class AutoRenewDetailVM
    {
        public string Status { get; set; }
        public string TimeStamp { get; set; }

        public AutoRenewDetailVM(LoginAutoRenew autoRenew)
        {
            Status = autoRenew.Enabled ? "Enabled" : "Disabled";
            TimeStamp = autoRenew.TimeStamp.ToShortDateString();
        }
    }
}