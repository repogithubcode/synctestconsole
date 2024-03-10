namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Wisetack to request promo info
    public class WisetackLoanApplicationPromoRequest
    {
        public string Amount { get; set; }
        public string MerchantId { get; set; }
    }
}
