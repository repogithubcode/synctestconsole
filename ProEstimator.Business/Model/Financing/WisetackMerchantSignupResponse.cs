namespace ProEstimator.Business.Model.Financing
{
    // Data returned from Wisetack API when requesting a signup link
    //  NOTE: Message is only populated if the request fails (perhaps due to use of dupe ExternalID)
    public class WisetackMerchantSignupResponse
    {
        public string MerchantID { get; set; }
        public string SignupLink { get; set; }
        public string Message { get; set; }
    }
}
