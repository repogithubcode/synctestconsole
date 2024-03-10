namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Wisetack to initiate a loan application
    public class WisetackLoanApplicationRequest
    {
        public string TransactionAmount { get; set; }
        public string ServiceCompletedOn { get; set; }
        public string TransactionPurpose { get; set; }
        public string MobileNumber { get; set; }
        public string PurchaseId { get; set; }
        public string CallbackURL { get; set; }
        public string AppSource { get; set; }
        public bool SettlementDelay { get; set; }
    }
}
