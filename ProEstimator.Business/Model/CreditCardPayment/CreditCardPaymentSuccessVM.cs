namespace ProEstimator.Business.Model.CreditCardPayment
{
    // Data returned from Intellipay's Lightbox CC payment modal when successful
    public class CreditCardPaymentSuccessVM
    {
        public int LoginID { get; set; }
        public int EstimateID { get; set; }
        public int Status { get; set; }
        public int CustID { get; set; }
        public int PaymentID { get; set; }
        public string Response { get; set; }
        public string AuthCode { get; set; }
        public string DeclineReason { get; set; }
        public double Fee { get; set; }
        public int Invoice { get; set; }
        public string Account { get; set; }
        public double Amount { get; set; }
        public bool AmountIncludesFee { get; set; }
        public double Total { get; set; }
        public string PaymentType { get; set; }
        public string MethodHint { get; set; }
        public string CardBrand { get; set; }
        public string CardNumDisplay { get; set; }
        public string ReceiptToken { get; set; }
        public string Call { get; set; }
        public string Nonce { get; set; }
        public string Hmac { get; set; }
        public string PaymentReferenceID { get; set; }
        public string CardNum { get; set; }
        public string Email { get; set; }
        public string NameOnCard { get; set; }
        public string CardType { get; set; }
    }
}