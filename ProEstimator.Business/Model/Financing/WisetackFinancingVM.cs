using System;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Financing page on load and on periodic timer-based refresh
    public class WisetackFinancingVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string MerchantID { get; set; }
        public string SignupLink { get; set; }
        public string Status { get; set; }
        public string Reasons { get; set; }
        public DateTime CallbackDate { get; set; }

        public bool IsTrial { get; set; }
    }
}
