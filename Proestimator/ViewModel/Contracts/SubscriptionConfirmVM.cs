using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Proestimator.ViewModelMappers.Contracts;
using ProEstimatorData.DataModel.Contracts;

namespace Proestimator.ViewModel.Contracts
{
    public class SubscriptionConfirmVM
    {
        public int UserID { get; set; }
        public int LoginID { get; set; }
        public int ContractID { get; set; }
        public bool HasMainContract { get; set; }
        public string TermDescription { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime ExpirationDate { get; set; }
        public List<InvoiceVM> EstimaticsInvoices { get; set; } = new List<InvoiceVM>();
        public List<AddOnConfirmVM> AddOns { get; set; } = new List<AddOnConfirmVM>();
        public bool HasPromo { get; set; }
        public string CurrentPromoCode { get; set; }
        public decimal CurrentPromoAmount { get; set; }
        public string PromoCode { get; set; }
        public string ErrorMessage { get; set; }
        public bool IsEarlyRenewalPromoCodeApplied { get; set; }

    }
}