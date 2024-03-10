using ProEstimatorData;
using System;
using System.Data;

namespace ProEstimator.Business.Model.Financing
{
    // Data passed to Financing Approved page for the list of loan apps associated to the current merchant
    public class WisetackLoanApplicationInfoVM
    {
        public string TransactionID { get; set; }
        public string CustomerName { get; set; }
        public string MaximumAmount { get; set; }
        public string RequestedLoanAmount { get; set; }
        public string ApprovedLoanAmount { get; set; }
        public string LoanApplicationLink { get; set; }
        public string LoanApplicationStatus { get; set; }
        public DateTime? ApplicationStartDate { get; set; }
        public DateTime? DateOfLastUpdate { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string AdminInfoID { get; set; }

        public WisetackLoanApplicationInfoVM(DataRow row)
        {
            TransactionID = row["TransactionID"].ToString();
            CustomerName = row["CustomerName"].ToString();
            MaximumAmount = row["MaximumAmount"].ToString();
            RequestedLoanAmount = row["RequestedLoanAmount"].ToString();
            ApprovedLoanAmount = row["ApprovedLoanAmount"].ToString();
            LoanApplicationLink = row["LoanApplicationLink"].ToString();
            LoanApplicationStatus = row["LoanApplicationStatus"].ToString();
            AdminInfoID = row["AdminInfoID"].ToString();
            if (!string.IsNullOrEmpty(row["ApplicationStartDate"]?.ToString()))
                ApplicationStartDate = InputHelper.GetDateTime(row["ApplicationStartDate"].ToString());
            if (!string.IsNullOrEmpty(row["DateOfLastUpdate"]?.ToString()))
                DateOfLastUpdate = InputHelper.GetDateTime(row["DateOfLastUpdate"].ToString());
            if (!string.IsNullOrEmpty(row["ExpirationDate"]?.ToString()))
                ExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString());
        }
    }
}
