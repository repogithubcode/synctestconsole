namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Send Estimate page when user clicks the "Include Loan Application Payment Link" button,
    //  inserting the text into the body of the email/text message with the payment link.
    public class WisetackLoanApplicaitonInsertLinkInfoVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionID { get; set; }
        public string PaymentLink { get; set; }
        public string AmountToFinance { get; set; }
    }
}
