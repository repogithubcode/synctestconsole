namespace ProEstimator.Business.Model.Financing
{
    // Data passed back to our web app to display loan app promo info
    public class WisetackLoanApplicationPromoVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string MerchantID { get; set; }
        public string Headline { get; set; }
        public string Tagline { get; set; }
        public string ValueProp1 { get; set; }
        public string ValueProp2 { get; set; }
        public string ValueProp3 { get; set; }
        public string Button { get; set; }
        public string Disclosure { get; set; }
        public string AsLowAsMonthlyPayment { get; set; }
        public string AsLowAsTermLength { get; set; }
        public string AsLowAsApr { get; set; }
        public string AsLowAsMinApr { get; set; }
        public string AsLowAsMaxApr { get; set; }
    }
}
