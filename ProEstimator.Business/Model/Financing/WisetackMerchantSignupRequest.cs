namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Wisetack to initiate a new signup
    public class WisetackMerchantSignupRequest
    {
        public string CallbackURL { get; set; }
        public string SignupPurpose { get; set; }
        public WisetackMerchantSignupRequestInitiator Initiator { get; set; }
        public WisetackMerchantSignupRequestBusiness Business { get; set; }
    }
    public class WisetackMerchantSignupRequestInitiator
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string MobileNumber { get; set; }
    }
    public class WisetackMerchantSignupRequestBusiness
    {
        public string BusinessLegalName { get; set; }
        public string DoingBusinessAs { get; set; }
        public string BusinessAddress { get; set; }
        public string AddressSecondaryNumber { get; set; }
        public string Email { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string Zip { get; set; }
        public string PhoneNumber { get; set; }
        public string FederalEIN { get; set; }
        public int TtmSales { get; set; }
        public int TtmSalesUnits { get; set; }
        public int TtmDisputes { get; set; }
        public int TtmDisputesUnits { get; set; }
        public string EnrollmentDate { get; set; }
        public string CustomerTier { get; set; }
        public string BusinessWebsite { get; set; }
        public string Industry { get; set; }
        public string ExternalId { get; set; }
    }
}
