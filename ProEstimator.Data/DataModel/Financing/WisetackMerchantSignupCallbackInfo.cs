using System;

namespace ProEstimatorData.DataModel.Financing
{
    public class WisetackMerchantSignupCallbackInfo
    {
        public DateTime Date { get; set; }
        public string MerchantId { get; set; }
        public string SignupLink { get; set; }
        public string Status { get; set; }
        public string EventType { get; set; }
        public string Reasons { get; set; }
        public string ExternalId { get; set; }
        public string OnboardingType { get; set; }
        public bool TransactionsEnabled { get; set; }
        public string TransactionRange { get; set; }
    }
}
