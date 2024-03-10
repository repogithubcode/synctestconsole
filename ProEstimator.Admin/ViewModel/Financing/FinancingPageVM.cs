using System.Web.Mvc;

namespace ProEstimator.Admin.ViewModel.Financing
{
    public class FinancingPageVM
    {
        public SelectList MerchantSignupStatusList { get; set; }
        public string SelectedMerchantSignupStatus { get; set; }
    }
}