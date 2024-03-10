using System;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Financing - Request Loan App page
    public class WisetackFinancingRequestLoanAppVM
    {
        public int UserID { get; set; }
        public int EstimateID { get; set; }
        public string MobileNumber { get; set; }
        public string TransactionAmount { get; set; }
        public string CompletionDate { get; set; }
    }
}
