namespace ProEstimator.Business.Model.Financing
{
    // Data passed back from Wisetack in response to a request for promo info
    public class WisetackLoanApplicationPromoResponse
    {
        public string MerchantID { get; set; }
        public string ExternalID { get; set; }
        public WisetackLoanApplicationPromoResponsePromo Promo { get; set; }
    }

    public class WisetackLoanApplicationPromoResponsePromo
    {
        public string Headline { get; set; }
        public string Tagline { get; set; }
        public string ValueProp1 { get; set; }
        public string ValueProp2 { get; set; }
        public string ValueProp3 { get; set; }
        public string Button { get; set; }
        public string Disclosure { get; set; }
        public WisetackLoanApplicationPromoResponsePromoAsLowAs AsLowAs { get; set; }
    }

    public class WisetackLoanApplicationPromoResponsePromoAsLowAs
    {
        public string MonthlyPayment { get; set; }
        public string TermLength { get; set; }
        public string Apr { get; set; }
        public string MinApr { get; set; }
        public string MaxApr { get; set; }
    }
}
