namespace ProEstimator.Business.Model.Financing
{
    // Data returned from Wisetack API when generating a link fpr a new loan application
    //  NOTE: Message is only populated if the request fails (likely due to merchant not yet approved)
    public class WisetackLoanApplicationLinkResponse
    {
        public string TransactionID { get; set; }
        public string PaymentLink { get; set; }
        public string Message { get; set; }
    }
}