namespace ProEstimator.Business.Model.Financing
{
    // Data returned from Wisetack API when initiating a loan application
    //  NOTE: Message is only populated if the request fails (likely due to merchant not yet approved)
    public class WisetackLoanApplicationResponse
    {
        public string TransactionID { get; set; }
        public string Status { get; set; }
        public string InitToken { get; set; }
        public string MerchantName { get; set; }
        public string TransactionAmount { get; set; }
        public string customerFirstName { get; set; }
        public string CustomerLastName { get; set; }
        public string PaymentLink { get; set; }
        public string Message { get; set; }
    }
}