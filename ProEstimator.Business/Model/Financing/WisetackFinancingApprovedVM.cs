using System;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Financing Approved page on load and on periodic timer-based refresh
    public class WisetackFinancingApprovedVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string MerchantID { get; set; }
        public string SignupLink { get; set; }
        public string Status { get; set; }
        public string Reasons { get; set; }
        public DateTime CallbackDate { get; set; }

        public int UserID { get; set; }
        public bool IsDeclinedButWasPreviouslyApproved { get; set; }
    }
}
