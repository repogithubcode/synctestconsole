using System;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Financing - Request Loan App page when user clicks the Send Loan Application button
    //  in response to the api call made to Wisetack to initiate a new loan application for a merchant's
    //  customer
    public class WisetackSendLoanApplicationVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string TransactionID { get; set; }
        public string PaymentLink { get; set; }
    }
}
