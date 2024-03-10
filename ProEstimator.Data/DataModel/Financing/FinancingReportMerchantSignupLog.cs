using System;
using System.Data;

namespace ProEstimatorData.DataModel.Financing
{
    public class FinancingReportMerchantSignupLog
    {
        public DateTime? CreatedDate { get; set; }
        public DateTime? CallbackDate { get; set; }
        public string SignupStatus { get; set; }
        public string Reasons { get; set; }
        public string ExternalID { get; set; }
        public string EventType { get; set; }
        public string OnboardingType { get; set; }

        public FinancingReportMerchantSignupLog(DataRow row)
        {
            if (!string.IsNullOrEmpty(row["CreatedDate"]?.ToString()))
                CreatedDate = InputHelper.GetDateTime(row["CreatedDate"].ToString()).ToLocalTime();
            if (!string.IsNullOrEmpty(row["CallbackDate"]?.ToString()))
                CallbackDate = InputHelper.GetDateTime(row["CallbackDate"].ToString());

            SignupStatus = InputHelper.GetString(row["Status"].ToString());
            Reasons = InputHelper.GetString(row["Reasons"].ToString());
            ExternalID = InputHelper.GetString(row["ExternalID"].ToString());
            EventType = InputHelper.GetString(row["EventType"].ToString());
            OnboardingType = InputHelper.GetString(row["OnboardingType"].ToString());
        }
    }
}