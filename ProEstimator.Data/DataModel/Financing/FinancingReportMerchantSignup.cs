using System;
using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    public class FinancingReportMerchantSignup
    {
        public string MerchantID { get; set; }
        public string MerchantName { get; set; }
        public string MerchantNameFormatted { get; set; }
        public string LoginID { get; set; }
        public string SignupLink { get; set; }
        public string SignupStatus { get; set; }
        public DateTime? LastCallbackDate { get; set; }
        public int LoanAppCount { get; set; }

        public FinancingReportMerchantSignup(DataRow row)
        {
            MerchantID = InputHelper.GetString(row["MerchantID"].ToString());
            MerchantName = InputHelper.GetString(row["MerchantName"].ToString());
            MerchantNameFormatted = MerchantName?.Replace("'", "`");
            LoginID = InputHelper.GetString(row["LoginID"].ToString());
            SignupLink = InputHelper.GetString(row["SignupLink"].ToString());
            SignupStatus = InputHelper.GetString(row["SignupStatus"].ToString());
            LoanAppCount = InputHelper.GetInteger(row["LoanAppCount"].ToString());
            if (!string.IsNullOrEmpty(row["LastCallbackDate"]?.ToString()))
                LastCallbackDate = InputHelper.GetDateTime(row["LastCallbackDate"].ToString()).ToLocalTime();
        }
    }
}