using System;

namespace ProEstimatorData.DataModel.Financing
{
    public class WisetackLoanApplicationCallbackInfo
    {
        public string TransactionId { get; set; }
        public DateTime Date { get; set; }
        public string MessageId { get; set; }
        public string ChangedStatus { get; set; }
        public string ActionsRequired { get; set; }
        public string RequestedLoanAmount { get; set; }
        public string ApprovedLoanAmount { get; set; }
        public string SettledLoanAmount { get; set; }
        public string ProcessingFee { get; set; }
        public string MaximumAmount { get; set; }
        public string TransactionPurpose { get; set; }
        public DateTime ServiceCompletedOn { get; set; }
        public DateTime? TilaAcceptedOn { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string EventType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string PrequalId { get; set; }
        public string CustomerId { get; set; }
    }
}
