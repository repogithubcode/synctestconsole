namespace ProEstimator.Business.Model.Financing
{
    // Data passed back to our web app when the user clicks the Signup button
    public class WisetackMerchantSignupLinkVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string MerchantID { get; set; }
        public string SignupLink { get; set; }
    }
}
