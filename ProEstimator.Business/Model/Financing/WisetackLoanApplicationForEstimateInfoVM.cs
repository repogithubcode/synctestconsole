using ProEstimatorData;
using System;
using System.Data;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to pages where there is a "Send Loan Application" CTA, for a specific user/estimate
    public class WisetackLoanApplicationForEstimateInfoVM
    {
        public bool Success { get; set; }
        public string ErrorMessage { get; set; }
        public string MerchantID { get; set; }
        public string MerchantSignupLink { get; set; }
        public string MerchantSignupStatus { get; set; }
        public string TransactionID { get; set; }
        public string CustomerName { get; set; }
        public string MaximumAmount { get; set; }
        public string RequestedLoanAmount { get; set; }
        public string ApprovedLoanAmount { get; set; }
        public string LoanApplicationLink { get; set; }
        public string LoanApplicationStatus { get; set; }
        public DateTime ApplicationStartDate { get; set; }
        public DateTime DateOfLastUpdate { get; set; }
        public int AdminInfoID { get; set; }
        public string AmountToFinance { get; set; }
        public string EstimatedDateOfCompletion { get; set; }
        public string LoanAppMobileNumber { get; set; }
        public string ExpirationDate { get; set; }
        public string GrandTotal { get; set; }

        public WisetackLoanApplicationForEstimateInfoVM()
        {
        }

        public WisetackLoanApplicationForEstimateInfoVM(DataRow row)
        {
            if (row == null)
                return;

            MerchantID = row["MerchantID"].ToString();
            MerchantSignupLink = row["MerchantSignupLink"].ToString();
            MerchantSignupStatus = row["MerchantSignupStatus"].ToString();
            TransactionID = row["TransactionID"].ToString();
            CustomerName = row["CustomerName"].ToString();
            MaximumAmount = row["MaximumAmount"].ToString();
            RequestedLoanAmount = row["RequestedLoanAmount"].ToString();
            ApprovedLoanAmount = row["ApprovedLoanAmount"].ToString();
            LoanApplicationLink = row["LoanApplicationLink"].ToString();
            LoanApplicationStatus = row["LoanApplicationStatus"].ToString();
            ApplicationStartDate = InputHelper.GetDateTime(row["ApplicationStartDate"].ToString());
            DateOfLastUpdate = InputHelper.GetDateTime(row["DateOfLastUpdate"].ToString());
            AdminInfoID = InputHelper.GetInteger(row["AdminInfoID"].ToString());
            AmountToFinance = row["AmountToFinance"].ToString();
            if (string.IsNullOrEmpty(row["EstimatedDateOfCompletion"]?.ToString()))
                EstimatedDateOfCompletion = "";
            else
                EstimatedDateOfCompletion = InputHelper.GetDateTime(row["EstimatedDateOfCompletion"].ToString()).ToString("M/d/yyyy");
            if (string.IsNullOrEmpty(row["ExpirationDate"]?.ToString()))
                ExpirationDate = "";
            else
                ExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString()).ToString("M/d/yyyy");
            LoanAppMobileNumber = row["LoanAppMobileNumber"].ToString();
        }
    }
}
