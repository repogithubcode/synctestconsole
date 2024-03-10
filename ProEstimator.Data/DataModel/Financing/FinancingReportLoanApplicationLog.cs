using System;
using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    public class FinancingReportLoanApplicationLog
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? CallbackDate { get; set; }
        public string ChangedStatus { get; set; }
        public string ActionsRequired { get; set; }
        public string RequestedLoanAmount { get; set; }
        public string ApprovedLoanAmount { get; set; }
        public string SettledLoanAmount { get; set; }
        public string ProcessingFee { get; set; }
        public string MaximumAmount { get; set; }
        public string ServiceCompletedOn { get; set; }
        public string TilaAcceptedOn { get; set; }
        public DateTime? LoanCreatedAt { get; set; }
        public string EventType { get; set; }
        public DateTime? ExpirationDate { get; set; }
        public string CustomerID { get; set; }

        public FinancingReportLoanApplicationLog(DataRow row)
        {
            if (!string.IsNullOrEmpty(row["CreatedDate"]?.ToString()))
                CreatedDate = InputHelper.GetDateTime(row["CreatedDate"].ToString()).ToLocalTime();
            if (!string.IsNullOrEmpty(row["CallbackDate"]?.ToString()))
                CallbackDate = InputHelper.GetDateTime(row["CallbackDate"].ToString());
            if (!string.IsNullOrEmpty(row["ExpirationDate"]?.ToString()))
                ExpirationDate = InputHelper.GetDateTime(row["ExpirationDate"].ToString());
            if (!string.IsNullOrEmpty(row["LoanCreatedAt"]?.ToString()))
                LoanCreatedAt = InputHelper.GetDateTime(row["LoanCreatedAt"].ToString());
            ChangedStatus = InputHelper.GetString(row["ChangedStatus"].ToString());
            ActionsRequired = InputHelper.GetString(row["ActionsRequired"].ToString());
            RequestedLoanAmount = InputHelper.GetString(row["RequestedLoanAmount"].ToString());
            ApprovedLoanAmount = InputHelper.GetString(row["ApprovedLoanAmount"].ToString());
            SettledLoanAmount = InputHelper.GetString(row["SettledLoanAmount"].ToString());
            ProcessingFee = InputHelper.GetString(row["ProcessingFee"].ToString());
            MaximumAmount = InputHelper.GetString(row["MaximumAmount"].ToString());
            ServiceCompletedOn = InputHelper.GetString(row["ServiceCompletedOn"].ToString());
            TilaAcceptedOn = InputHelper.GetString(row["TilaAcceptedOn"].ToString());
            EventType = InputHelper.GetString(row["EventType"].ToString());
            CustomerID = InputHelper.GetString(row["CustomerID"].ToString());
        }
    }
}