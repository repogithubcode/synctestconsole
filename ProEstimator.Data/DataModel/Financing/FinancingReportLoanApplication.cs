using System;
using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    public class FinancingReportLoanApplication
    {
        public string MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string MerchantNameFormatted { get; set; }
        public string LoginID { get; set; }
        public string EstimateID { get; set; }
        public string CustomerName { get; set; }
        public string CustomerNameFormatted { get; set; }
        public string TransactionID { get; set; }
        public string EstimateGrandTotal { get; set; }
        public string MaximumAmount { get; set; }
        public string RequestedLoanAmount { get; set; }
        public string ApprovedLoanAmount { get; set; }
        public string LoanApplicationLink { get; set; }
        public string LoanApplicationStatus { get; set; }
        public DateTime? ApplicationStartDate { get; set; }
        public DateTime? DateOfLastUpdate { get; set; }
        public DateTime? ExpirationDate { get; set; }

        public FinancingReportLoanApplication(DataRow row)
        {
            MerchantID = InputHelper.GetString(row["MerchantID"].ToString());
            MerchantName = InputHelper.GetString(row["MerchantName"].ToString());
            MerchantNameFormatted = MerchantName?.Replace("'", "`");
            LoginID = InputHelper.GetString(row["LoginID"].ToString());
            EstimateID = InputHelper.GetString(row["EstimateID"].ToString());
            CustomerName = row["CustomerName"].ToString();
            CustomerNameFormatted = CustomerName?.Replace("'", "`");
            TransactionID = row["TransactionID"].ToString();
            EstimateGrandTotal = row["EstimateGrandTotal"].ToString();
            MaximumAmount = row["MaximumAmount"].ToString();
            RequestedLoanAmount = row["RequestedLoanAmount"].ToString();
            ApprovedLoanAmount = row["ApprovedLoanAmount"].ToString();
            LoanApplicationLink = row["LoanApplicationLink"].ToString();
            LoanApplicationStatus = row["LoanApplicationStatus"].ToString();
            if (!string.IsNullOrEmpty(row["ApplicationStartDate"]?.ToString()))
                ApplicationStartDate = InputHelper.GetDateTime(row["ApplicationStartDate"].ToString());
            if (!string.IsNullOrEmpty(row["DateOfLastUpdate"]?.ToString()))
                DateOfLastUpdate = InputHelper.GetDateTime(row["DateOfLastUpdate"].ToString()).ToLocalTime();
            if (!string.IsNullOrEmpty(row["ExpirationDate"]?.ToString()))
                ExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString());
        }
    }
}